using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Producers.Commands.HideManagementProducer;

public sealed record HideManagementProducerCommand(Guid ProducerId, Guid ConcurrencyStamp, string Reason)
    : IRequest<TResult<ProducerManagementResult>>, ITransactionalRequest;

public sealed class HideManagementProducerCommandValidator : AbstractValidator<HideManagementProducerCommand>
{
    public HideManagementProducerCommandValidator() { RuleFor(x => x.ProducerId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); }
}

public sealed class HideManagementProducerCommandHandler(IUnitOfWork unitOfWork, ProducerManagementService service)
    : IRequestHandler<HideManagementProducerCommand, TResult<ProducerManagementResult>>
{
    public async Task<TResult<ProducerManagementResult>> Handle(HideManagementProducerCommand request, CancellationToken ct)
    {
        var loaded = await service.LoadAsync(request.ProducerId, request.ConcurrencyStamp, Permissions.Producers.Publish, ct);
        if (!loaded.IsSuccess) return TResult<ProducerManagementResult>.Failure(loaded.Error!, loaded.ErrorCode);
        var hasPublishedProduct = await unitOfWork.Repository<Product>().AnyAsync([x => x.ProducerId == loaded.Data.Id && x.Status == ProductStatus.Published]);
        if (hasPublishedProduct) return TResult<ProducerManagementResult>.Failure("Unpublish or move products before hiding this producer.", ErrorCodes.UNPROCESSABLE_ENTITY);
        loaded.Data.Hide();
        loaded.Data.RenewConcurrencyStamp();
        await unitOfWork.Repository<Producer>().UpdateAsync(loaded.Data, ct);
        return TResult<ProducerManagementResult>.Success(ProducerManagementService.Result(loaded.Data));
    }
}
