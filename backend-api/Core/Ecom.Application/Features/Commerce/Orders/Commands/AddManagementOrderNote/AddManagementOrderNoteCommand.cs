using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Orders.Commands.AddManagementOrderNote;

public sealed record AddManagementOrderNoteCommand(Guid OrderId, string Content) : IRequest<TResult<ManagementOrderNoteDto>>, ITransactionalRequest;

public sealed class AddManagementOrderNoteCommandValidator : AbstractValidator<AddManagementOrderNoteCommand>
{
    public AddManagementOrderNoteCommandValidator() { RuleFor(x => x.OrderId).NotEmpty(); RuleFor(x => x.Content).NotEmpty().MaximumLength(4000); }
}

public sealed class AddManagementOrderNoteCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    : IRequestHandler<AddManagementOrderNoteCommand, TResult<ManagementOrderNoteDto>>
{
    public async Task<TResult<ManagementOrderNoteDto>> Handle(AddManagementOrderNoteCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated) return TResult<ManagementOrderNoteDto>.Failure(MessageKey.Unauthorized, ErrorCodes.UNAUTHORIZED);
        if (!currentUser.HasPolicy(Permissions.Orders.Manage)) return TResult<ManagementOrderNoteDto>.Failure(MessageKey.Forbidden, ErrorCodes.FORBIDDEN);
        if (!await unitOfWork.Repository<Order>().AnyAsync([x => x.Id == request.OrderId])) return TResult<ManagementOrderNoteDto>.Failure(MessageKey.ResourceNotFound, ErrorCodes.NOT_FOUND);
        var note = OrderNote.CreateInternal(request.OrderId, currentUser.UserId, request.Content);
        await unitOfWork.Repository<OrderNote>().InsertAsync(note, ct);
        return TResult<ManagementOrderNoteDto>.Success(new(note.Id, note.AuthorUserId, note.NoteType, note.Content, note.IsVisibleToCustomer, note.CreatedAt));
    }
}
