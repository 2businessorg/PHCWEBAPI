using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Xunit;
using Xunit.Sdk;

namespace Parameters.Application.Tests.Fixtures;

/// <summary>
/// 🎯 AutoData Attribute customizado com AutoMoq
/// Gera automaticamente dados de teste e mocks
/// </summary>
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture();

            // ✅ Configura AutoMoq para criar mocks automaticamente
            fixture.Customize(new AutoMoqCustomization
            {
                ConfigureMembers = true, // ✅ Configura propriedades automáticas
                GenerateDelegates = true // ✅ Gera delegates automaticamente
            });

            // ✅ Customizações específicas do domínio
            fixture.Customize<Parameters.Domain.Entities.Para1>(composer => composer
                .FromFactory(() => new Parameters.Domain.Entities.Para1(
                    para1Stamp: fixture.Create<string>().Substring(0, Math.Min(25, fixture.Create<string>().Length)),
                    descricao: fixture.Create<string>().Substring(0, Math.Min(100, fixture.Create<string>().Length)),
                    valor: fixture.Create<string>(),
                    tipo: fixture.Create<string>().Substring(0, Math.Min(10, fixture.Create<string>().Length)),
                    dec: fixture.Create<decimal?>(),
                    tam: fixture.Create<decimal?>(),
                    criadoPor: "test_user"
                ))
                .OmitAutoProperties());

            return fixture;
        })
    {
    }
}

/// <summary>
/// 🎯 InlineAutoMoqData Attribute
/// Combina InlineData (valores específicos) com AutoMoq (valores gerados)
/// Perfeito para testes parametrizados
/// </summary>
public class InlineAutoMoqDataAttribute : CompositeDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new DataAttribute[]
        {
            new InlineDataAttribute(values),
            new AutoMoqDataAttribute()
        })
    {
    }
}
