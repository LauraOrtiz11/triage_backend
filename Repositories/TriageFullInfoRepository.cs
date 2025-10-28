using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using triage_backend.Dtos;
using triage_backend.Utilities;
using static triage_backend.Dtos.TriageFullInfoDto;

namespace triage_backend.Repositories
{
    public class TriageFullInfoRepository
    {
        private readonly ContextDB _context;

        public TriageFullInfoRepository(ContextDB context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la información detallada de un triage específico.
        /// </summary>
        public TriageFullInfoDto.TriageDetailsDto? GetTriageDetailsById(int triageId)
        {
            TriageFullInfoDto.TriageDetailsDto? dto = null;

            using (var conn = _context.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    WITH UltimaPrioridad AS (
                        SELECT 
                            tr.ID_Triage, 
                            tr.ID_Prioridad, 
                            tr.ID_Usuario AS ID_MedicoTriage,
                            tr.Fecha_Registro,
                            ROW_NUMBER() OVER(PARTITION BY tr.ID_Triage ORDER BY tr.Fecha_Registro DESC) AS rn
                        FROM TRIAGE_RESULTADO tr
                    )
                    SELECT 
                        paciente.ID_USUARIO AS PacienteID,
                        paciente.NOMBRE_US AS NombrePaciente,
                        paciente.CEDULA_US AS CedulaPaciente,
                        CASE 
                            WHEN paciente.Sexo_Us = 1 THEN 'Masculino'
                            WHEN paciente.Sexo_Us = 0 THEN 'Femenino'
                            ELSE 'No especificado'
                        END AS Sexo,
                        paciente.FECHA_NAC_US AS FechaNacimiento,
                        t.ID_TRIAGE,
                        t.SINTOMAS,
                        t.TEMPERATURA AS Temp,
                        t.PRESION_ARTERIAL AS Presion,
                        t.FRECUENCIA_CARD AS FrecuenciaCardiaca,
                        t.FRECUENCIA_RES AS FrecuenciaRespiratoria,
                        t.OXIGENACION AS SaturacionO2,
                        pr.NOMBRE_PRIO AS UltimaPrioridad,
                        medico.NOMBRE_US AS MedicoAsignado,
                        CONVERT(VARCHAR(5), t.FECHA_REGISTRO, 108) AS HoraTriage
                    FROM TRIAGE t
                    INNER JOIN USUARIO paciente ON paciente.ID_USUARIO = t.ID_PACIENTE
                    LEFT JOIN UltimaPrioridad up ON up.ID_Triage = t.ID_TRIAGE AND up.rn = 1
                    LEFT JOIN PRIORIDAD pr ON pr.ID_PRIORIDAD = up.ID_Prioridad
                    LEFT JOIN USUARIO medico ON medico.ID_USUARIO = up.ID_MedicoTriage
                    WHERE t.ID_TRIAGE = @TriageID;";

                cmd.Parameters.Add(new SqlParameter("@TriageID", SqlDbType.Int) { Value = triageId });

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dto = new TriageFullInfoDto.TriageDetailsDto
                        {
                            PatientId = reader["PacienteID"] is DBNull ? 0 : Convert.ToInt32(reader["PacienteID"]),
                            PatientName = reader["NombrePaciente"]?.ToString() ?? "Sin nombre",
                            PatientDocument = reader["CedulaPaciente"]?.ToString() ?? "Sin cédula",
                            Gender = reader["Sexo"]?.ToString() ?? "No especificado",
                            BirthDate = reader["FechaNacimiento"] is DBNull ? null : Convert.ToDateTime(reader["FechaNacimiento"]),
                            TriageId = reader["ID_TRIAGE"] is DBNull ? 0 : Convert.ToInt32(reader["ID_TRIAGE"]),
                            Symptoms = reader["SINTOMAS"]?.ToString() ?? "No especificado",
                            Temperature = reader["Temp"] is DBNull ? null : Convert.ToDecimal(reader["Temp"]),
                            BloodPressure = reader["Presion"]?.ToString() ?? "No registrada",
                            HeartRate = reader["FrecuenciaCardiaca"] is DBNull ? null : Convert.ToInt32(reader["FrecuenciaCardiaca"]),
                            RespiratoryRate = reader["FrecuenciaRespiratoria"] is DBNull ? null : Convert.ToInt32(reader["FrecuenciaRespiratoria"]),
                            OxygenSaturation = reader["SaturacionO2"] is DBNull ? null : Convert.ToDecimal(reader["SaturacionO2"]),
                            LastPriority = reader["UltimaPrioridad"]?.ToString() ?? "Sin prioridad",
                            AssignedDoctor = reader["MedicoAsignado"]?.ToString() ?? "Sin asignar",
                            TriageTime = reader["HoraTriage"]?.ToString() ?? ""
                        };
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Obtiene el historial médico completo de un paciente, incluyendo consultas, diagnósticos y tratamientos.
        /// </summary>
        public List<PatientHistoryDto> GetPatientHistory(int patientId)
        {
            var list = new List<PatientHistoryDto>();

            using (var conn = _context.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
WITH DatosPrincipales AS (
    SELECT 
        c.ID_CONSULTA,
        h.ID_HISTORIAL,
        CONVERT(VARCHAR(5), c.FECHA_INICIO_CONSULTA, 108) AS HoraInicioConsulta,
        CONVERT(VARCHAR(5), c.FECHA_FIN_CONSULTA, 108) AS HoraFinConsulta,
        c.ID_ESTADO
    FROM CONSULTA c
    INNER JOIN HISTORIAL h ON h.ID_HISTORIAL = c.ID_HISTORIAL
    WHERE h.ID_PACIENTE = @PacienteID
),
Detalle AS (
    SELECT 
        dp.ID_CONSULTA,
        dp.HoraInicioConsulta,
        dp.HoraFinConsulta,
        dp.ID_ESTADO,
        diag.ID_DIAGNOSTICO,
        diag.NOMBRE_DIAG,
        diag.OBSERV_DIAG,
        trat.ID_TRATAMIENTO,
        trat.DESCRIP_TRATA,
        exam.ID_EXAMEN,
        exam.NOMBRE_EXAM,
        exam.DESCRIP_EXAM,
        med.ID_MEDICAMENTO,
        med.NOMBRE_MEDICA,
        med.DESCRIP_MEDICA,
        med.PROVEEDOR_MEDICA,
        ROW_NUMBER() OVER(
            PARTITION BY dp.ID_CONSULTA 
            ORDER BY diag.ID_DIAGNOSTICO, trat.ID_TRATAMIENTO
        ) AS rnFila
    FROM DatosPrincipales dp
    LEFT JOIN CONSULTA_DIAGNOSTICO cd ON cd.ID_CONSULTA = dp.ID_CONSULTA
    LEFT JOIN DIAGNOSTICO diag ON diag.ID_DIAGNOSTICO = cd.ID_DIAGNOSTICO
    LEFT JOIN TRATAMIENTO trat ON trat.ID_CONSULTA = dp.ID_CONSULTA
    LEFT JOIN TRATAMIENTO_EXAMEN te ON te.ID_TRATAMIENTO = trat.ID_TRATAMIENTO
    LEFT JOIN EXAMEN exam ON exam.ID_EXAMEN = te.ID_EXAMEN
    LEFT JOIN TRATAMIENTO_MEDICAMENTO tm ON tm.ID_TRATAMIENTO = trat.ID_TRATAMIENTO
    LEFT JOIN MEDICAMENTO med ON med.ID_MEDICAMENTO = tm.ID_MEDICAMENTO
)
SELECT
    CASE WHEN rnFila = 1 THEN ID_CONSULTA ELSE NULL END AS ID_CONSULTA,
    CASE WHEN rnFila = 1 THEN HoraInicioConsulta ELSE NULL END AS HoraInicioConsulta,
    CASE WHEN rnFila = 1 THEN HoraFinConsulta ELSE NULL END AS HoraFinConsulta,
    CASE WHEN rnFila = 1 THEN ID_ESTADO ELSE NULL END AS ID_ESTADO,
    ID_DIAGNOSTICO,
    NOMBRE_DIAG,
    OBSERV_DIAG,
    ID_TRATAMIENTO,
    DESCRIP_TRATA,
    ID_EXAMEN,
    NOMBRE_EXAM,
    DESCRIP_EXAM,
    ID_MEDICAMENTO,
    NOMBRE_MEDICA,
    DESCRIP_MEDICA,
    PROVEEDOR_MEDICA
FROM Detalle
ORDER BY ID_CONSULTA, rnFila;
";

                cmd.Parameters.Add(new SqlParameter("@PacienteID", SqlDbType.Int) { Value = patientId });

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new PatientHistoryDto
                        {
                            ConsultationId = reader["ID_CONSULTA"] is DBNull ? null : Convert.ToInt32(reader["ID_CONSULTA"]),
                            StartTime = reader["HoraInicioConsulta"]?.ToString() ?? "No aplica",
                            EndTime = reader["HoraFinConsulta"]?.ToString() ?? "No aplica",
                            StatusId = reader["ID_ESTADO"] is DBNull ? null : Convert.ToInt32(reader["ID_ESTADO"]),

                            DiagnosisId = reader["ID_DIAGNOSTICO"] is DBNull ? null : Convert.ToInt32(reader["ID_DIAGNOSTICO"]),
                            DiagnosisName = reader["NOMBRE_DIAG"]?.ToString() ?? "No aplica",
                            DiagnosisObservation = reader["OBSERV_DIAG"]?.ToString() ?? "No aplica",

                            TreatmentId = reader["ID_TRATAMIENTO"] is DBNull ? null : Convert.ToInt32(reader["ID_TRATAMIENTO"]),
                            TreatmentDescription = reader["DESCRIP_TRATA"]?.ToString() ?? "No aplica",

                            ExamId = reader["ID_EXAMEN"] is DBNull ? null : Convert.ToInt32(reader["ID_EXAMEN"]),
                            ExamName = reader["NOMBRE_EXAM"]?.ToString() ?? "No aplica",
                            ExamDescription = reader["DESCRIP_EXAM"]?.ToString() ?? "No aplica",

                            MedicationId = reader["ID_MEDICAMENTO"] is DBNull ? null : Convert.ToInt32(reader["ID_MEDICAMENTO"]),
                            MedicationName = reader["NOMBRE_MEDICA"]?.ToString() ?? "No aplica",
                            MedicationDescription = reader["DESCRIP_MEDICA"]?.ToString() ?? "No aplica",
                            MedicationProvider = reader["PROVEEDOR_MEDICA"]?.ToString() ?? "No aplica"
                        };

                        list.Add(item);
                    }
                }
            }

            return list;
        }
    }
}
