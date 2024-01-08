using EPR.RegulatorService.Frontend.Core.Extensions;

namespace EPR.RegulatorService.Frontend.UnitTests.Core.Extensions;

[TestClass]
public class EnumExtensionMethods
{
    [TestMethod]
    public async Task GetDescription_EnumWithDescription_ConvertsSuccessfully()
    {
        string expectedDescription = "OPTION 111";

        string actualDescription = TestEnum.Option1.GetDescription();

        actualDescription.Should().Be(expectedDescription);
    }

    [TestMethod]
    public async Task GetDescription_EnumWithoutDescription_DisplaysOriginalEnum()
    {
        string expectedDescription = "Option2";

        string actualDescription = TestEnum.Option2.GetDescription();

        actualDescription.Should().Be(expectedDescription);
    }
}
