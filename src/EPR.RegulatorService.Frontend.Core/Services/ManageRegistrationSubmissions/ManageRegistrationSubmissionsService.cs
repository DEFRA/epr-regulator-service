namespace EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.Models.ManageRegistrationSubmissions;
    using EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces;

    public class ManageRegistrationSubmissionsService : IManageRegistrationSubmissionsService
    {
        public Task<RegistrationSubmissionsModel> GetRegistrationSubmissionsAsync(int? pageNumber) => throw new NotImplementedException();
    }
}
