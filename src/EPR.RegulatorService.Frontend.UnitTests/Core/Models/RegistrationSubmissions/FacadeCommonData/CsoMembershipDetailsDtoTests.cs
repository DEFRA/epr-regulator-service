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
            Assert.IsFalse(dto.IsClosedLoopRecycling);
            Assert.AreEqual(0, dto.NumberOfSubsidiaries);
            Assert.AreEqual(0, dto.NumberOfSubsidiariesClosedLoopRecycling);
            Assert.AreEqual(0, dto.NumberOfSubsidiariesOnlineMarketPlace);
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
            var isClosedLoopRecycling = true;
            var numberOfSubsidiaries = 5;
            var noOfSubsidiariesClosedLoopRecycling = 3;
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
                IsClosedLoopRecycling = isClosedLoopRecycling,
                NumberOfSubsidiaries = numberOfSubsidiaries,
                NumberOfSubsidiariesClosedLoopRecycling = noOfSubsidiariesClosedLoopRecycling,
                NumberOfSubsidiariesOnlineMarketPlace = noOfSubsidiariesOnlineMarketplace,
                RelevantYear = relevantYear,
                SubmittedDate = submittedDate,
                SubmissionPeriodDescription = submissionPeriodDescription
            };

            // Assert
            Assert.AreEqual(memberId, dto.MemberId);
            Assert.AreEqual(memberType, dto.MemberType);
            Assert.AreEqual(isOnlineMarketplace, dto.IsOnlineMarketPlace);
            Assert.AreEqual(isLateFeeApplicable, dto.IsLateFeeApplicable);
            Assert.AreEqual(isClosedLoopRecycling, dto.IsClosedLoopRecycling);
            Assert.AreEqual(numberOfSubsidiaries, dto.NumberOfSubsidiaries);
            Assert.AreEqual(noOfSubsidiariesClosedLoopRecycling, dto.NumberOfSubsidiariesClosedLoopRecycling);
            Assert.AreEqual(noOfSubsidiariesOnlineMarketplace, dto.NumberOfSubsidiariesOnlineMarketPlace);
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
                NumberOfSubsidiariesClosedLoopRecycling = int.MaxValue,
                NumberOfSubsidiariesOnlineMarketPlace = int.MaxValue,
                RelevantYear = int.MaxValue,
                SubmittedDate = DateTime.MaxValue,
                SubmissionPeriodDescription = string.Empty
            };

            // Assert
            Assert.AreEqual(string.Empty, dto.MemberId);
            Assert.AreEqual(string.Empty, dto.MemberType);
            Assert.AreEqual(int.MinValue, dto.NumberOfSubsidiaries);
            Assert.AreEqual(int.MaxValue, dto.NumberOfSubsidiariesClosedLoopRecycling);
            Assert.AreEqual(int.MaxValue, dto.NumberOfSubsidiariesOnlineMarketPlace);
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
                IsClosedLoopRecycling = true,
                NumberOfSubsidiaries = 5,
                NumberOfSubsidiariesClosedLoopRecycling = 4,
                NumberOfSubsidiariesOnlineMarketPlace = 2,
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
            Assert.AreEqual(dto.IsClosedLoopRecycling, request.IsClosedLoopRecycling);
            Assert.AreEqual(dto.NumberOfSubsidiaries, request.NumberOfSubsidiaries);
            Assert.AreEqual(dto.NumberOfSubsidiariesClosedLoopRecycling, request.NumberOfSubsidiariesClosedLoopRecycling);
            Assert.AreEqual(dto.NumberOfSubsidiariesOnlineMarketPlace, request.NoOfSubsidiariesOnlineMarketplace);
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
                IsClosedLoopRecycling = false,
                NumberOfSubsidiaries = 0,
                NumberOfSubsidiariesOnlineMarketPlace = 0,
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
            Assert.AreEqual(dto.IsClosedLoopRecycling, request.IsClosedLoopRecycling);
            Assert.AreEqual(dto.NumberOfSubsidiaries, request.NumberOfSubsidiaries);
            Assert.AreEqual(dto.NumberOfSubsidiariesClosedLoopRecycling, request.NumberOfSubsidiariesClosedLoopRecycling);
            Assert.AreEqual(dto.NumberOfSubsidiariesOnlineMarketPlace, request.NoOfSubsidiariesOnlineMarketplace);
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
                IsClosedLoopRecycling = false,
                NumberOfSubsidiaries = 0,
                NumberOfSubsidiariesOnlineMarketPlace = 0,
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
            Assert.AreEqual(dto.IsClosedLoopRecycling, request.IsClosedLoopRecycling);
            Assert.AreEqual(dto.NumberOfSubsidiaries, request.NumberOfSubsidiaries);
            Assert.AreEqual(dto.NumberOfSubsidiariesClosedLoopRecycling, request.NumberOfSubsidiariesClosedLoopRecycling);
            Assert.AreEqual(dto.NumberOfSubsidiariesOnlineMarketPlace, request.NoOfSubsidiariesOnlineMarketplace);
        }
    }
}
