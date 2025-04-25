namespace EPR.RegulatorService.Frontend.Web.ViewModels.RegistrationSubmissions
{
    public class WaveFeesViewModel
    {
        public decimal ApplicationFee { get; set; }

        public bool ApplicationFeeSectionEnable => ApplicationFee > 0;

        public decimal WavedApplicationFee { get; set; }

        public bool SchemeMembersSectionEnable { get; set; }

        public int SmallProducerCount { get; set; }

        public bool SmalProducerSectionEnable => SmallProducerCount > 0 || SmallProducerFee > 0;

        public decimal SmallProducerFee { get; set; }

        public decimal WavedSmallProducerFee { get; set; }

        public int LargeProducerCount { get; set; }

        public bool LargeProducerSectionEnable => LargeProducerCount > 0 || LargeProducerFee > 0;

        public decimal LargeProducerFee { get; set; }

        public decimal WavedLargeProdcuerFee { get; set; }

        public int LateProducerCount { get; set; }

        public bool LateProducerSectionEnable => LateProducerCount > 0 || LateProducerFee > 0;

        public decimal LateProducerFee { get; set; }

        public decimal WavedLateProducerFee { get; set; }

        public int OnlineMarketPlaceCount { get; set; }

        public bool OnlineMarketPlaceSectionEnable => OnlineMarketPlaceCount > 0 || OnlineMarketPlaceFee > 0;

        public decimal OnlineMarketPlaceFee { get; set; }

        public decimal WavedOnlineMarketPlaceFee { get; set; }

        public int SubsidiariesCompanyCount { get; set; }

        public bool SubsidiariesCompanySectionEnable => SubsidiariesCompanyCount > 0 || SubsidiariesCompanyFee > 0;

        public decimal SubsidiariesCompanyFee { get; set; }

        public decimal WavedSubsidiariesCompanyFee { get; set; }

        public bool IsComplianceSchemeSelected { get; set; }

        public bool IsProducerSelected { get; set; }
    }
}
