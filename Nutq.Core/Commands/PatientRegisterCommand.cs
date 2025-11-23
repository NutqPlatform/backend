namespace Nutq.Core.Commands
{
    public record PatientRegisterCommand(
        string Name,
        string Email,
        string Password,
        int Age,
        string InvitationCode
    );
}
