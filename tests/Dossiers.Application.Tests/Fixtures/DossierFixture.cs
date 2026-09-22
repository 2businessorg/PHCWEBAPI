using Bogus;
using Dossiers.Application.DTOs;
using Dossiers.Domain.Entities;

namespace Dossiers.Application.Tests.Fixtures;

/// <summary>
/// Fixture para gerar dados de teste para Dossiers
/// </summary>
public class DossierFixture
{
    private readonly Faker _faker = new("pt_PT");

    /// <summary>
    /// Cria um DTO válido para criar dossier
    /// </summary>
    public CreateDossierInputDTO CreateValidDossierDto()
    {
        return new CreateDossierInputDTO
        {
            Ndos = 1,
            Boano = 2026,
            No = 58,
            Estab = 0,
            Data = DateTime.Now.Date,
            Moeda = "MT",
            Linhas = new List<CreateDossierLineInputDTO>
            {
                new()
                {
                    Referencia = "REF-001",
                    Quantidade = 2,
                    PrecoUnitario = 5000.00m,
                    TabIva = 1,
                    IvaIncl = false
                }
            }
        };
    }

    /// <summary>
    /// Cria um DTO de dossier com múltiplas linhas
    /// </summary>
    public CreateDossierInputDTO CreateDossierWithMultipleLines()
    {
        return new CreateDossierInputDTO
        {
            Ndos = 1,
            Boano = 2026,
            No = 58,
            Estab = 0,
            Data = DateTime.Now.Date,
            Moeda = "MT",
            Linhas = new List<CreateDossierLineInputDTO>
            {
                new()
                {
                    Referencia = "REF-001",
                    Quantidade = 2,
                    PrecoUnitario = 5000.00m,
                    TabIva = 1,
                    IvaIncl = false
                },
                new()
                {
                    Referencia = "REF-002",
                    Quantidade = 1,
                    PrecoUnitario = 3000.00m,
                    TabIva = 2,
                    IvaIncl = true
                }
            }
        };
    }

    /// <summary>
    /// Cria um DTO de dossier com dados mínimos (alguns campos omitidos)
    /// </summary>
    public CreateDossierInputDTO CreateDossierWithMinimalData()
    {
        return new CreateDossierInputDTO
        {
            Ndos = 1,
            Boano = 2026,
            No = 2,
            Estab = 0,
            Data = DateTime.Now.Date,
            Linhas = new List<CreateDossierLineInputDTO>
            {
                new()
                {
                    Referencia = "REF-001",
                    Quantidade = 6.00m
                }
            }
        };
    }

    /// <summary>
    /// Cria uma entidade Bo válida
    /// </summary>
    public Bo CreateValidBo()
    {
        return new Bo
        {
            Bostamp = GenerateStamp(),
            Ndos = 1,
            Nmdos = "FATURA",
            Obrano = _faker.Random.Int(1, 999),
            Boano = 2026,
            No = 58,
            Estab = 0,
            Nome = _faker.Company.CompanyName(),
            Dataobra = DateTime.Now.Date,
            Moeda = "MT",
            Ousrinis = "USER001",
            Ousrdata = DateTime.Now,
            Ousrhora = DateTime.Now.ToString("HH:mm:ss"),
            Usrinis = "USER001",
            Usrdata = DateTime.Now,
            Usrhora = DateTime.Now.ToString("HH:mm:ss")
        };
    }

    /// <summary>
    /// Cria um cliente válido
    /// </summary>
    public DossierClient CreateValidClient()
    {
        return new DossierClient
        {
            Clstamp = GenerateStamp(),
            No = 58,
            Estab = 0,
            Nome = _faker.Company.CompanyName(),
            Morada = _faker.Address.StreetAddress(),
            Local = _faker.Address.City(),
            Codpost = _faker.Address.ZipCode(),
            Preco = 1,
            Ncont = _faker.Random.Int(1, 10).ToString(),
            Segmento = "SEG-001",
            Telefone = _faker.Phone.PhoneNumber(),
            Contacto = _faker.Name.FullName(),
            Email = _faker.Internet.Email()
        };
    }

    /// <summary>
    /// Cria um stock válido
    /// </summary>
    public St CreateValidStock()
    {
        return new St
        {
            Ststamp = GenerateStamp(),
            Ref = "REF-001",
            Design = "Produto Teste",
            Pv1 = 5000.00m,
            Epv1 = 5000.00m,
            Pv2 = 4800.00m,
            Epv2 = 4800.00m,
            Pv3 = 4600.00m,
            Epv3 = 4600.00m,
            Pv4 = 4400.00m,
            Epv4 = 4400.00m,
            Pv5 = 4200.00m,
            Epv5 = 4200.00m,
            Tabiva = 1,
            Familia = "FAM-001",
            Faminome = "Familia Teste",
            Pcusto = 2500.00m,
            Epcusto = 2500.00m,
            Pcpond = 2400.00m,
            Epcpond = 2400.00m,
            Pcult = 2450.00m,
            Epcult = 2450.00m,
            Cpoc = 1,
            Inactivo = false,
            Iva1Incl = false,
            Iva2Incl = true,
            Iva3Incl = false,
            Iva4Incl = false,
            Iva5Incl = false,
            Ivaincl = false,
            IvapcIncl = false,
            Stock = 100.00m
        };
    }

    /// <summary>
    /// Cria um tipo de dossier válido
    /// </summary>
    public Ts CreateValidTipoDossier()
    {
        return new Ts
        {
            Ndos = 1,
            Nmdos = "FATURA",
            Bdempresas = "CL",
            Qpreco = 1,
            Qprecocusto = 3  // Usa custo padrão
        };
    }

    /// <summary>
    /// Cria uma tabela de IVA válida
    /// </summary>
    public Taxasiva CreateValidTaxasIva()
    {
        return new Taxasiva
        {
            Codigo = 1,
            Taxa = 16.00m
        };
    }

    /// <summary>
    /// Gera um stamp (identificador único)
    /// </summary>
    private static string GenerateStamp()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 25);
    }
}
