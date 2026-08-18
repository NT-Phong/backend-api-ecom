using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Producers.Commands.UpdateManagementProducer;

public sealed record UpdateManagementProducerCommand(Guid ProducerId, Guid ConcurrencyStamp, string Code, string Name,
    string? LegalName, string? Description, string? WebsiteUrl) : IRequest<TResult<ProducerManagementResult>>, ITransactionalRequest;

public sealed class UpdateManagementProducerCommandValidator : AbstractValidator<UpdateManagementProducerCommand>
{
    public UpdateManagementProducerCommandValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty(); RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50); RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.LegalName).MaximumLength(300); RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.WebsiteUrl).MaximumLength(500).Must(x => string.IsNullOrWhiteSpace(x) || Uri.TryCreate(x, UriKind.Absolute, out _));
    }
}

public sealed class UpdateManagementProducerCommandHandler(IUnitOfWork unitOfWork, ProducerManagementService service)
    : IRequestHandler<UpdateManagementProducerCommand, TResult<ProducerManagementResult>>
{
    public async Task<TResult<ProducerManagementResult>> Handle(UpdateManagementProducerCommand request, CancellationToken ct)
    {
        var loaded = await service.LoadAsync(request.ProducerId, request.ConcurrencyStamp, Permissions.Producers.Update, ct);
        if (!loaded.IsSuccess) return TResult<ProducerManagementResult>.Failure(loaded.Error!, loaded.ErrorCode);
        var duplicate = await service.EnsureCodeAvailableAsync(request.Code, request.ProducerId, ct);
        if (!duplicate.IsSuccess) return TResult<ProducerManagementResult>.Failure(duplicate.Error!, duplicate.ErrorCode);
        loaded.Data.UpdateDetails(request.Code, request.Name, request.LegalName, request.Description, request.WebsiteUrl);
        loaded.Data.RenewConcurrencyStamp();
        await unitOfWork.Repository<Producer>().UpdateAsync(loaded.Data, ct);
        return TResult<ProducerManagementResult>.Success(ProducerManagementService.Result(loaded.Data));
    }
}
