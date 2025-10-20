namespace triage_backend.Utilities
{
    public static class EmailTemplates
    {
        public static string BuildPriorityUpdateBody(string patientName, string priorityName, string turnCode)
        {
            return $@"
                <html>
                    <body style='font-family: Arial, sans-serif; color: #333;'>
                    <h3>Estimado(a) {patientName},</h3>
                    <p>Se ha registrado su atención en el sistema de triage.</p>
                    <p>Su prioridad actual es <b>{priorityName}</b> y su turno asignado es <b>{turnCode}</b>.</p>
                    <p>Por favor permanezca atento(a) al llamado del personal médico.</p>
                    <br/>
                    <small>Este es un mensaje automático, por favor no responda.</small>
                </body>
                </html>";
        }
    }
}
