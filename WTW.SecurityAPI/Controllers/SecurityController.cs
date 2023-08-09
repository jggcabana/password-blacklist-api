using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using WTW.SecurityAPI.Models.Request;
using WTW.SecurityServices.Interfaces;
using WTW.SecurityServices.Services;

namespace WTW.SecurityAPI.Controllers
{
    [Route("api/security")]
    [ApiController]
    [EnableCors("AnyOrigin")]
    public class SecurityController : ControllerBase
    {
        private readonly ILogger<SecurityController> _logger;
        private readonly ISecurityService _securityService;

        public SecurityController(ILogger<SecurityController> logger, ISecurityService securityService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        [HttpPost]
        [Route("check-password-validity")]
        public async Task<IActionResult> CheckPasswordValidity([FromBody]CreatePasswordRequest request)
        {
            try
            {
                var result = await _securityService.IsValidPassword(request.Password);
                return Ok(result);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e.Message);
                return StatusCode(503);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        [Route("test")]
        public  async Task<IActionResult> Test()
        {
            try
            {
                return Ok(await _securityService.Test());
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return StatusCode(500, e.Message);
            }
        }
    }
}
