using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using triage_backend.Utilities;
using TriageBackend.DTOs;

namespace TriageBackend.Repositories
{
    /// <summary>
    /// Implementación del repositorio para obtener el historial (consulta + diagnóstico más reciente).
    /// Nombre: HistoryReportRepository
    /// </summary>
    public class HistoryReportRepository : IHistoryRepository
    {
        private readonly ContextDB _context;

        public HistoryReportRepository(ContextDB context)
        {
            _context = context;
        }

        // SQL base: trae diagnóstico más reciente por consulta + nombre del diagnóstico + observación (desde DIAGNOSTICO),
        // además trae doctor full name y usa OUTER APPLY para traer tratamiento (último) y agregar meds/exams.
        private const string BaseCteSql = @"
;WITH cd_ranked AS (
    SELECT cd.ID_CD,
           cd.ID_CONSULTA,
           cd.ID_DIAGNOSTICO,
           ROW_NUMBER() OVER (PARTITION BY cd.ID_CONSULTA ORDER BY cd.ID_CD DESC) AS rn
    FROM CONSULTA_DIAGNOSTICO cd
)
SELECT 
    c.ID_CONSULTA,
    c.ID_HISTORIAL,
    c.ID_MEDICO,
    c.ID_TRIAGE,
    c.ID_ESTADO,
    c.FECHA_INICIO_CONSULTA,
    c.FECHA_FIN_CONSULTA,
    cd.ID_CD AS DiagnosisRowId,
    cd.ID_DIAGNOSTICO AS DiagnosisId,
    dia.NOMBRE_DIAG AS DiagnosisName,
    dia.OBSERV_DIAG AS DiagnosisObservation,
    (u.NOMBRE_US + ' ' + u.APELLIDO_US) AS DoctorFullName,
    tr.DESCRIP_TRATA AS TreatmentDescription,
    tr.ID_TRATAMIENTO AS TreatmentId,
    meds.MedicationIds,
    exs.ExamIds
FROM CONSULTA c
LEFT JOIN cd_ranked cd ON cd.ID_CONSULTA = c.ID_CONSULTA AND cd.rn = 1
LEFT JOIN DIAGNOSTICO dia ON dia.ID_DIAGNOSTICO = cd.ID_DIAGNOSTICO
LEFT JOIN USUARIO u ON u.ID_USUARIO = c.ID_MEDICO
OUTER APPLY (
    SELECT TOP (1) t.ID_TRATAMIENTO, t.DESCRIP_TRATA
    FROM TRATAMIENTO t
    WHERE t.ID_CONSULTA = c.ID_CONSULTA
    ORDER BY t.ID_TRATAMIENTO DESC
) tr
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(varchar(20), tm.ID_MEDICAMENTO), ',') AS MedicationIds
    FROM TRATAMIENTO_MEDICAMENTO tm
    WHERE tm.ID_TRATAMIENTO = tr.ID_TRATAMIENTO
) meds
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(varchar(20), te.ID_EXAMEN), ',') AS ExamIds
    FROM TRATAMIENTO_EXAMEN te
    WHERE te.ID_TRATAMIENTO = tr.ID_TRATAMIENTO
) exs
";

        // -------------------- GetPatientHistoryAsync --------------------
        public async Task<(IEnumerable<ConsultationReportDto> items, long total)> GetPatientHistoryAsync(
            int patientId, DateTime? from, DateTime? to, int page, int limit)
        {
            var items = new List<ConsultationReportDto>();
            var count = 0L;

            using var conn = _context.OpenConnection();

            // Filtro dinámico por paciente (vía triage) y fechas
            var whereSb = new StringBuilder(@"
WHERE 
    t.ID_PACIENTE = @patientId 
    OR c.ID_TRIAGE IN (SELECT ID_Triage FROM TRIAGE WHERE ID_PACIENTE = @patientId)
");

            if (from.HasValue)
                whereSb.Append(" AND c.FECHA_INICIO_CONSULTA >= @fromDate ");
            if (to.HasValue)
                whereSb.Append(" AND c.FECHA_INICIO_CONSULTA <= @toDate ");

            // --- COUNT ---
            using (var cmdCount = conn.CreateCommand())
            {
                cmdCount.CommandText = $@"
SELECT COUNT(1)
FROM CONSULTA c
LEFT JOIN TRIAGE t ON t.ID_Triage = c.ID_TRIAGE
{whereSb}";

                var p = cmdCount.CreateParameter(); p.ParameterName = "@patientId"; p.Value = patientId; cmdCount.Parameters.Add(p);
                if (from.HasValue) { var f = cmdCount.CreateParameter(); f.ParameterName = "@fromDate"; f.Value = from.Value; cmdCount.Parameters.Add(f); }
                if (to.HasValue) { var tparam = cmdCount.CreateParameter(); tparam.ParameterName = "@toDate"; tparam.Value = to.Value; cmdCount.Parameters.Add(tparam); }

                var cntObj = await ExecuteScalarAsync(cmdCount);
                if (cntObj != null && long.TryParse(cntObj.ToString(), out var cval)) count = cval;
            }

            // --- Main paged query ---
            var sb = new StringBuilder();
            sb.Append(BaseCteSql);
            sb.Append(" LEFT JOIN TRIAGE t ON t.ID_Triage = c.ID_TRIAGE ");
            sb.Append(whereSb.ToString());
            sb.Append(" ORDER BY c.FECHA_INICIO_CONSULTA DESC ");
            sb.Append(" OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;");

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sb.ToString();

            var pId = cmd.CreateParameter(); pId.ParameterName = "@patientId"; pId.Value = patientId; cmd.Parameters.Add(pId);
            if (from.HasValue) { var f = cmd.CreateParameter(); f.ParameterName = "@fromDate"; f.Value = from.Value; cmd.Parameters.Add(f); }
            if (to.HasValue) { var tparam = cmd.CreateParameter(); tparam.ParameterName = "@toDate"; tparam.Value = to.Value; cmd.Parameters.Add(tparam); }

            var offset = (page - 1) * limit;
            var pOffset = cmd.CreateParameter(); pOffset.ParameterName = "@offset"; pOffset.Value = offset; cmd.Parameters.Add(pOffset);
            var pLimit = cmd.CreateParameter(); pLimit.ParameterName = "@limit"; pLimit.Value = limit; cmd.Parameters.Add(pLimit);

            using var reader = await ExecuteReaderAsync(cmd);
            while (await ReadAsync(reader))
            {
                var dto = MapReaderToDto(reader);
                items.Add(dto);
            }

            return (items, count);
        }

        // -------------------- GetConsultationDetailAsync --------------------
        public async Task<ConsultationReportDto?> GetConsultationDetailAsync(int patientHistorialId, int consultaId)
        {
            using var conn = _context.OpenConnection();
            using var cmd = conn.CreateCommand();

            // Reutiliza CTE pero filtrando por consulta específica (aquí se usa ID_CONSULTA)
            cmd.CommandText = @"
;WITH cd_ranked AS (
    SELECT cd.ID_CD,
           cd.ID_CONSULTA,
           cd.ID_DIAGNOSTICO,
           ROW_NUMBER() OVER (PARTITION BY cd.ID_CONSULTA ORDER BY cd.ID_CD DESC) AS rn
    FROM CONSULTA_DIAGNOSTICO cd
)
SELECT 
    c.ID_CONSULTA,
    c.ID_HISTORIAL,
    c.ID_MEDICO,
    c.ID_TRIAGE,
    c.ID_ESTADO,
    c.FECHA_INICIO_CONSULTA,
    c.FECHA_FIN_CONSULTA,
    cd.ID_CD AS DiagnosisRowId,
    cd.ID_DIAGNOSTICO AS DiagnosisId,
    dia.NOMBRE_DIAG AS DiagnosisName,
    dia.OBSERV_DIAG AS DiagnosisObservation,
    (u.NOMBRE_US + ' ' + u.APELLIDO_US) AS DoctorFullName,
    tr.DESCRIP_TRATA AS TreatmentDescription,
    tr.ID_TRATAMIENTO AS TreatmentId,
    meds.MedicationIds,
    exs.ExamIds
FROM CONSULTA c
LEFT JOIN cd_ranked cd ON cd.ID_CONSULTA = c.ID_CONSULTA AND cd.rn = 1
LEFT JOIN DIAGNOSTICO dia ON dia.ID_DIAGNOSTICO = cd.ID_DIAGNOSTICO
LEFT JOIN USUARIO u ON u.ID_USUARIO = c.ID_MEDICO
OUTER APPLY (
    SELECT TOP (1) t.ID_TRATAMIENTO, t.DESCRIP_TRATA
    FROM TRATAMIENTO t
    WHERE t.ID_CONSULTA = c.ID_CONSULTA
    ORDER BY t.ID_TRATAMIENTO DESC
) tr
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(varchar(20), tm.ID_MEDICAMENTO), ',') AS MedicationIds
    FROM TRATAMIENTO_MEDICAMENTO tm
    WHERE tm.ID_TRATAMIENTO = tr.ID_TRATAMIENTO
) meds
OUTER APPLY (
    SELECT STRING_AGG(CONVERT(varchar(20), te.ID_EXAMEN), ',') AS ExamIds
    FROM TRATAMIENTO_EXAMEN te
    WHERE te.ID_TRATAMIENTO = tr.ID_TRATAMIENTO
) exs
WHERE c.ID_CONSULTA = @consultaId;
";
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@consultaId"; p1.Value = consultaId; cmd.Parameters.Add(p1);

            using var reader = await ExecuteReaderAsync(cmd);
            if (await ReadAsync(reader))
            {
                return MapReaderToDto(reader);
            }
            return null;
        }

        // -------------------- GetPatientHistoryForPdfAsync --------------------
        public async Task<IEnumerable<ConsultationReportDto>> GetPatientHistoryForPdfAsync(
            int patientId, DateTime? from, DateTime? to, int maxRows = 1000)
        {
            var list = new List<ConsultationReportDto>();
            using var conn = _context.OpenConnection();
            using var cmd = conn.CreateCommand();

            var sb = new StringBuilder();
            sb.Append(BaseCteSql);
            sb.Append(" LEFT JOIN TRIAGE t ON t.ID_Triage = c.ID_TRIAGE ");
            sb.Append(@"
WHERE 
    t.ID_PACIENTE = @patientId 
    OR c.ID_TRIAGE IN (SELECT ID_Triage FROM TRIAGE WHERE ID_PACIENTE = @patientId)
");

            if (from.HasValue) sb.Append(" AND c.FECHA_INICIO_CONSULTA >= @fromDate ");
            if (to.HasValue) sb.Append(" AND c.FECHA_INICIO_CONSULTA <= @toDate ");

            sb.Append(" ORDER BY c.FECHA_INICIO_CONSULTA DESC ");
            sb.Append(" OFFSET 0 ROWS FETCH NEXT @maxRows ROWS ONLY;");

            cmd.CommandText = sb.ToString();

            var p = cmd.CreateParameter();
            p.ParameterName = "@patientId";
            p.Value = patientId;
            cmd.Parameters.Add(p);

            if (from.HasValue)
            {
                var pFrom = cmd.CreateParameter();
                pFrom.ParameterName = "@fromDate";
                pFrom.Value = from.Value;
                cmd.Parameters.Add(pFrom);
            }

            if (to.HasValue)
            {
                var pTo = cmd.CreateParameter();
                pTo.ParameterName = "@toDate";
                pTo.Value = to.Value;
                cmd.Parameters.Add(pTo);
            }

            var pMax = cmd.CreateParameter();
            pMax.ParameterName = "@maxRows";
            pMax.Value = maxRows;
            cmd.Parameters.Add(pMax);

            using var reader = await ExecuteReaderAsync(cmd);
            while (await ReadAsync(reader))
            {
                list.Add(MapReaderToDto(reader));
            }

            return list;
        }

        #region Helper ADO wrappers (async)

        private static async Task<object?> ExecuteScalarAsync(IDbCommand cmd)
        {
            if (cmd is System.Data.Common.DbCommand dbCmd)
            {
                return await dbCmd.ExecuteScalarAsync();
            }
            return cmd.ExecuteScalar();
        }

        private static async Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd)
        {
            if (cmd is System.Data.Common.DbCommand dbCmd)
            {
                return await dbCmd.ExecuteReaderAsync();
            }
            return cmd.ExecuteReader();
        }

        private static async Task<bool> ReadAsync(IDataReader reader)
        {
            if (reader is System.Data.Common.DbDataReader dbReader)
                return await dbReader.ReadAsync();
            return reader.Read();
        }

        private static ConsultationReportDto MapReaderToDto(IDataReader reader)
        {
            var dto = new ConsultationReportDto();

            dto.ConsultationId = reader["ID_CONSULTA"] != DBNull.Value ? Convert.ToInt32(reader["ID_CONSULTA"]) : 0;
            dto.HistorialId = reader["ID_HISTORIAL"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ID_HISTORIAL"]) : null;
            dto.DoctorId = reader["ID_MEDICO"] != DBNull.Value ? Convert.ToInt32(reader["ID_MEDICO"]) : 0;
            dto.TriageId = reader["ID_TRIAGE"] != DBNull.Value ? Convert.ToInt32(reader["ID_TRIAGE"]) : 0;
            dto.EstadoId = reader["ID_ESTADO"] != DBNull.Value ? Convert.ToInt32(reader["ID_ESTADO"]) : 0;

            dto.FechaInicioConsulta = reader["FECHA_INICIO_CONSULTA"] != DBNull.Value ? Convert.ToDateTime(reader["FECHA_INICIO_CONSULTA"]) : DateTime.MinValue;
            dto.FechaFinConsulta = reader["FECHA_FIN_CONSULTA"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["FECHA_FIN_CONSULTA"]) : null;

            dto.DiagnosisRowId = reader["DiagnosisRowId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["DiagnosisRowId"]) : null;
            dto.DiagnosisId = reader["DiagnosisId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["DiagnosisId"]) : null;

            // Nuevos campos
            dto.DiagnosisName = ColumnExists(reader, "DiagnosisName") && reader["DiagnosisName"] != DBNull.Value
                ? reader["DiagnosisName"].ToString()
                : null;

            dto.DiagnosisObservation = ColumnExists(reader, "DiagnosisObservation") && reader["DiagnosisObservation"] != DBNull.Value
                ? reader["DiagnosisObservation"].ToString()
                : null;

            dto.DoctorFullName = ColumnExists(reader, "DoctorFullName") && reader["DoctorFullName"] != DBNull.Value
                ? reader["DoctorFullName"].ToString()
                : null;

            dto.TreatmentDescription = ColumnExists(reader, "TreatmentDescription") && reader["TreatmentDescription"] != DBNull.Value
                ? reader["TreatmentDescription"].ToString()
                : null;

            dto.TreatmentId = ColumnExists(reader, "TreatmentId") && reader["TreatmentId"] != DBNull.Value
                ? (int?)Convert.ToInt32(reader["TreatmentId"])
                : null;

            dto.MedicationIds = ColumnExists(reader, "MedicationIds") && reader["MedicationIds"] != DBNull.Value
                ? reader["MedicationIds"].ToString()
                : null;

            dto.ExamIds = ColumnExists(reader, "ExamIds") && reader["ExamIds"] != DBNull.Value
                ? reader["ExamIds"].ToString()
                : null;

            return dto;
        }

        private static bool ColumnExists(IDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        // -------------------- GetHistorialIdByUserIdAsync (sin cambios) --------------------
        public async Task<int?> GetHistorialIdByUserIdAsync(int userId)
        {
            using var conn = _context.OpenConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
SELECT TOP(1) ID_HISTORIAL
FROM HISTORIAL
WHERE ID_PACIENTE = @userId;
";
            var p = cmd.CreateParameter();
            p.ParameterName = "@userId";
            p.Value = userId;
            cmd.Parameters.Add(p);

            var result = await ExecuteScalarAsync(cmd);
            if (result == null || result == DBNull.Value) return null;
            return Convert.ToInt32(result);
        }
    }
}
