using EPR.RegulatorService.Frontend.Core.Sessions;
using EPR.RegulatorService.Frontend.UnitTests.TestData;

using FluentAssertions;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Sessions;

[TestClass]
public class RegulatorSubmissionSessionTests
{
    [TestMethod]
    public void Given_Null_Submission_When_Call_GetSubmissionHashCode_Should_Retrun_ArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.ThrowsException<ArgumentNullException>(() => RegulatorSubmissionSession.GetSubmissionHashCode(null));
        Assert.AreEqual("submission", ex.ParamName); 

    }

    [TestMethod]
    public void Given_Valid_Submission_When_Call_GetSubmissionHashCode_Should_Retrun_ArgumentNullException()
    {
        // Act & Assert
        int hashCode = RegulatorSubmissionSession.GetSubmissionHashCode(TestSubmission.GetTestSubmission());
        hashCode.Should().NotBe(null);

    }

}
