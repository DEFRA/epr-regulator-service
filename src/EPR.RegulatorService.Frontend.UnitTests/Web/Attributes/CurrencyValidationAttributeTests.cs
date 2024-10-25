namespace EPR.RegulatorService.Frontend.UnitTests.Web.Attributes;

using System;

using EPR.RegulatorService.Frontend.Web.Attributes;

[TestClass]
public class CurrencyValidationAttributeTests
{
    CurrencyValidationAttribute _sut;
    private const string MaxValue = "100000000000";

    [TestInitialize]
    public void Setup()
    {
        _sut = new("requireMessage", "valueTooBig", "invalidFormat", MaxValue);
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
    public void CurrencyCantExceedSuppliedValue() {
        bool result = _sut.IsValid((decimal.Parse(MaxValue) + 100.00M).ToString());
        result.Should().BeFalse();
    }

    [TestMethod]
    public void CurrencyCantbeBlank()
    {
        bool result = _sut.IsValid("");
        result.Should().BeFalse();
    }
}
