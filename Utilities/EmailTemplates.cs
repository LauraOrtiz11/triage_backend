namespace triage_backend.Utilities
{
    public static class EmailTemplates
    {
        public static string BuildPriorityUpdateBody(string patientName, string priorityName, string turnCode)
        {
            // === Colores dinámicos según prioridad ===
            string priorityColor;
            string headerGradient;
            string titleColor = "white";

            switch (priorityName.ToLower())
            {
                case "rojo":
                    priorityColor = "#dc3545";
                    headerGradient = "linear-gradient(90deg, #b71c1c, #f44336)";
                    break;
                case "amarillo":
                    priorityColor = "#ffc107";
                    headerGradient = "linear-gradient(90deg, #ffca28, #fdd835)";
                    titleColor = "#222";
                    break;
                case "verde":
                    priorityColor = "#20c997";
                    headerGradient = "linear-gradient(90deg, #009688, #20c997)";
                    break;
                case "naranja":
                    priorityColor = "#fd7e14";
                    headerGradient = "linear-gradient(90deg, #f57c00, #ffa726)";
                    break;
                case "azul":
                    priorityColor = "#0d6efd";
                    headerGradient = "linear-gradient(90deg, #0d47a1, #2196f3)";
                    break;
                default:
                    priorityColor = "#6c757d";
                    headerGradient = "linear-gradient(90deg, #6c757d, #adb5bd)";
                    break;
            }

            // === Cuerpo del correo ===
            return $@"
<html>
  <body style='font-family: Arial, sans-serif; background-color: #f7f9fc; color: #333; margin: 0; padding: 0;'>
    <div style='max-width: 600px; margin: 30px auto; background: #ffffff; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>

      <!-- Encabezado SIN IMAGEN -->
      <div style='background: {headerGradient}; padding: 25px; text-align: center;'>
        <h2 style='color: {titleColor}; margin: 0; font-size: 26px;'>Registro de Atención en Triage</h2>
      </div>

      <!-- Cuerpo -->
      <div style='padding: 30px; text-align: center;'>
        <p style='font-size: 16px;'>Estimado(a) <b>{patientName}</b>,</p>
        <p style='font-size: 16px;'>Su atención ha sido registrada exitosamente en el sistema de triage.</p>

        <div style='margin: 25px auto; padding: 20px; background: #f1f8ff; border-radius: 8px; display: inline-block;'>
          <p style='font-size: 18px; margin: 10px 0;'>🔹 <b>Prioridad actual:</b></p>
          <p style='font-size: 26px; font-weight: bold; color: {priorityColor}; margin: 5px 0;'>{priorityName}</p>

          <p style='font-size: 18px; margin: 15px 0 5px;'>🔹 <b>Turno asignado:</b></p>
          <p style='font-size: 30px; font-weight: bold; color: #0d6efd; margin: 5px 0;'>{turnCode}</p>
        </div>

        <p style='margin-top: 25px; font-size: 15px;'>Por favor permanezca atento(a) al llamado del personal médico.</p>
      </div>

      <div style='background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 12px; color: #777;'>
        Este es un mensaje automático, por favor no responda.
      </div>

    </div>
  </body>
</html>";
        }
    }
}
