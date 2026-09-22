using FluentAssertions;
using Treasury.Application.Matching;
using Xunit;

namespace Treasury.Application.Tests.Matching;

public class ReconciliationTextHintsTests
{
    [Fact]
    public void Party_BciTransferDescription_ShouldKeepCounterparty()
    {
        var party = ReconciliationTextHints.Party(
            "TRF : 2Business-FT3/2023 - GEOSYSTEMS",
            "FT3/2023",
            cheque: null);

        party.Should().Contain("GEOSYSTEMS");
        ReconciliationTextHints.Invoice("TRF : 2Business-FT3/2023 - GEOSYSTEMS", "FT3/2023")
            .Should().Contain("FT");
    }

    [Fact]
    public void PartiesOverlap_PromarVariants_ShouldMatch()
    {
        var bank = ReconciliationTextHints.Party("TRF : promar", null, null);
        var treasury = ReconciliationTextHints.Party("PROMAR | 271", null, null);

        ReconciliationTextHints.PartiesOverlap(bank, treasury).Should().BeTrue();
    }

    [Fact]
    public void PartiesOverlap_AmountOnlyCoincidence_ShouldNotMatch()
    {
        ReconciliationTextHints.PartiesOverlap("2BUSINESS", "REFORCO CAIXA").Should().BeFalse();
    }
}
