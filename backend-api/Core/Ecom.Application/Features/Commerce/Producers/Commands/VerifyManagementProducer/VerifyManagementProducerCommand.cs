namespace Ecom.Application.Features.Commerce.Producers.Commands.VerifyManagementProducer;

using Ecom.Domain.Entities;

public sealed record VerifyManagementProducerCommand(Guid ProducerId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProducerManagementResult>>, ITransactionalRequest;

public sealed class VerifyManagementProducerCommandValidator : AbstractValidator<VerifyManagementProducerCommand>
{
    public VerifyManagementProducerCommandValidator() { RuleFor(x => x.ProducerId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}

public sealed class VerifyManagementProducerCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser, ProducerManagementService service)
    : IRequestHandler<VerifyManagementProducerCommand, TResult<ProducerManagementResult>>
{
    public async Task<TResult<ProducerManagementResult>> Handle(VerifyManagementProducerCommand request, CancellationToken ct)
    {
        var loaded = await service.LoadAsync(request.ProducerId, request.ConcurrencyStamp, Permissions.Producers.Verify, ct);
        if (!loaded.IsSuccess) return TResult<ProducerManagementResult>.Failure(loaded.Error!, loaded.ErrorCode);
        loaded.Data.Verify(currentUser.UserId, DateTime.UtcNow);
        loaded.Data.RenewConcurrencyStamp();
        await unitOfWork.Repository<Producer>().UpdateAsync(loaded.Data, ct);
        return TResult<ProducerManagementResult>.Success(ProducerManagementService.Result(loaded.Data));
    }
}
