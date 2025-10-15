using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    /// <summary>
    /// Repository responsible for retrieving triage patient information from the database.
    /// </summary>
    public class TriagePatientRepository
    {
        private readonly ContextDB _context;

        public TriagePatientRepository(ContextDB context)
        {
            _context = context;
        }

        /// <summary>
        /// Recupera una lista de pacientes clasificados, opcionalmente filtrados por color de prioridad.
        /// </summary>
        /// <param name="color">Optional color filter (e.g., "rojo", "verde", etc.).</param>
        /// <returns>List of TriagePatientDto with patient and triage details.</returns>
        public List<TriagePatientDto> GetTriagePatients(string? color)
        {
            var patients = new List<TriagePatientDto>();

            using (var connection = _context.OpenConnection())
            {
                var query = @"
                    SELECT 
                        T.ID_TRIAGE, 
                        U.ID_Usuario AS PatientId,                  
                        U.Cedula_Us AS Identification,              
                        (U.Nombre_Us + ' ' + U.Apellido_Us) AS FullName,                 
                        CASE 
                            WHEN U.Sexo_Us = 1 THEN 'Masculino'
                            WHEN U.Sexo_Us = 0 THEN 'Femenino'
                            ELSE 'No especificado'
                        END AS Gender,                      
                        DATEDIFF(YEAR, U.Fecha_Nac_Us, GETDATE()) AS Age, 
                        T.ID_Triage AS TriageId,                    
                        T.Fecha_Registro AS RegistrationDate,       
                        T.Sintomas AS Symptoms,                     
                        T.Temperatura AS Temperature,               
                        T.Frecuencia_Card AS HeartRate,             
                        T.Presion_Arterial AS BloodPressure,        
                        T.Frecuencia_Res AS RespiratoryRate,        
                        T.Oxigenacion AS OxygenSaturation,          
                        P.Nombre_Prio AS PriorityName,              
                        P.Color_Prio AS PriorityColor,              
                        M.Nombre_Us AS AssignedDoctorName           
                    FROM TRIAGE AS T
                        INNER JOIN USUARIO AS U ON U.ID_Usuario = T.ID_Paciente
                        INNER JOIN PRIORIDAD AS P ON P.ID_Prioridad = T.ID_Prioridad
                        LEFT JOIN USUARIO AS M ON M.ID_Usuario = T.ID_Medico
                    WHERE 
                         T.ID_Estado = 1 
                        AND (@Color IS NULL OR P.Color_Prio = @Color)
                    ORDER BY 
                        T.Fecha_Registro DESC,
                        CASE 
                            WHEN P.Color_Prio = 'rojo' THEN 1
                            WHEN P.Color_Prio = 'naranja' THEN 2
                            WHEN P.Color_Prio = 'amarillo' THEN 3
                            WHEN P.Color_Prio = 'verde' THEN 4
                            WHEN P.Color_Prio = 'azul' THEN 5
                            ELSE 6
                        END;";

                using (var command = new SqlCommand(query, (SqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@Color", (object?)color ?? DBNull.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dto = new TriagePatientDto
                            {
                                TriageId = reader["ID_TRIAGE"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ID_TRIAGE"]),
                                PatientId = Convert.ToInt64(reader["PatientId"]),
                                Identification = reader["Identification"] as string ?? string.Empty,
                                FullName = reader["FullName"] as string ?? string.Empty,
                                Gender = reader["Gender"] as string ?? string.Empty,
                                Age = Convert.ToInt32(reader["Age"]),

                                Symptoms = reader["Symptoms"] as string ?? string.Empty,
                                Temperature = reader["Temperature"] != DBNull.Value ? Convert.ToDecimal(reader["Temperature"]) : 0,
                                HeartRate = reader["HeartRate"] != DBNull.Value ? Convert.ToInt32(reader["HeartRate"]) : 0,
                                BloodPressure = reader["BloodPressure"] as string ?? string.Empty,
                                RespiratoryRate = reader["RespiratoryRate"] != DBNull.Value ? Convert.ToInt32(reader["RespiratoryRate"]) : 0,
                                OxygenSaturation = reader["OxygenSaturation"] != DBNull.Value ? Convert.ToInt32(reader["OxygenSaturation"]) : 0,
                                PriorityName = reader["PriorityName"] as string ?? string.Empty,

                            };

                            patients.Add(dto);
                        }
                    }
                }
            }

            _context.CloseConnection();
            return patients;
        }
    }
}
