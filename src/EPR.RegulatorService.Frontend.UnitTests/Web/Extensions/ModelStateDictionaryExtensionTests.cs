using EPR.RegulatorService.Frontend.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Extensions
{
    [TestClass]
    public class ModelStateDictionaryExtensionTests
    {
        [TestMethod]
        public void ToErrorDictionary_ConvertsModelStateToKeyValueErrorList()
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Name", "Name is required");
            modelState.AddModelError("Email", "Email is invalid");
            modelState.AddModelError("Email", "Email already exists");

            // Act
            var result = ModelStateDictionaryExtension.ToErrorDictionary(modelState,null);

            // Assert
            result.Should().HaveCount(2);

            result[0].Key.Equals("Email");
            result[0].Errors.Should().HaveCount(2);
            result[0].Errors[0].Message.Equals("Email is invalid");
            result[0].Errors[1].Message.Equals("Email already exists");

            result[1].Key.Equals("Name");
            result[1].Errors.Should().HaveCount(1);
            result[1].Errors[0].Message.Equals("Name is required");
        }

        [TestMethod]
        public void ToErrorDictionary_ConvertsModelStateToKeyValueErrorList_And_FilterTheList()
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Name", "Name is required");
            modelState.AddModelError("Email", "Email is invalid");
            modelState.AddModelError("Email", "Email already exists");

            // Act
            var result = ModelStateDictionaryExtension.ToErrorDictionary(modelState, ["Email"]);

            // Assert
            result.Should().HaveCount(1);

            result[0].Key.Equals("Email");
            result[0].Errors.Should().HaveCount(2);
            result[0].Errors[0].Message.Equals("Email is invalid");
            result[0].Errors[1].Message.Equals("Email already exists");

            result[0].Key.Should().NotBe("Name");
        }
    }
}
