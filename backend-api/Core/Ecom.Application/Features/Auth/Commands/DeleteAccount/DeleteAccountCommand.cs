namespace Ecom.Application.Features.Auth.Commands.DeleteAccount;

public class DeleteAccountCommand : IRequest<TResult<string>>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string> SelectedReasons { get; set; } = new();
    public string? OtherReasonNote { get; set; }
    public string OtpCode { get; set; } = string.Empty;
}
