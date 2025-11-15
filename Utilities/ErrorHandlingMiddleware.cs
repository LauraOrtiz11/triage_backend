using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace triage_backend.Utilities.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); 

                // Captura estados comunes de autenticación y autorización
                if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    await WriteErrorAsync(context, HttpStatusCode.Unauthorized,
                        "No autorizado: debes iniciar sesión para acceder a esta función.");
                }
                else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    await WriteErrorAsync(context, HttpStatusCode.Forbidden,
                        "Acceso denegado: no tienes permisos para esta acción.");
                }
                else if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    await WriteErrorAsync(context, HttpStatusCode.NotFound,
                        "Recurso no encontrado o ruta inválida.");
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            string message = "Ocurrió un error inesperado en el servidor.";

            if (ex is SqlException)
            {
                message = "Error al conectar con la base de datos. Por favor, inténtalo más tarde.";
            }

            await WriteErrorAsync(context, statusCode, message, ex);
        }

        private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode code, string message, Exception? ex = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var response = new
            {
                success = false,
                statusCode = (int)code,
                error = message,
                detail = ex?.Message // Se puede quitar en producción
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            await context.Response.WriteAsync(json);
        }
    }
}
