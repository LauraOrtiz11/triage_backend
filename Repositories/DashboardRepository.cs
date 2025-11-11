using iText.Commons.Actions.Contexts;
using Microsoft.Data.SqlClient;
using System.Data;
using triage_backend.Dtos;
using triage_backend.Utilities;

namespace triage_backend.Repositories
{
    public class DashboardRepository
    {
        private readonly ContextDB _context;

        public DashboardRepository(ContextDB context)
        {
            _context = context;
        }

        //Tiempos promedio
        public List<AvgTimesDto> GetAverageTimes(DashboardFilterDto filter)
        {
            var result = new List<AvgTimesDto>();

            const string query = @"
                SELECT 
                    FORMAT(T.FECHA_REGISTRO, 'HH:00') AS Hour,
                    AVG(DATEDIFF(MINUTE, T.FECHA_REGISTRO, C.FECHA_INICIO_CONSULTA)) AS WaitingTime,
                    AVG(DATEDIFF(MINUTE, C.FECHA_INICIO_CONSULTA, C.FECHA_FIN_CONSULTA)) AS AttentionTime
                FROM TRIAGE T
                INNER JOIN CONSULTA C ON T.ID_TRIAGE = C.ID_TRIAGE
                WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
                AND (@PriorityId IS NULL OR T.ID_PRIORIDAD = @PriorityId)
                AND (@DoctorId IS NULL OR T.ID_MEDICO = @DoctorId)
                AND (@NurseId IS NULL OR T.ID_ENFERMERO = @NurseId)
                GROUP BY FORMAT(T.FECHA_REGISTRO, 'HH:00')
                ORDER BY Hour";

            using var connection = _context.OpenConnection();
            using var command = new SqlCommand(query, (SqlConnection)connection);

            command.Parameters.AddWithValue("@StartDate", filter.StartDate);
            command.Parameters.AddWithValue("@EndDate", filter.EndDate);
            command.Parameters.AddWithValue("@PriorityId", (object?)filter.PriorityId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DoctorId", (object?)filter.DoctorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@NurseId", (object?)filter.NurseId ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new AvgTimesDto
                {
                    Hour = reader["Hour"]?.ToString() ?? "00:00",
                    WaitingTime = reader["WaitingTime"] == DBNull.Value ? 0 : Convert.ToDouble(reader["WaitingTime"]),
                    AttentionTime = reader["AttentionTime"] == DBNull.Value ? 0 : Convert.ToDouble(reader["AttentionTime"])
                });
            }

            return result;
        }

        // Atenciones por semana
        public List<AttentionsDto> GetAttentionsPerWeek(DashboardFilterDto filter)
        {
            var result = new List<AttentionsDto>();

            const string query = @"
                SELECT 
                    DATEPART(YEAR, T.FECHA_REGISTRO) AS Year,
                    DATEPART(WEEK, T.FECHA_REGISTRO) AS WeekNum,
                    COUNT(T.ID_TRIAGE) AS TotalPatients
                FROM TRIAGE T
                INNER JOIN CONSULTA C ON C.ID_TRIAGE = T.ID_TRIAGE
                WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
                AND (@PriorityId IS NULL OR T.ID_PRIORIDAD = @PriorityId)
                AND (@DoctorId IS NULL OR T.ID_MEDICO = @DoctorId)
                AND (@NurseId IS NULL OR T.ID_ENFERMERO = @NurseId)
                GROUP BY DATEPART(YEAR, T.FECHA_REGISTRO), DATEPART(WEEK, T.FECHA_REGISTRO)
                ORDER BY Year, WeekNum";

            using var connection = _context.OpenConnection();
            using var command = new SqlCommand(query, (SqlConnection)connection);

            command.Parameters.AddWithValue("@StartDate", filter.StartDate);
            command.Parameters.AddWithValue("@EndDate", filter.EndDate);
            command.Parameters.AddWithValue("@PriorityId", (object?)filter.PriorityId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DoctorId", (object?)filter.DoctorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@NurseId", (object?)filter.NurseId ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new AttentionsDto
                {
                    Week = $"Week {reader["WeekNum"]} ({reader["Year"]})",
                    TotalPatients = Convert.ToInt32(reader["TotalPatients"])
                });
            }

