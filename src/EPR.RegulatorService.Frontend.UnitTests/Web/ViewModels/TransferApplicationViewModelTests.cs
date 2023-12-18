using EPR.RegulatorService.Frontend.UnitTests.Constants;
using EPR.RegulatorService.Frontend.Core.Models;
using EPR.RegulatorService.Frontend.Web.ViewModels.Applications;
using System.ComponentModel.DataAnnotations;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.ViewModels
{
    [TestClass]
    public class TransferApplicationViewModelTests
    {
        private readonly TransferApplicationViewModel _model = new();
        
        [TestMethod]
        [DataRow(TransferDetails.SelectedNationIdScotland,TransferDetails.TransferDetailsString, TransferDetails.ScotlandAgencyName)]
        [DataRow(TransferDetails.SelectedNationIdNorthernIreland,TransferDetails.TransferDetailsString, TransferDetails.NorthernIrelandAgencyName)]
        [DataRow(TransferDetails.SelectedNationIdWales,TransferDetails.TransferDetailsString, TransferDetails.WalesAgencyName)]
        [DataRow(TransferDetails.SelectedNationIdEngland,TransferDetails.TransferDetailsString, TransferDetails.EnglandAgencyName)]
        public Task Given_AgencyIndexSet_And_NationSelected_And_NotesSet_Then_SuccessReturned(int selectedNationId, string notes, string agency)
        {
            _model.AgencyIndex = 0;
            _model.TransferNotes = new List<TransferNote>
            {
                new()
                {
                    AgencyName = agency,
                    Notes = notes,
                    NationId = selectedNationId
                }
            };
            
            var context = new ValidationContext(_model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(_model, context, results, true);

            results.Count.Should().Be(0);
            isValid.Should().Be(true);
            
            return Task.CompletedTask;
        }
        
        [TestMethod]
        public Task Given_NoAgencyIndexSet_Then_ErrorReturned()
        {
            _model.AgencyIndex = null;
            _model.TransferNotes = new List<TransferNote>
            {
                new()
                {
                    AgencyName = null,
                    Notes = null,
                    NationId = 0
                }
            };
            
            var context = new ValidationContext(_model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(_model, context, results, true);
            
            isValid.Should().Be(false);
            results.Exists(x => x.ErrorMessage == TransferDetails.ModelErrorValueNoAgencyIndexSelected).Should().Be(true);
            
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Given_AgencyIndexSet_And_TransferNotesNotSet_Then_ErrorReturned()
        {
            _model.AgencyIndex = 0;
            _model.TransferNotes = new List<TransferNote>
            {
                new()
                {
                    AgencyName = TransferDetails.NorthernIrelandAgencyName,
                    Notes = null,
                    NationId = TransferDetails.SelectedNationIdNorthernIreland
                }
            };
            
            var context = new ValidationContext(_model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(_model, context, results, true);
            
            isValid.Should().Be(false);
            results.Exists(x => x.ErrorMessage == TransferDetails.ModelErrorValueNoTransferDetails).Should().Be(true);
            
            return Task.CompletedTask;
        }
        
        [TestMethod]
        public Task Given_AgencyIndexSet_And_TransferNotesTooLong_Then_ErrorReturned()
        {
            _model.AgencyIndex = 0;
            _model.TransferNotes = new List<TransferNote>
            {
                new()
                {
                    AgencyName = TransferDetails.NorthernIrelandAgencyName,
                    Notes = TransferDetails.LongTransferDetailsString,
                    NationId = TransferDetails.SelectedNationIdNorthernIreland
                }
            };
            
            var context = new ValidationContext(_model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(_model, context, results, true);
            
            isValid.Should().Be(false);
            results.Exists(x => x.ErrorMessage == TransferDetails.ModelErrorValueSummaryTooLong).Should().Be(true);
            
            return Task.CompletedTask;
        }
    }
}

