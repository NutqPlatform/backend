namespace Nutq.Core.Commands
{
    public record DoctorRegisterCommand(
        string Name,
        string Email,
        string Password,
        string InvitationCode
    );
}
