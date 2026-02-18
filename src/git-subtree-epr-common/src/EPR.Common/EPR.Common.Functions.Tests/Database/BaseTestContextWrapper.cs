namespace EPR.Common.Functions.Test.Database;

using EPR.Common.Functions.Services.Interfaces;
using Functions.AccessControl;
using Functions.AccessControl.Interfaces;
using Functions.Database.Decorators;
using Functions.Database.Decorators.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Services;
using System;
using System.Collections.Generic;

internal abstract class BaseTestContextWrapper
{
    private readonly IAuthenticator authenticator;
    private Guid userProfileId;
    private string emailAddress;
    private DateTime utcNow;

    public BaseTestContextWrapper()
    {
        this.RequestTimeService = Substitute.For<IRequestTimeService>();
        this.UtcNow = DateTime.UtcNow;

        this.ContextAdminOverride = new ContextAdminOverride(Substitute.For<ILogger<ContextAdminOverride>>());

        this.authenticator = Substitute.For<IAuthenticator>();
        this.UserId = Guid.NewGuid();
        this.EmailAddress = "john.doe@here.com";

        this.UserContextProvider = new UserContextProvider(this.authenticator, this.ContextAdminOverride);
        this.EntityDecorators = new IEntityDecorator[] { new CreatedUpdatedDecorator(this.RequestTimeService) };
    }

    public ContextAdminOverride ContextAdminOverride { get; }

    public Guid UserId
    {
        get
        {
            return this.userProfileId;
        }

        set
        {
            this.userProfileId = value;
            this.authenticator.UserId.Returns(value);
        }
    }

    public string EmailAddress
    {
        get
        {
            return this.emailAddress;
        }

        set
        {
            this.emailAddress = value;
            this.authenticator.EmailAddress.Returns(value);
        }
    }

    public DateTime UtcNow
    {
        get
        {
            return this.utcNow;
        }

        set
        {
            this.utcNow = value;
            this.RequestTimeService.UtcRequest.Returns(value);
        }
    }

    protected IEnumerable<IEntityDecorator> EntityDecorators { get; }

    protected IRequestTimeService RequestTimeService { get; }

    protected IUserContextProvider UserContextProvider { get; }
}
