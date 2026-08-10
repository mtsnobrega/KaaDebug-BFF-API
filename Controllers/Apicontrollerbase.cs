using kaadebug_bff_api.Models;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace kaadebug_bff_api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Extrai o userId da claim "sub" do token JWT.
        /// Lança InvalidOperationException se o token não contiver a claim —
        /// o que nunca deve acontecer em endpoints marcados com [Authorize].
        /// </summary>
        protected Guid CurrentUserId
        {
            get
            {
                var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

                if (sub is null || !Guid.TryParse(sub, out var userId))
                    throw new InvalidOperationException("Token JWT inválido: claim 'sub' ausente.");

                return userId;
            }
        }

        /// <summary>
        /// Converte um ServiceResult sem dados para IActionResult com o StatusCode correto.
        /// </summary>
        protected IActionResult ToActionResult(ServiceResult result)
        {
            if (result.Success)
                return Ok();

            return result.StatusCode switch
            {
                404 => NotFound(new { error = result.ErrorMessage }),
                401 => Unauthorized(new { error = result.ErrorMessage }),
                409 => Conflict(new { error = result.ErrorMessage }),
                _ => BadRequest(new { error = result.ErrorMessage })
            };
        }

        /// <summary>
        /// Converte um ServiceResult com dados para IActionResult com o StatusCode correto.
        /// </summary>
        protected IActionResult ToActionResult<T>(ServiceResult<T> result)
        {
            if (result.Success)
                return Ok(result.Data);

            return result.StatusCode switch
            {
                404 => NotFound(new { error = result.ErrorMessage }),
                401 => Unauthorized(new { error = result.ErrorMessage }),
                409 => Conflict(new { error = result.ErrorMessage }),
                _ => BadRequest(new { error = result.ErrorMessage })
            };
        }

        /// <summary>
        /// Igual ao ToActionResult<T> mas retorna 201 Created com o Location header.
        /// Usado nos endpoints POST de criação de recursos.
        /// </summary>
        protected IActionResult ToCreatedResult<T>(ServiceResult<T> result, string routeName, object routeValues)
        {
            if (!result.Success)
                return ToActionResult(result);

            return CreatedAtRoute(routeName, routeValues, result.Data);
        }
    }
}
