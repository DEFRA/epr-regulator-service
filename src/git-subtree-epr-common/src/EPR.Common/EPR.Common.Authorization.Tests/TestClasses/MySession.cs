namespace EPR.Common.Authorization.Test.TestClasses;

using Interfaces;
using Models;

public class MySession : IHasUserData
{
    public MySession()
    {
    }

    public UserData UserData { get; set; }
    
    public string? OtherProperty { get; set; }
}