using Advances.Application.DTOs;
using MediatR;

namespace Advances.Application.Features.GetAdvanceById;

/// <summary>
/// Query para obter um Adiantamento por chave composta (ndoc / rno / rdano)
/// </summary>
public sealed record GetAdvanceByIdQuery(
    decimal Ndoc,
    decimal Rno,
    decimal Rdano) : IRequest<AdvanceOutputDTO?>;
