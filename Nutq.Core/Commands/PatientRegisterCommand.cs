namespace Nutq.Core.Commands
{
    public record PatientRegisterCommand(
        string Name,
        string Email,
        string Password,
        DateTime? DateOfBirth,
        string InvitationCode,
        string? PhoneNumber
    );
}
