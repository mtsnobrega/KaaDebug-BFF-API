using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Authorize]
    [Route("species")]
    public class SpeciesController : ApiControllerBase
    {
        private readonly ISpeciesService _speciesService;

        public SpeciesController(ISpeciesService speciesService)
        {
            _speciesService = speciesService;
        }

        /// <summary>
        /// Lista todas as espécies disponíveis no catálogo do sistema.
        /// Usada na tela de Cadastro de Planta para seleção de espécie.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SpeciesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _speciesService.GetAllAsync();
            return ToActionResult(result);
        }
    }
}
