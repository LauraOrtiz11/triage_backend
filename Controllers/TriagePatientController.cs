using Microsoft.AspNetCore.Mvc;
using triage_backend.Services;
using triage_backend.Utilities;

namespace triage_backend.Controllers
{
    /// <summary>
    /// Controller responsible for managing triage patient operations.
    /// Provides access to triage data filtered by priority color.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TriagePatientController : ControllerBase
    {
        private readonly ITriagePatientService _triageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriagePatientController"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration for database connection.</param>
        public TriagePatientController(IConfiguration configuration)
        {
            var context = new ContextDB(configuration);
            _triageService = new TriageService(context);
        }

        /// <summary>
        /// Retrieves a list of triage patients, optionally filtered by priority color.
        /// </summary>
        /// <param name="color">
        /// (Optional) Priority color to filter triage patients.
        /// Valid values: 'rojo', 'naranja', 'amarillo', 'verde', 'azul'.
        /// </param>
        /// <returns>
        /// A list of patients with triage information and assigned priority.
        /// </returns>
        /// <response code="200">Returns the list of triage patients.</response>
        /// <response code="400">If an error occurs during the process.</response>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public IActionResult GetPatients([FromQuery] string? color)
        {
            try
            {
                var patients = _triageService.GetTriagePatients(color);
                return Ok(new { success = true, data = patients });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
