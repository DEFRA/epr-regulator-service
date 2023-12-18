namespace EPR.RegulatorService.Frontend.Core.Models
{
    public class EnrolInvitedUserRequest
    {
        public string InviteToken { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}
