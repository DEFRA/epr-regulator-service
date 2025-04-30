using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using EPR.RegulatorService.Frontend.Core.Models.ReprocessorExporter.Registrations;
using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
using EPR.RegulatorService.Frontend.Web.Mappings;
using EPR.RegulatorService.Frontend.Web.ViewModels.ReprocessorExporter.Registrations;

namespace EPR.RegulatorService.Frontend.UnitTests.Web.Mappings
{
    [TestClass]
    public class RegistrationMaterialReprocessingIOMappingProfileTests
    {
        private IMapper _mapper;

        [TestInitialize]
        public void TestInitialize()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<RegistrationMaterialReprocessingIOMappingProfile>());
            _mapper = config.CreateMapper();
        }

        [TestMethod]
        public void Map_WhenCalledWithRegistrationMaterialReprocessingIO_ShouldReturnRegistrationMaterialReprocessingIOViewModel()
        {
            var registrationMaterialReprocessingIO = new RegistrationMaterialReprocessingIO            
            {
                MaterialName = "Plastic",
                SourcesOfPackagingWaste = "N/A",
                PlantEquipmentUsed = "N/A",
                ReprocessingPackagingWasteLastYearFlag = true,
                UKPackagingWasteTonne = 100,
                NonUKPackagingWasteTonne = 50,
                NotPackingWasteTonne = 10,
                SenttoOtherSiteTonne = 5,
                ContaminantsTonne = 2,
                ProcessLossTonne = 1,
                TotalInput = 100,
                TotalOutput = 95
            };

            var result = _mapper.Map<RegistrationMaterialReprocessingIOViewModel>(registrationMaterialReprocessingIO);

            result.Should().NotBeNull();
            result.MaterialName.Should().Be(registrationMaterialReprocessingIO.MaterialName);
            result.SourcesOfPackagingWaste.Should().Be(registrationMaterialReprocessingIO.SourcesOfPackagingWaste);
            result.PlantEquipmentUsed.Should().Be(registrationMaterialReprocessingIO.PlantEquipmentUsed);
            result.ReprocessingPackagingWasteLastYearFlag.Should().Be(registrationMaterialReprocessingIO.ReprocessingPackagingWasteLastYearFlag);
            result.UKPackagingWasteTonne.Should().Be(registrationMaterialReprocessingIO.UKPackagingWasteTonne);
            result.NonUKPackagingWasteTonne.Should().Be(registrationMaterialReprocessingIO.NonUKPackagingWasteTonne);
            result.NotPackingWasteTonne.Should().Be(registrationMaterialReprocessingIO.NotPackingWasteTonne);
            result.SenttoOtherSiteTonne.Should().Be(registrationMaterialReprocessingIO.SenttoOtherSiteTonne);
            result.ContaminantsTonne.Should().Be(registrationMaterialReprocessingIO.ContaminantsTonne);
            result.ProcessLossTonne.Should().Be(registrationMaterialReprocessingIO.ProcessLossTonne);
            result.TotalInput.Should().Be(registrationMaterialReprocessingIO.TotalInput);
            result.TotalOutput.Should().Be(registrationMaterialReprocessingIO.TotalOutput);
        }
    }
}