            return result;
        }

        // Distribución por prioridad
        public List<PriorityDistributionDto> GetPriorityDistribution(DashboardFilterDto filter)
        {
            var result = new List<PriorityDistributionDto>();

            const string query = @"
                WITH Total AS (
                    SELECT COUNT(*) AS TotalCount
                    FROM TRIAGE T
                    INNER JOIN CONSULTA C ON T.ID_TRIAGE = C.ID_TRIAGE
                    WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
                    AND (@PriorityId IS NULL OR T.ID_PRIORIDAD = @PriorityId)
                    AND (@DoctorId IS NULL OR T.ID_MEDICO = @DoctorId)
                    AND (@NurseId IS NULL OR T.ID_ENFERMERO = @NurseId)
                )
                SELECT 
                    ISNULL(P.NOMBRE_PRIO, 'Unclassified') AS PriorityName,
                    COUNT(T.ID_TRIAGE) AS TotalPatients,
                    CAST(COUNT(T.ID_TRIAGE) * 100.0 / (SELECT TotalCount FROM Total) AS DECIMAL(5,2)) AS Percentage
                FROM TRIAGE T
                LEFT JOIN PRIORIDAD P ON T.ID_PRIORIDAD = P.ID_PRIORIDAD
                INNER JOIN CONSULTA C ON T.ID_TRIAGE = C.ID_TRIAGE
                WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
                AND (@PriorityId IS NULL OR T.ID_PRIORIDAD = @PriorityId)
                AND (@DoctorId IS NULL OR T.ID_MEDICO = @DoctorId)
                AND (@NurseId IS NULL OR T.ID_ENFERMERO = @NurseId)
                GROUP BY ISNULL(P.NOMBRE_PRIO, 'Unclassified')
                ORDER BY TotalPatients DESC";

            using var connection = _context.OpenConnection();
            using var command = new SqlCommand(query, (SqlConnection)connection);

            command.Parameters.AddWithValue("@StartDate", filter.StartDate);
            command.Parameters.AddWithValue("@EndDate", filter.EndDate);
            command.Parameters.AddWithValue("@PriorityId", (object?)filter.PriorityId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DoctorId", (object?)filter.DoctorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@NurseId", (object?)filter.NurseId ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new PriorityDistributionDto
                {
                    PriorityName = reader["PriorityName"].ToString(),
                    TotalPatients = Convert.ToInt32(reader["TotalPatients"]),
                    Percentage = Convert.ToDouble(reader["Percentage"])
                });
            }

            return result;
        }

        // Frecuencia de diagnósticos
        public List<DiagnosisFrequencyDto> GetDiagnosisFrequency(DashboardFilterDto filter)
        {
            var result = new List<DiagnosisFrequencyDto>();

            const string query = @"
        WITH Total AS (
            SELECT COUNT(*) AS TotalCount
            FROM CONSULTA_DIAGNOSTICO CD
            INNER JOIN CONSULTA C ON CD.ID_CONSULTA = C.ID_CONSULTA
            INNER JOIN TRIAGE T ON C.ID_TRIAGE = T.ID_TRIAGE
            INNER JOIN DIAGNOSTICO D ON CD.ID_DIAGNOSTICO = D.ID_DIAGNOSTICO
            WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
            AND (@PriorityId IS NULL OR T.ID_PRIORIDAD = @PriorityId)
            AND (@DoctorId IS NULL OR T.ID_MEDICO = @DoctorId)
            AND (@NurseId IS NULL OR T.ID_ENFERMERO = @NurseId)
        )
        SELECT TOP 5 
            D.NOMBRE_DIAG AS DiagnosisName,
            COUNT(D.ID_DIAGNOSTICO) AS TotalOccurrences,
            CASE WHEN (SELECT TotalCount FROM Total) > 0
                 THEN CAST(COUNT(D.ID_DIAGNOSTICO) * 100.0 / (SELECT TotalCount FROM Total) AS DECIMAL(5,2))
                 ELSE 0 END AS Percentage
        FROM CONSULTA_DIAGNOSTICO CD
        INNER JOIN CONSULTA C ON CD.ID_CONSULTA = C.ID_CONSULTA
        INNER JOIN TRIAGE T ON C.ID_TRIAGE = T.ID_TRIAGE
        INNER JOIN DIAGNOSTICO D ON CD.ID_DIAGNOSTICO = D.ID_DIAGNOSTICO
        WHERE T.FECHA_REGISTRO BETWEEN @StartDate AND @EndDate
        AND (@PriorityId IS NULL OR T.ID_PRIORIDAD = @PriorityId)
        AND (@DoctorId IS NULL OR T.ID_MEDICO = @DoctorId)
        AND (@NurseId IS NULL OR T.ID_ENFERMERO = @NurseId)
        GROUP BY D.NOMBRE_DIAG
        ORDER BY TotalOccurrences DESC;";

            using var connection = _context.OpenConnection();
            using var command = new SqlCommand(query, (SqlConnection)connection);

            command.Parameters.AddWithValue("@StartDate", filter.StartDate);
            command.Parameters.AddWithValue("@EndDate", filter.EndDate);
            command.Parameters.AddWithValue("@PriorityId", (object?)filter.PriorityId ?? DBNull.Value);
            command.Parameters.AddWithValue("@DoctorId", (object?)filter.DoctorId ?? DBNull.Value);
            command.Parameters.AddWithValue("@NurseId", (object?)filter.NurseId ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new DiagnosisFrequencyDto
                {
                    DiagnosisName = reader["DiagnosisName"].ToString(),
                    TotalOccurrences = Convert.ToInt32(reader["TotalOccurrences"]),
                    Percentage = Convert.ToDouble(reader["Percentage"])
                });
            }

            return result;
        }


        // Listar enfermeros
        public List<UserBasicDto> GetNurses()
        {
            var result = new List<UserBasicDto>();

            const string query = @"
        SELECT 
            U.ID_USUARIO AS UserId,
            CONCAT(U.NOMBRE_US, ' ', U.APELLIDO_US) AS FullName,
            R.NOMBRE_ROL AS RoleName
        FROM USUARIO U
        INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
        WHERE U.ID_ROL = 2
        ORDER BY U.NOMBRE_US;";

            using var connection = _context.OpenConnection();
            using var command = new SqlCommand(query, (SqlConnection)connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new UserBasicDto
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    FullName = reader["FullName"].ToString() ?? string.Empty,
                    RoleName = reader["RoleName"].ToString() ?? string.Empty
                });
            }

            return result;
        }

        // Listar médicos
        public List<UserBasicDto> GetDoctors()
        {
            var result = new List<UserBasicDto>();

            const string query = @"
        SELECT 
            U.ID_USUARIO AS UserId,
            CONCAT(U.NOMBRE_US, ' ', U.APELLIDO_US) AS FullName,
            R.NOMBRE_ROL AS RoleName
        FROM USUARIO U
        INNER JOIN ROL R ON U.ID_ROL = R.ID_ROL
        WHERE U.ID_ROL = 4
        ORDER BY U.NOMBRE_US;";

            using var connection = _context.OpenConnection();
            using var command = new SqlCommand(query, (SqlConnection)connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new UserBasicDto
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    FullName = reader["FullName"].ToString() ?? string.Empty,
                    RoleName = reader["RoleName"].ToString() ?? string.Empty
                });
            }

            return result;
        }



    }
}