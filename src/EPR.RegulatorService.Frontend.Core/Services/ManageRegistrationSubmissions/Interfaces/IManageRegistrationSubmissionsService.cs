namespace EPR.RegulatorService.Frontend.Core.Services.ManageRegistrationSubmissions.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using EPR.RegulatorService.Frontend.Core.Models.ManageRegistrationSubmissions;

    public interface IManageRegistrationSubmissionsService
    {
        public Task<RegistrationSubmissionsModel> GetRegistrationSubmissionsAsync(int? pageNumber);
    }
}
