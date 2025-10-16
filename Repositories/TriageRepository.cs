using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using triage_backend.Dtos;
using triage_backend.Interfaces;

namespace triage_backend.Repositories
{
    public class TriageRepository : ITriageRepository
    {
        private readonly string _connectionString;

        public TriageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Repositories/TriageRepository.cs
        public async Task<int> InsertTriageAsync(
    TriageRequestDto request,
    string suggestedLevel,
    int ID_Patient,
    int ID_Doctor,
    int ID_Nurse,
    int ID_Priority,
    int ID_State,
    int PatientAge)
        {
            const string query = @"
        INSERT INTO TRIAGE (
            ID_PACIENTE, ID_MEDICO, ID_PRIORIDAD, ID_ESTADO, FECHA_REGISTRO,
            SINTOMAS, TEMPERATURA, FRECUENCIA_CARD, FRECUENCIA_RES,
            PRESION_ARTERIAL, OXIGENACION, ID_ENFERMERO, EDAD_PACIENTE
        )
        VALUES (
            @ID_PACIENTE, @ID_MEDICO, @ID_PRIORIDAD, @ID_ESTADO, GETDATE(),
            @SINTOMAS, @TEMPERATURA, @FRECUENCIA_CARD, @FRECUENCIA_RES,
            @PRESION_ARTERIAL, @OXIGENACION, @ID_ENFERMERO, @EDAD_PACIENTE
        );

        SELECT SCOPE_IDENTITY();
    ";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID_PACIENTE", ID_Patient);
            command.Parameters.AddWithValue("@ID_MEDICO", (object?)ID_Doctor ?? DBNull.Value);
            command.Parameters.AddWithValue("@ID_PRIORIDAD", ID_Priority);
            command.Parameters.AddWithValue("@ID_ESTADO", ID_State);
            command.Parameters.AddWithValue("@SINTOMAS", (object?)request.Symptoms ?? DBNull.Value);
            command.Parameters.AddWithValue("@TEMPERATURA", request.VitalSigns.Temperature);
            command.Parameters.AddWithValue("@FRECUENCIA_CARD", request.VitalSigns.HeartRate);
            command.Parameters.AddWithValue("@FRECUENCIA_RES", request.VitalSigns.RespiratoryRate);
            command.Parameters.AddWithValue("@PRESION_ARTERIAL", (object?)request.VitalSigns.BloodPressure ?? DBNull.Value);
            command.Parameters.AddWithValue("@OXIGENACION", request.VitalSigns.OxygenSaturation);
            command.Parameters.AddWithValue("@ID_ENFERMERO", ID_Nurse);
            command.Parameters.AddWithValue("@EDAD_PACIENTE", PatientAge);


            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }


    }
}
