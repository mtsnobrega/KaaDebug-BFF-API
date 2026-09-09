/*
 * Responsabilidade:
 * O principal Controller do sistema. Ponto de entrada para operações 
 * de CRUD das plantas, visualização de gráficos e integrações complexas (como 
 * uploads de imagens para diagnósticos de IA).
 * 
 * Endpoints:
 * - GET /plants (Lista)
 * - GET /plants/{id} (Detalhes / Aggregate)
 * - POST /plants (Cadastro)
 * - PUT /plants/{id} (Atualização e reassociação de device)
 * - DELETE /plants/{id} (Exclusão)
 * - GET /plants/{id}/history (Dados temporais dos sensores)
 * - GET /plants/{id}/care-tips (Dicas JSON)
 * - POST /plants/{id}/diagnosis (Upload de Imagem [multipart/form-data])
 * - GET /plants/{id}/diagnosis (Histórico de análises visuais)
 * - PUT /plants/{id}/device (Vinculação forçada de hardware)
 * 
 * Serviços utilizados: 
 * IPlantService, IPlantHistoryService, IPlantCareService, IDiagnosisService
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Authorize]
    [Route("plants")]
    public class PlantsController : ApiControllerBase
    {
        private readonly IPlantService _plantService;
        private readonly IPlantHistoryService _historyService;
        private readonly IPlantCareService _careService;
        private readonly IDiagnosisService _diagnosisService;

        public PlantsController(
            IPlantService plantService,
            IPlantHistoryService historyService,
            IPlantCareService careService,
            IDiagnosisService diagnosisService)
        {
            _plantService = plantService;
            _historyService = historyService;
            _careService = careService;
            _diagnosisService = diagnosisService;
        }

        /// <summary>
        /// Lista todas as plantas do usuário autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PlantSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _plantService.GetAllAsync(CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retorna detalhes completos de uma planta: dados cadastrais,
        /// indicadores atuais dos sensores (24h), status do dispositivo
        /// e notificações relacionadas.
        /// </summary>
        [HttpGet("{id:guid}", Name = "GetPlantById")]
        [ProducesResponseType(typeof(PlantDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetails([FromRoute] Guid id)
        {
            var result = await _plantService.GetDetailsAsync(id, CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Cadastra uma nova planta.
        /// Ao informar DeviceCode, o dispositivo é associado imediatamente.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PlantSummaryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreatePlantRequest request)
        {
            var result = await _plantService.CreateAsync(request, CurrentUserId);
            return ToCreatedResult(result, "GetPlantById", new { id = result.Data?.Id });
        }

        /// <summary>
        /// Atualiza nome e/ou dispositivo associado de uma planta.
        /// DeviceCode null = sem alteração | "" = desassociar | "ESP32-XXXX" = associar/substituir
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePlantRequest request)
        {
            var result = await _plantService.UpdateAsync(id, request, CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Remove a planta e todos os seus dados históricos (leituras, notificações, diagnósticos).
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var result = await _plantService.DeleteAsync(id, CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retorna o histórico de leituras dos sensores por período.
        /// Query param: period = 24h | 7d | 30d (padrão: 24h)
        /// </summary>
        [HttpGet("{id:guid}/history")]
        [ProducesResponseType(typeof(PlantHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(
            [FromRoute] Guid id,
            [FromQuery] string period = "24h")
        {
            var result = await _historyService.GetHistoryAsync(id, CurrentUserId, period);
            return ToActionResult(result);
        }

        /// <summary>
        /// Retorna as dicas de cuidados baseadas na espécie da planta.
        /// O conteúdo vem do campo care_info (JSONB) da tabela species.
        /// </summary>
        [HttpGet("{id:guid}/care-tips")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCareTips([FromRoute] Guid id)
        {
            var result = await _careService.GetCareInfoAsync(id, CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Envia imagem para análise por IA e retorna o diagnóstico.
        /// A imagem deve ser enviada como multipart/form-data, campo "image".
        /// </summary>
        [HttpPost("{id:guid}/diagnosis")]
        [ProducesResponseType(typeof(DiagnosisResultResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Diagnose(
            [FromRoute] Guid id,
            [FromForm] IFormFile image)
        {
            if (image is null || image.Length == 0)
                return BadRequest(new { error = "Nenhuma imagem foi enviada." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(image.ContentType.ToLower()))
                return BadRequest(new { error = "Formato de imagem inválido. Use JPEG, PNG ou WebP." });

            using var stream = image.OpenReadStream();
            var result = await _diagnosisService.AnalyzeAsync(
                id, CurrentUserId, stream, image.FileName);

            return ToActionResult(result);
        }

        /// <summary>
        /// Lista o histórico de diagnósticos realizados para a planta.
        /// </summary>
        [HttpGet("{id:guid}/diagnosis")]
        [ProducesResponseType(typeof(IEnumerable<DiagnosisResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDiagnosisHistory([FromRoute] Guid id)
        {
            var result = await _diagnosisService.GetHistoryAsync(id, CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Associa ou desassocia um dispositivo ESP32 de uma planta.
        /// Não exige o nome da planta — operação exclusiva de dispositivo.
        ///
        /// DeviceCode ""           → desassocia o dispositivo atual
        /// DeviceCode "ESP32-XXXX" → associa/substitui o dispositivo
        /// </summary>
        [HttpPut("{id:guid}/device")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssociateDevice(
            [FromRoute] Guid id,
            [FromBody] AssociateDeviceRequest request)
        {
            var result = await _plantService.AssociateDeviceAsync(id, request, CurrentUserId);
            return ToActionResult(result);
        }
    }
}
