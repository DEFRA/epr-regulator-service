namespace EPR.Common.Functions.Test.AccessControl;

using FluentAssertions;
using Functions.AccessControl;
using Functions.AccessControl.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Services;

[TestClass]
public class UserContextProviderTests
{
    private IUserContextProvider userContextProvider;
    private ContextAdminOverride contextAdminOverride;

    private Guid userProfileId;
    private string userEmailAddress;
    private Guid userCustomerOrganisationId;
    private Guid userCustomerId;

    public UserContextProviderTests()
    {
        this.userProfileId = Guid.NewGuid();
        this.userEmailAddress = "john.doe@here.com";
        this.userCustomerOrganisationId = Guid.NewGuid();
        this.userCustomerId = Guid.NewGuid();
        var authenticator = Substitute.For<IAuthenticator>();
        authenticator.UserId.Returns(this.userProfileId);
        authenticator.EmailAddress.Returns(this.userEmailAddress);
        authenticator.CustomerOrganisationId.Returns(this.userCustomerOrganisationId);
        authenticator.CustomerId.Returns(this.userCustomerId);

        this.contextAdminOverride = new ContextAdminOverride(Substitute.For<ILogger<ContextAdminOverride>>());

        this.userContextProvider = new UserContextProvider(authenticator, this.contextAdminOverride);
    }

    [TestMethod]
    public void UserId_WhenNotOverridden_ReturnsAuthenticatorUserProfileId()
    {
        this.userContextProvider.UserId.Should().Be(this.userProfileId);
    }

    [TestMethod]
    public void UserId_WhenOverridden_ReturnsOverriddenUserProfileId()
    {
        var newGuid = Guid.NewGuid();
        using (this.contextAdminOverride.OverrideContext(newGuid, string.Empty, Guid.Empty, Guid.Empty))
        {
            this.userContextProvider.UserId.Should().Be(newGuid);
            this.contextAdminOverride.IsOverridden.Should().BeTrue();
        }
    }

    [TestMethod]
    public void UserId_AfterOverride_ReturnsAuthenticatorUserProfileId()
    {
        var newGuid = Guid.NewGuid();
        using (this.contextAdminOverride.OverrideContext(newGuid, string.Empty, Guid.Empty, Guid.Empty))
        {
        }

        this.userContextProvider.UserId.Should().Be(this.userProfileId);
        this.contextAdminOverride.IsOverridden.Should().BeFalse();
    }

    [TestMethod]
    public void EmailAddress_WhenNotOverridden_ReturnsAuthenticatorUserEmailAddress()
    {
        this.userContextProvider.EmailAddress.Should().Be(this.userEmailAddress);
    }

    [TestMethod]
    public void EmailAddress_WhenOverridden_ReturnsOverriddenUserEmailAddress()
    {
        var emailAddress = "jane.doe@here.com";
        using (this.contextAdminOverride.OverrideContext(Guid.Empty, emailAddress, Guid.Empty, Guid.Empty))
        {
            this.userContextProvider.EmailAddress.Should().Be(emailAddress);
            this.contextAdminOverride.IsOverridden.Should().BeTrue();
        }
    }

    [TestMethod]
    public void EmailAddress_AfterOverride_ReturnsAuthenticatorUserEmailAddress()
    {
        var emailAddress = "jane.doe@here.com";
        using (this.contextAdminOverride.OverrideContext(Guid.Empty, emailAddress, Guid.Empty, Guid.Empty))
        {
        }

        this.userContextProvider.EmailAddress.Should().Be(this.userEmailAddress);
        this.contextAdminOverride.IsOverridden.Should().BeFalse();
    }

    [TestMethod]
    public void CustomerOrganisationId_WhenNotOverridden_ReturnsAuthenticatorUserCustomerOrganisationId()
    {
        this.userContextProvider.CustomerOrganisationId.Should().Be(this.userCustomerOrganisationId);
    }

    [TestMethod]
    public void CustomerOrganisationId_WhenOverridden_ReturnsOverriddenUserCustomerOrganisationId()
    {
        var customerOrganisationId = Guid.NewGuid();
        using (this.contextAdminOverride.OverrideContext(Guid.Empty, string.Empty, customerOrganisationId, Guid.Empty))
        {
            this.userContextProvider.CustomerOrganisationId.Should().Be(customerOrganisationId);
            this.contextAdminOverride.IsOverridden.Should().BeTrue();
        }
    }

    [TestMethod]
    public void CustomerOrganisationId_AfterOverride_ReturnsAuthenticatorUserCustomerOrganisationId()
    {
        var customerOrganisationId = Guid.NewGuid();
        using (this.contextAdminOverride.OverrideContext(Guid.Empty, string.Empty, customerOrganisationId, Guid.Empty))
        {
        }

        this.userContextProvider.CustomerOrganisationId.Should().Be(this.userCustomerOrganisationId);
        this.contextAdminOverride.IsOverridden.Should().BeFalse();
    }

    [TestMethod]
    public void CustomerId_WhenNotOverridden_ReturnsAuthenticatorUserCustomerId()
    {
        this.userContextProvider.CustomerId.Should().Be(this.userCustomerId);
    }

    [TestMethod]
    public void CustomerId_WhenOverridden_ReturnsOverriddenUserCustomerId()
    {
        var customerId = Guid.NewGuid();
        using (this.contextAdminOverride.OverrideContext(Guid.Empty, string.Empty, Guid.Empty, customerId))
        {
            this.userContextProvider.CustomerId.Should().Be(customerId);
            this.contextAdminOverride.IsOverridden.Should().BeTrue();
        }
    }

    [TestMethod]
    public void CustomerId_AfterOverride_ReturnsAuthenticatorUserCustomerId()
    {
        var customerId = Guid.NewGuid();
        using (this.contextAdminOverride.OverrideContext(Guid.Empty, string.Empty, Guid.Empty, customerId))
        {
        }

        this.userContextProvider.CustomerId.Should().Be(this.userCustomerId);
        this.contextAdminOverride.IsOverridden.Should().BeFalse();
    }
}