namespace EPR.Common.Authorization.Test.Extensions;

using System.Security.Claims;
using System.Text.Json;
using Authorization.Extensions;
using Constants;
using Microsoft.Identity.Web;
using Models;

[TestClass]
public class ClaimsPrincipleExtensionsTests
{
    private ClaimsPrincipal? _claimsPrinciple;
    private ClaimsIdentity? _claimsIdentity;

    [TestInitialize]
    public void Setup()
    {
        _claimsIdentity = new ClaimsIdentity();
        _claimsPrinciple = new ClaimsPrincipal(_claimsIdentity);
    }

    [TestMethod]
    public void AddOrUpdateUserData_AddsUserData_WhenNoExistingClaim()
    {
        var userData = new UserData { Id = Guid.NewGuid(), ServiceRole = ServiceRoles.ApprovedPerson };

        _claimsPrinciple?.AddOrUpdateUserData(userData);

        var claim = _claimsIdentity?.FindFirst(ClaimTypes.UserData);
        Assert.IsNotNull(claim);

        var deserializedUserData = JsonSerializer.Deserialize<UserData>(claim.Value);
        Assert.AreEqual(deserializedUserData?.Id, userData.Id);
        Assert.AreEqual(deserializedUserData?.RoleInOrganisation, userData.RoleInOrganisation);
    }

    [TestMethod]
    public void AddOrUpdateUserData_UpdatesUserData_WhenExistingClaim()
    {
        var initialUserData = new UserData { Id = Guid.NewGuid(), ServiceRole = ServiceRoles.ApprovedPerson };
        _claimsIdentity?.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(initialUserData)));

        var updatedUserData = new UserData { Id = Guid.NewGuid(), RoleInOrganisation = RoleInOrganisation.Employee };
        _claimsPrinciple?.AddOrUpdateUserData(updatedUserData);

        var claim = _claimsIdentity?.FindFirst(ClaimTypes.UserData);
        Assert.IsNotNull(claim);

        var deserializedUserData = JsonSerializer.Deserialize<UserData>(claim.Value);
        Assert.AreEqual(deserializedUserData?.Id, updatedUserData.Id);
        Assert.AreEqual(deserializedUserData?.RoleInOrganisation, updatedUserData.RoleInOrganisation);
    }

    [TestMethod]
    public void UserId_Returns_Correct_Guid()
    {
        // Arrange
        var objectId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimConstants.ObjectId, objectId.ToString())
        }));

        // Act
        var result = user.UserId();

        // Assert
        Assert.AreEqual(result, objectId);
    }

    [TestMethod]
    public void UserId_Throws_Exception_When_ObjectId_Claim_Missing()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act + Assert
        var ex = Assert.ThrowsException<InvalidOperationException>(() => user.UserId());
        Assert.AreEqual(ex?.Message, "Sequence contains no matching element");
    }

    [TestMethod]
    public void GetUserData_ReturnsUserData_WhenExistingClaim()
    {
        // Arrange
        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            ServiceRole = ServiceRoles.ApprovedPerson,
            FirstName = "John",
            LastName = "Smith",
            Organisations = new List<Organisation>()
            {
                new() { Name = "Org1" }
            }
        };
        _claimsIdentity?.AddClaim(new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)));

        // Act
        var result = _claimsPrinciple?.GetUserData();

        // Assert
        Assert.AreEqual(userData.Id, result.Id);
        Assert.AreEqual(userData.ServiceRole, result.ServiceRole);
        Assert.AreEqual(userData.FirstName, result.FirstName);
        Assert.AreEqual(userData.LastName, result.LastName);
        Assert.AreEqual(userData.Organisations.First().Name, result.Organisations.First().Name);
    }

    [TestMethod]
    public void GetUserData_ReturnsDefault_WhenNoUserDataClaimExists()
    {
        // Act
        var result = _claimsPrinciple?.GetUserData();

        // Assert
        Assert.IsNull(result);
    }
}