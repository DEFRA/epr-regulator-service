
namespace EPR.RegulatorService.Frontend.UnitTests.Core.Models.RegistrationSubmissions.FacadeCommonData;

[TestClass]
public class CsoMembershipDetailsDtoTests
{
    [TestMethod]
    public void ImplicitOperator_ShouldMapPropertiesCorrectly()
    {
        // Arrange
        var dto = new CsoMembershipDetailsDto
        {
            MemberId = "M12345",
            MemberType = "Large",
            IsOnlineMarketPlace = true,
            IsLateFeeApplicable = false,
            NumberOfSubsidiaries = 5,
            NumberOfLateSubsidiaries = 3,
            NoOfSubsidiariesOnlineMarketplace = 2,
            RelevantYear = 2024,
            SubmittedDate = DateTime.Now,
            SubmissionPeriodDescription = "Period 1"
        };

        // Act
        var request = (ComplianceSchemeMemberRequest)dto;

        // Assert
        Assert.AreEqual(dto.MemberId, request.MemberId);
        Assert.AreEqual(dto.MemberType, request.MemberType);
        Assert.AreEqual(dto.IsOnlineMarketPlace, request.IsOnlineMarketplace);
        Assert.AreEqual(dto.IsLateFeeApplicable, request.IsLateFeeApplicable);
        Assert.AreEqual(dto.NumberOfSubsidiaries, request.NumberOfSubsidiaries);
        Assert.AreEqual(dto.NumberOfLateSubsidiaries, request.NumberOfLateSubsidiaries);
        Assert.AreEqual(dto.NoOfSubsidiariesOnlineMarketplace, request.NoOfSubsidiariesOnlineMarketplace);
    }

    [TestMethod]
    public void ImplicitOperator_ShouldHandleEmptyDtoValuesCorrectly()
    {
        // Arrange
        var dto = new CsoMembershipDetailsDto
        {
            MemberId = string.Empty,
            MemberType = string.Empty,
            IsOnlineMarketPlace = false,
            IsLateFeeApplicable = false,
            NumberOfSubsidiaries = 0,
            NumberOfLateSubsidiaries = 0,
            NoOfSubsidiariesOnlineMarketplace = 0,
            RelevantYear = 0,
            SubmittedDate = default,
            SubmissionPeriodDescription = string.Empty
        };

        // Act
        var request = (ComplianceSchemeMemberRequest)dto;

        // Assert
        Assert.AreEqual(dto.MemberId, request.MemberId);
        Assert.AreEqual(dto.MemberType, request.MemberType);
        Assert.AreEqual(dto.IsOnlineMarketPlace, request.IsOnlineMarketplace);
        Assert.AreEqual(dto.IsLateFeeApplicable, request.IsLateFeeApplicable);
        Assert.AreEqual(dto.NumberOfSubsidiaries, request.NumberOfSubsidiaries);
        Assert.AreEqual(dto.NumberOfLateSubsidiaries, request.NumberOfLateSubsidiaries);
        Assert.AreEqual(dto.NumberOfSubsidiaries, request.NoOfSubsidiariesOnlineMarketplace);
    }

    // not sure there's much value in this test
    [TestMethod]
    public void ImplicitOperator_ShouldHandleNullDtoValues()
    {
        // Arrange
        var dto = new CsoMembershipDetailsDto
        {
            MemberId = null,
            MemberType = null,
            IsOnlineMarketPlace = false,
            IsLateFeeApplicable = false,
            NumberOfSubsidiaries = 0,
            NumberOfLateSubsidiaries = 0,
            NoOfSubsidiariesOnlineMarketplace = 0,
            RelevantYear = 0,
            SubmittedDate = default,
            SubmissionPeriodDescription = null
        };

        // Act
        var request = (ComplianceSchemeMemberRequest)dto;

        // Assert
        Assert.IsNull(request.MemberId);
        Assert.IsNull(request.MemberType);
        Assert.AreEqual(dto.IsOnlineMarketPlace, request.IsOnlineMarketplace);
        Assert.AreEqual(dto.IsLateFeeApplicable, request.IsLateFeeApplicable);
        Assert.AreEqual(dto.NumberOfSubsidiaries, request.NumberOfSubsidiaries);
        Assert.AreEqual(dto.NumberOfSubsidiaries, request.NoOfSubsidiariesOnlineMarketplace);
        Assert.AreEqual(dto.NumberOfLateSubsidiaries, request.NumberOfLateSubsidiaries);
    }
}