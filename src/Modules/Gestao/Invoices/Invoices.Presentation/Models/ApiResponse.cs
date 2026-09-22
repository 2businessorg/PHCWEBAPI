using System.Collections.Generic;
using Shared.Kernel.Responses;

namespace Invoices.Presentation.Models
{
    /// <summary>Link HATEOAS para navegação</summary>
    public class Link
    {
        /// <summary>Relação do link (self, list, create, etc)</summary>
        public string Rel { get; set; }

        /// <summary>Caminho/URL do link</summary>
        public string Href { get; set; }

        /// <summary>Método HTTP (GET, POST, PUT, DELETE, etc)</summary>
        public string Method { get; set; }
    }

    /// <summary>Estrutura de resposta da API</summary>
    public class ApiResponse<T>
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<Link> Links { get; set; } = new List<Link>();
    }

    /// <summary>Estrutura de resposta com lista</summary>
    public class ApiListResponse<T>
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<T> Items { get; set; }
        public PaginationMeta Meta { get; set; }
        public List<Link> Links { get; set; } = new List<Link>();
    }

    /// <summary>Estrutura de resposta com erro</summary>
    public class ApiErrorResponse
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}
