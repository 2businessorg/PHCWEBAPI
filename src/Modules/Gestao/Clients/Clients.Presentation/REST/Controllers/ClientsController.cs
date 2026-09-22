using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Clients.Application.Features.CreateClient;
using Clients.Application.Features.CreateClientBulk;
using Clients.Application.Features.UpdateClient;
using Clients.Application.Features.DeleteClient;
using Clients.Application.Features.GetClientByNo;
using Clients.Application.Features.GetAllClients;
using Clients.Application.DTOs;
using Clients.Application.Errors;
using Clients.Presentation.REST.Services;
using Shared.Kernel.Responses;
using Shared.Kernel.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace Clients.Presentation.REST.Controllers;

[ApiController]
[Route("api/clients")]
[Produces("application/json")]
public sealed class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IClientsLinkBuilder _linkBuilder;

    public ClientsController(IMediator mediator, IClientsLinkBuilder linkBuilder)
    {
        _mediator = mediator;
        _linkBuilder = linkBuilder;
    }

    /// <summary>
    /// Obter todos os clientes com suporte a filtros e HATEOAS
    /// </summary>
    /// <param name="ncont">Filtro exato por NUIT/Ncont</param>
    /// <param name="nome">Filtro por nome (like)</param>
    /// <param name="telefone">Filtro por telefone (like)</param>
    /// <param name="morada">Filtro por morada (like)</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpGet]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("clients-query")]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(CollectionResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CollectionResponseDTO>> GetAll(
        [FromQuery(Name = "id")] decimal? id = null,
        [FromQuery] decimal? no = null,
        [FromQuery] string? ncont = null,
        [FromQuery] string? nome = null,
        [FromQuery] string? telefone = null,
        [FromQuery] string? morada = null,
        [FromQuery(Name = "branch")] decimal? estab = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var clientId = id ?? no;
        var result = await _mediator.Send(new GetAllClientsQuery(clientId, ncont, nome, telefone, morada, estab, page, pageSize), ct);

        var totalItems = result.TotalItems;
        page = result.CurrentPage;
        pageSize = result.PageSize;

        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
        if (totalPages < 1) totalPages = 1;

        // montar links relativos (filtros anexados)
        var filterSuffix = BuildQueryString(clientId, ncont, nome, telefone, morada, estab);
        if (filterSuffix.StartsWith("?")) filterSuffix = "&" + filterSuffix.Substring(1);

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetSelfPaginationLink(page, pageSize, filterSuffix),
            _linkBuilder.GetLastLink(totalPages, pageSize, filterSuffix),
            _linkBuilder.GetCreateLink()
        };

        if (page < totalPages)
        {
            links.Insert(1, _linkBuilder.GetNextLink(page + 1, pageSize, filterSuffix));
        }

        var items = result.Items
            .Select(MapClientItem)
            .Cast<object>()
            .ToList();

        var response = CollectionResponseDTO.FromPaged(items, totalItems, page, pageSize, links);
        return Ok(response);
    }

    /// <summary>
    /// Obter cliente por número (No) e estabelecimento (Estab)
    /// Ambos os parâmetros de rota são obrigatórios
    /// </summary>
    /// <param name="no">Número do cliente (identificador canônico)</param>
    /// <param name="estab">Estabelecimento</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpGet("{id:decimal}/{estab:decimal}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("clients-query")]
    [ProducesResponseType(typeof(SingleResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SingleResponseDTO>> GetByNo(
        [FromRoute(Name = "id")] decimal no,
        [FromRoute] decimal estab,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetClientByNoQuery(no, estab), ct);

        if (result is null)
        {
            return NotFound(PaginatedResponseDTO.Error(
                code: ClientsErrorCatalog.ClientNotFound.Code,
                message: ClientsErrorCatalog.ClientNotFound.Description));
        }

        var item = MapClientItem(result);

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetDetailLink(no, estab),
            _linkBuilder.GetUpdateLink(no, estab),
            _linkBuilder.GetDeleteLink(no, estab),
            _linkBuilder.GetListLink()
        };

        var response = SingleResponseDTO.FromItem(item, links);
        return Ok(response);
    }

    /// <summary>
    /// Criar novo cliente
    /// </summary>
    /// <param name="dto">Dados do cliente</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("clients-create")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Create(
        [FromBody] CreateClientInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId) || userId.Length > 20)
            userId = null;

        var command = new CreateClientCommand(dto, userId);

        try
        {
            var result = await _mediator.Send(command, ct);

            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetDetailLink(result.No, result.Estab),
                _linkBuilder.GetListLink()
            };

            var response = new PaginatedResponseDTO(
                code: ClientsErrorCatalog.Success.Code,
                message: "Cliente criado com sucesso",
                data: new[] { (object)result },
                links: links);

            return CreatedAtAction(nameof(GetByNo), new { id = result.No, estab = result.Estab }, response);
        }
        catch (ClientsModuleException ex)
        {
            return BadRequest(PaginatedResponseDTO.Error(
                code: ex.Code,
                message: ex.Message));
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var message = first?.ErrorMessage ?? ex.Message;
            var code = !string.IsNullOrWhiteSpace(first?.ErrorCode) && first!.ErrorCode.StartsWith("CL")
                ? first.ErrorCode
                : ClientsErrorCatalog.InvalidNoEstabCombination.Code;

            return BadRequest(PaginatedResponseDTO.Error(
                code: code,
                message: message));
        }
    }

    /// <summary>
    /// Criar múltiplos clientes em bulk (melhor esforço - insere os válidos, rejeita os inválidos)
    /// </summary>
    /// <param name="bulkDto">Lote com até 100 clientes</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpPost("bulk")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("clients-bulk-create")]
    [ProducesResponseType(typeof(BulkResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BulkResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BulkResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BulkResponseDTO>> CreateBulk(
        [FromBody] CreateClientBulkInputDTO bulkDto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId) || userId.Length > 20)
            userId = null;

        var command = new CreateClientBulkCommand(bulkDto, userId);

        try
        {
            var bulkResult = await _mediator.Send(command, ct);

            // Montar resposta com dados dos sucessos
            var links = new List<HATEOASLink>
            {
                _linkBuilder.GetListLink()
            };

            var response = new BulkResponseDTO
            {
                Code = bulkResult.Code,
                Message = bulkResult.Message,
                Data = bulkResult.Items,
                SuccessCount = bulkResult.SuccessCount,
                FailureCount = bulkResult.FailureCount,
                Links = links
            };

            // Se todos falharam, retorna 400; se todos passou, 201; senão 206 (partial)
            if (bulkResult.FailureCount > 0 && bulkResult.SuccessCount == 0)
            {
                return BadRequest(response);
            }

            return bulkResult.SuccessCount > 0
                ? CreatedAtAction(nameof(GetAll), response)
                : BadRequest(response);
        }
        catch (ValidationException ex)
        {
            var first = ex.Errors.FirstOrDefault();
            var message = first?.ErrorMessage ?? ex.Message;
            var code = first?.ErrorCode ?? ClientsErrorCatalog.BulkEmpty.Code;

            return BadRequest(new BulkResponseDTO
            {
                Code = code,
                Message = message,
                Data = new(),
                SuccessCount = 0,
                FailureCount = 1,
                Links = null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new BulkResponseDTO
            {
                Code = ClientsErrorCatalog.DatabaseUpdateError.Code,
                Message = $"Erro ao processar lote: {ex.Message}",
                Data = new(),
                SuccessCount = 0,
                FailureCount = 1,
                Links = null
            });
        }
    }

    /// <summary>
    /// Atualizar cliente (atualização parcial)
    /// </summary>
    /// <param name="no">Identificador canônico do cliente no servidor</param>
    /// <param name="estab">Estabelecimento (opcional, default=0)</param>
    /// <param name="dto">Dados a atualizar</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpPatch("{id:decimal}/{estab:decimal?}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("clients-update")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Update(
        [FromRoute(Name = "id")] decimal no,
        [FromBody] UpdateClientInputDTO dto,
        [FromRoute] decimal estab = 0,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var command = new UpdateClientCommand(no, dto, userId, estab);
        var result = await _mediator.Send(command, ct);

        if (result is null)
        {
            return NotFound(PaginatedResponseDTO.Error(
                code: ClientsErrorCatalog.ClientNotFound.Code,
                message: ClientsErrorCatalog.ClientNotFound.Description));
        }

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetDetailLink(no, estab),
            _linkBuilder.GetListLink()
        };

        return Ok(new PaginatedResponseDTO(
            code: ClientsErrorCatalog.Success.Code,
            message: "Cliente atualizado com sucesso",
            data: new[] { (object)result },
            links: links));
    }

    /// <summary>
    /// Eliminar cliente
    /// </summary>
    /// <param name="no">Identificador canônico do cliente no servidor</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpDelete("{id:decimal}/{estab:decimal?}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("clients-delete")]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PaginatedResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponseDTO>> Delete(
        [FromRoute(Name = "id")] decimal no,
        [FromRoute] decimal estab = 0,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteClientCommand(no, estab), ct);

        if (!result)
        {
            return NotFound(PaginatedResponseDTO.Error(
                code: ClientsErrorCatalog.ClientNotFound.Code,
                message: ClientsErrorCatalog.ClientNotFound.Description));
        }

        var links = new List<HATEOASLink>
        {
            _linkBuilder.GetListLink()
        };

        return Ok(new PaginatedResponseDTO(
            code: ClientsErrorCatalog.Success.Code,
            message: "Cliente eliminado com sucesso",
            links: links));
    }

    /// <summary>
    /// Construir string de query a partir dos parâmetros de filtro
    /// </summary>
    private string BuildQueryString(decimal? id, string? ncont, string? nome, string? telefone, string? morada, decimal? estab = null)
    {
        var parts = new List<string>();
        if (id.HasValue) parts.Add($"id={id.Value}");
        if (!string.IsNullOrWhiteSpace(ncont)) parts.Add($"ncont={Uri.EscapeDataString(ncont)}");
        if (!string.IsNullOrWhiteSpace(nome)) parts.Add($"nome={Uri.EscapeDataString(nome)}");
        if (!string.IsNullOrWhiteSpace(telefone)) parts.Add($"telefone={Uri.EscapeDataString(telefone)}");
        if (!string.IsNullOrWhiteSpace(morada)) parts.Add($"morada={Uri.EscapeDataString(morada)}");
        if (estab.HasValue) parts.Add($"branch={estab.Value}");

        return parts.Any() ? "?" + string.Join("&", parts) : string.Empty;
    }

    private static Dictionary<string, object?> MapClientItem(ClientOutputDTO client)
    {
        return new Dictionary<string, object?>()
        {
            ["id"] = client.No,
            ["name"] = client.Nome?.Trim(),
            ["nuit"] = client.Ncont?.Trim(),
            ["branch"] = client.Estab,
            ["phone"] = client.Telefone?.Trim(),
            ["address"] = client.Morada?.Trim(),
            ["email"] = client.Email?.Trim(),
            ["inactive"] = client.Inactivo,
            ["addFields"] = TrimAddFields(client.AddFields)
        };
    }

    private static Dictionary<string, object?>? TrimAddFields(Dictionary<string, object?>? addFields)
    {
        if (addFields == null)
            return null;

        var trimmed = new Dictionary<string, object?>();
        foreach (var kvp in addFields)
        {
            trimmed[kvp.Key] = kvp.Value is string str ? str.Trim() : kvp.Value;
        }
        return trimmed;
    }
}