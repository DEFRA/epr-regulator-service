namespace EPR.RegulatorService.Frontend.UnitTests.Web.Attributes;

using System;
using System.Globalization;

using EPR.RegulatorService.Frontend.Web.Attributes;

[TestClass]
public class CurrencyValidationAttributeTests
{
    CurrencyValidationAttribute _sut;
    private const decimal MaxValue = 10000000.00M;

    [TestInitialize]
    public void Setup() => _sut = new(
        "requireMessage",
        "valueTooBig",
        "invalidFormat",
        MaxValue.ToString(CultureInfo.CurrentCulture),
        "specialChar",
        "nonNumeric");

    [TestMethod]
    public void CurrencyWillNotConstructWithBadMaxValue() =>
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new CurrencyValidationAttribute(
                "requireMessage",
                "valueTooBig",
                "invalidFormat",
                "baddecimal",
                "specialChar",
                "nonNumeric"));

    [TestMethod]
    public void CurrencyCannotBeANullString()
    {
        bool result = _sut.IsValid(null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CurrencyCanHaveAMinusSign()
    {
        bool result = _sut.IsValid("-100");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCanHaveNoDecimalValue()
    {
        bool result = _sut.IsValid("10");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCanHaveAPartialDecimal()
    {
        bool result = _sut.IsValid("100.4");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCanHave2DecimalPlaces()
    {
        bool result = _sut.IsValid("100.00");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCantHaveMoreThan2DecimalPlaces()
    {
        bool result = _sut.IsValid("100.414");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCanHaveThousandsSeparator()
    {
        bool result = _sut.IsValid("10,000");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCanHaveThousandsSeparatorAndAMinusSign()
    {
        bool result = _sut.IsValid("-10,000");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCanHaveThousandsSeparatorAndAMinusSignAnd1Decimal()
    {
        bool result = _sut.IsValid("-10,000.4");
        result.Should().BeTrue();
    }


    [TestMethod]
    public void CurrencyCanHaveThousandsSeparatorAndAMinusSignAnd2Decimals()
    {
        bool result = _sut.IsValid("-10,000.44");
        result.Should().BeTrue();
    }

    [TestMethod]
    public void CurrencyCanHaveThousandsSeparatorAndAMinusSignAndNotMoreThan2Decimals()
    {
        bool result = _sut.IsValid("-10,000.44534");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCanHaveABadPlaceThousandsSeparator()
    {
        bool result = _sut.IsValid("10,00.10");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantHaveMoreThanOneDecimalPoint()
    {
        bool result = _sut.IsValid("200..00");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantExceedHighCharacterCount()
    {
        bool result = _sut.IsValid("-10,000,000,000.00");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantExceedSuppliedValue()
    {
        bool result = _sut.IsValid((MaxValue + 100.00M).ToString(CultureInfo.CurrentCulture));
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantbeBlank()
    {
        bool result = _sut.IsValid("");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantContainChars()
    {
        bool result = _sut.IsValid("122abc");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantContainSpecialChars()
    {
        bool result = _sut.IsValid("!43");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyWontTestSpecialChars()
    {
        _sut = new("requireMessage", "valueTooBig", "invalidFormat", MaxValue.ToString(CultureInfo.CurrentCulture), "", "nonNumeric");
        bool result = _sut.IsValid("!43");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyWontTestAlphaChars()
    {
        _sut = new("requireMessage", "valueTooBig", "invalidFormat", MaxValue.ToString(CultureInfo.CurrentCulture), "specialChar", "");
        bool result = _sut.IsValid("abc");
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyValueCannotBeZero()
    {
        bool result = _sut.IsValid("0");
        result.Should().BeFalse();
    }
}
