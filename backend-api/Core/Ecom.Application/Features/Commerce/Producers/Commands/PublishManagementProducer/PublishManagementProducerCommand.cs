namespace Ecom.Application.Features.Commerce.Producers.Commands.PublishManagementProducer;

using Ecom.Domain.Entities;

public sealed record PublishManagementProducerCommand(Guid ProducerId, Guid ConcurrencyStamp)
    : IRequest<TResult<ProducerManagementResult>>, ITransactionalRequest;

public sealed class PublishManagementProducerCommandValidator : AbstractValidator<PublishManagementProducerCommand>
{
    public PublishManagementProducerCommandValidator() { RuleFor(x => x.ProducerId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty(); }
}

public sealed class PublishManagementProducerCommandHandler(IUnitOfWork unitOfWork, ProducerManagementService service)
    : IRequestHandler<PublishManagementProducerCommand, TResult<ProducerManagementResult>>
{
    public async Task<TResult<ProducerManagementResult>> Handle(PublishManagementProducerCommand request, CancellationToken ct)
    {
        var loaded = await service.LoadAsync(request.ProducerId, request.ConcurrencyStamp, Permissions.Producers.Publish, ct);
        if (!loaded.IsSuccess) return TResult<ProducerManagementResult>.Failure(loaded.Error!, loaded.ErrorCode);
        loaded.Data.Publish();
        loaded.Data.RenewConcurrencyStamp();
        await unitOfWork.Repository<Producer>().UpdateAsync(loaded.Data, ct);
        return TResult<ProducerManagementResult>.Success(ProducerManagementService.Result(loaded.Data));
    }
}
