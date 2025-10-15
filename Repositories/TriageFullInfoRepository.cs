using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using triage_backend.Dtos;
using triage_backend.Utilities;

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
        /// Gets detailed information about a specific triage.
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
                            WHEN paciente.SEXO_US = 1 THEN 'Masculino'
                            WHEN paciente.SEXO_US = 0 THEN 'Femenino'
                            ELSE 'No especificado'
                        END AS Sexo,
                        paciente.FECHA_NAC_US AS FechaNacimiento,
                        t.ID_Triage,
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
                    LEFT JOIN UltimaPrioridad up ON up.ID_Triage = t.ID_Triage AND up.rn = 1
                    LEFT JOIN PRIORIDAD pr ON pr.ID_PRIORIDAD = up.ID_Prioridad
                    LEFT JOIN USUARIO medico ON medico.ID_USUARIO = up.ID_MedicoTriage
                    WHERE t.ID_Triage = @TriageID;";

                var param = new SqlParameter("@TriageID", SqlDbType.Int) { Value = triageId };
                cmd.Parameters.Add(param);

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
                            TriageId = reader["ID_Triage"] is DBNull ? 0 : Convert.ToInt32(reader["ID_Triage"]),
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
        /// Gets the full medical history of a patient, including diagnoses and treatments.
        /// </summary>
        public List<TriageFullInfoDto.PatientHistoryDto> GetPatientHistory(int patientId)
        {
            var list = new List<TriageFullInfoDto.PatientHistoryDto>();

            using (var conn = _context.OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    WITH DatosPrincipales AS (
                        SELECT 
                            c.ID_CONSULTA,
                            c.ID_HISTORIAL,
                            CONVERT(VARCHAR(5), c.FECHA_INICIO_CONSULTA, 108) AS HoraInicioConsulta,
                            CONVERT(VARCHAR(5), c.FECHA_FIN_CONSULTA, 108) AS HoraFinConsulta,
                            c.ID_ESTADO
                        FROM CONSULTA c
                        WHERE c.ID_HISTORIAL IN (
                            SELECT ID_HISTORIAL FROM HISTORIAL WHERE ID_PACIENTE = @PacienteID
                        )
                    ),
                    Detalle AS (
                        SELECT 
                            dp.*,
                            diag.ID_Diagnostico,
                            diag.NOMBRE_DIAG,
                            diag.OBSERV_DIAG,
                            trat.ID_Tratamiento,
                            trat.Descrip_Trata,
                            trat.Observ_Trata,
                            trat.Dosis_Recet,
                            ROW_NUMBER() OVER(PARTITION BY dp.ID_CONSULTA ORDER BY diag.ID_Diagnostico, trat.ID_Tratamiento) AS rnFila
                        FROM DatosPrincipales dp
                        LEFT JOIN HISTORIAL_DIAGNOSTICO hd ON hd.ID_HISTORIAL = dp.ID_HISTORIAL
                        LEFT JOIN DIAGNOSTICO diag ON diag.ID_Diagnostico = hd.ID_Diagnostico
                        LEFT JOIN DIAGNOSTICO_TRATAMIENTO dt ON dt.ID_Diagnostico = diag.ID_Diagnostico
                        LEFT JOIN TRATAMIENTO trat ON trat.ID_Tratamiento = dt.ID_Tratamiento
                    )
                    SELECT
                        CASE WHEN rnFila = 1 THEN ID_CONSULTA ELSE NULL END AS ID_CONSULTA,
                        CASE WHEN rnFila = 1 THEN HoraInicioConsulta ELSE NULL END AS HoraInicioConsulta,
                        CASE WHEN rnFila = 1 THEN HoraFinConsulta ELSE NULL END AS HoraFinConsulta,
                        CASE WHEN rnFila = 1 THEN ID_ESTADO ELSE NULL END AS ID_ESTADO,
                        ID_Diagnostico,
                        NOMBRE_DIAG,
                        OBSERV_DIAG,
                        ID_Tratamiento,
                        Descrip_Trata,
                        Observ_Trata,
                        Dosis_Recet
                    FROM Detalle
                    WHERE ID_Diagnostico IS NOT NULL OR ID_Tratamiento IS NOT NULL
                    ORDER BY ID_CONSULTA, rnFila;";

                cmd.Parameters.Add(new SqlParameter("@PacienteID", SqlDbType.Int) { Value = patientId });

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new TriageFullInfoDto.PatientHistoryDto
                        {
                            ConsultationId = reader["ID_CONSULTA"] is DBNull ? null : Convert.ToInt32(reader["ID_CONSULTA"]),
                            StartTime = reader["HoraInicioConsulta"]?.ToString(),
                            EndTime = reader["HoraFinConsulta"]?.ToString(),
                            StatusId = reader["ID_ESTADO"] is DBNull ? null : Convert.ToInt32(reader["ID_ESTADO"]),
                            DiagnosisId = reader["ID_Diagnostico"] is DBNull ? null : Convert.ToInt32(reader["ID_Diagnostico"]),
                            DiagnosisName = reader["NOMBRE_DIAG"]?.ToString(),
                            DiagnosisObservation = reader["OBSERV_DIAG"]?.ToString(),
                            TreatmentId = reader["ID_Tratamiento"] is DBNull ? null : Convert.ToInt32(reader["ID_Tratamiento"]),
                            TreatmentDescription = reader["Descrip_Trata"]?.ToString(),
                            TreatmentObservation = reader["Observ_Trata"]?.ToString(),
                            TreatmentDose = reader["Dosis_Recet"]?.ToString()
                        };

                        list.Add(item);
                    }
                }
            }

            return list;
        }
    }
}
