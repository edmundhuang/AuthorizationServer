using Microsoft.AspNetCore.Mvc;

namespace AuthorizationServer.Controllers
{
    [Route("api/[controller]")]
    public class ValueController : ControllerBase
    {
        private readonly ILogger<ValueController> _logger;
        public ValueController(ILogger<ValueController> logger)
        {
            _logger = logger;

            _logger.LogInformation($"{this} initialize.");
        }

        [HttpGet]
        public string[] GetAll()
        {
            return new string[] { "Value1", "Value2" };
        }

        [HttpGet("Version")]
        public string Version()
        {
            var ver = System.Reflection.Assembly.GetAssembly(typeof(ValueController)).GetName().Version;

            return ver.ToString();
        }
    }
}
