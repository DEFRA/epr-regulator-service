namespace EPR.RegulatorService.Frontend.Web.Controllers.ReprocessorExporter.Accreditations
{ 
    using EPR.RegulatorService.Frontend.Core.Sessions;
    using EPR.RegulatorService.Frontend.Core.Sessions.ReprocessorExporter;
    using EPR.RegulatorService.Frontend.Web.Sessions;


    public abstract class AccreditationBaseController(ISessionManager<JourneySession> sessionManager,
    IConfiguration configuration) : ReprocessorExporterBaseController(sessionManager, configuration)
    {
        protected static void InitialiseAccreditationStatusSessionIfNotExists(JourneySession session, Guid accreditationId, int year)
        {
            if (session.ReprocessorExporterSession.AccreditationStatusSession != null &&
                session.ReprocessorExporterSession.AccreditationStatusSession!.AccreditationId == accreditationId &&
                session.ReprocessorExporterSession.AccreditationStatusSession!.Year == year)
            {
                return;
            }

            var accreditationStatusSession = new AccreditationStatusSession
            {
                AccreditationId = accreditationId,
                Year = year,
                OrganisationName = null!,
                MaterialName = null!
            };

            session.ReprocessorExporterSession.AccreditationStatusSession = accreditationStatusSession;
        }
    }
}
