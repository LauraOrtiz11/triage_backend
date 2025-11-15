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
        /// Obtiene la información detallada de un triage específico (usando SP_GetTriageDetailsById).
        /// </summary>
        public TriageFullInfoDto.TriageDetailsDto? GetTriageDetailsById(int triageId)
        {
            TriageFullInfoDto.TriageDetailsDto? dto = null;

            using (var conn = _context.OpenConnection())
            using (var cmd = new SqlCommand("SP_GetTriageDetailsById", (SqlConnection)conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
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
        /// Obtiene el historial médico completo de un paciente, incluyendo consultas, diagnósticos y tratamientos (usando SP_GetPatientHistory).
        /// </summary>
        public List<PatientHistoryDto> GetPatientHistory(int patientId)
        {
            var list = new List<PatientHistoryDto>();

            using (var conn = _context.OpenConnection())
            using (var cmd = new SqlCommand("SP_GetPatientHistory", (SqlConnection)conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
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
