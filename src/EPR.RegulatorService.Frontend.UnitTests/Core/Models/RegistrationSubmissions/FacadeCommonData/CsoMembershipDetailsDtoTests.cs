namespace EPR.RegulatorService.Frontend.UnitTests.Core.Models.RegistrationSubmissions.FacadeCommonData
{
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Models.RegistrationSubmissions.FacadeCommonData;

    [TestClass]
    public class CsoMembershipDetailsDtoTests
    {
        [TestMethod]
        public void CsoMembershipDetailsDto_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var dto = new CsoMembershipDetailsDto();

            // Assert
            Assert.IsNull(dto.MemberId);
            Assert.IsNull(dto.MemberType);
            Assert.IsFalse(dto.IsOnlineMarketPlace);
            Assert.IsFalse(dto.IsLateFeeApplicable);
            Assert.AreEqual(0, dto.NumberOfSubsidiaries);
            Assert.AreEqual(0, dto.NoOfSubsidiariesOnlineMarketplace);
            Assert.AreEqual(0, dto.RelevantYear);
            Assert.AreEqual(default(DateTime), dto.SubmittedDate);
            Assert.IsNull(dto.SubmissionPeriodDescription);
        }

        [TestMethod]
        public void CsoMembershipDetailsDto_PropertyAssignments_ShouldBePersisted()
        {
            // Arrange
            var memberId = "M12345";
            var memberType = "TypeA";
            var isOnlineMarketplace = true;
            var isLateFeeApplicable = true;
            var numberOfSubsidiaries = 5;
            var noOfSubsidiariesOnlineMarketplace = 2;
            var relevantYear = 2024;
            var submittedDate = DateTime.Now;
            var submissionPeriodDescription = "Period 1";

            // Act
            var dto = new CsoMembershipDetailsDto
            {
                MemberId = memberId,
                MemberType = memberType,
                IsOnlineMarketPlace = isOnlineMarketplace,
                IsLateFeeApplicable = isLateFeeApplicable,
                NumberOfSubsidiaries = numberOfSubsidiaries,
                NoOfSubsidiariesOnlineMarketplace = noOfSubsidiariesOnlineMarketplace,
                RelevantYear = relevantYear,
                SubmittedDate = submittedDate,
                SubmissionPeriodDescription = submissionPeriodDescription
            };

            // Assert
            Assert.AreEqual(memberId, dto.MemberId);
            Assert.AreEqual(memberType, dto.MemberType);
            Assert.AreEqual(isOnlineMarketplace, dto.IsOnlineMarketPlace);
            Assert.AreEqual(isLateFeeApplicable, dto.IsLateFeeApplicable);
            Assert.AreEqual(numberOfSubsidiaries, dto.NumberOfSubsidiaries);
            Assert.AreEqual(noOfSubsidiariesOnlineMarketplace, dto.NoOfSubsidiariesOnlineMarketplace);
            Assert.AreEqual(relevantYear, dto.RelevantYear);
            Assert.AreEqual(submittedDate, dto.SubmittedDate);
            Assert.AreEqual(submissionPeriodDescription, dto.SubmissionPeriodDescription);
        }

        [TestMethod]
        public void CsoMembershipDetailsDto_EdgeCases_ShouldBeHandledCorrectly()
        {
            // Arrange
            var dto = new CsoMembershipDetailsDto
            {
                MemberId = string.Empty,
                MemberType = string.Empty,
                NumberOfSubsidiaries = int.MinValue,
                NoOfSubsidiariesOnlineMarketplace = int.MaxValue,
                RelevantYear = int.MaxValue,
                SubmittedDate = DateTime.MaxValue,
                SubmissionPeriodDescription = string.Empty
            };

            // Assert
            Assert.AreEqual(string.Empty, dto.MemberId);
            Assert.AreEqual(string.Empty, dto.MemberType);
            Assert.AreEqual(int.MinValue, dto.NumberOfSubsidiaries);
            Assert.AreEqual(int.MaxValue, dto.NoOfSubsidiariesOnlineMarketplace);
            Assert.AreEqual(int.MaxValue, dto.RelevantYear);
            Assert.AreEqual(DateTime.MaxValue, dto.SubmittedDate);
            Assert.AreEqual(string.Empty, dto.SubmissionPeriodDescription);
        }

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
            Assert.AreEqual(dto.NumberOfSubsidiaries, request.NoOfSubsidiariesOnlineMarketplace); // Maps dto.NumberOfSubsidiaries to NoOfSubsidiariesOnlineMarketplace
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
            Assert.AreEqual(dto.NumberOfSubsidiaries, request.NoOfSubsidiariesOnlineMarketplace);
        }

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
        }
    }
}
