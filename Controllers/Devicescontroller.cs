using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Authorize]
    [Route("devices")]
    public class DevicesController : ApiControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Verifica o status de conectividade de um dispositivo ESP32 pelo código.
        /// Usado na tela de Cadastro de Dispositivo para confirmar se o ESP32
        /// está respondendo após a associação.
        /// </summary>
        [HttpGet("{code}/status")]
        [ProducesResponseType(typeof(DeviceVerificationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStatus([FromRoute] string code)
        {
            var result = await _deviceService.VerifyAsync(code);
            return ToActionResult(result);
        }
    }
}
