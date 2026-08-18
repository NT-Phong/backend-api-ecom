using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.Producers.Commands.CreateManagementProducer;

public sealed record CreateManagementProducerCommand(string Code, string Name, string? LegalName, string? Description,
    string? WebsiteUrl) : IRequest<TResult<ProducerManagementResult>>, ITransactionalRequest;

public sealed class CreateManagementProducerCommandValidator : AbstractValidator<CreateManagementProducerCommand>
{
    public CreateManagementProducerCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.LegalName).MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(10000);
        RuleFor(x => x.WebsiteUrl).MaximumLength(500).Must(x => string.IsNullOrWhiteSpace(x) || Uri.TryCreate(x, UriKind.Absolute, out _));
    }
}

public sealed class CreateManagementProducerCommandHandler(IUnitOfWork unitOfWork, ProducerManagementService service)
    : IRequestHandler<CreateManagementProducerCommand, TResult<ProducerManagementResult>>
{
    public async Task<TResult<ProducerManagementResult>> Handle(CreateManagementProducerCommand request, CancellationToken ct)
    {
        var authorization = service.Ensure(Permissions.Producers.Create);
        if (!authorization.IsSuccess) return TResult<ProducerManagementResult>.Failure(authorization.Error!, authorization.ErrorCode);
        var duplicate = await service.EnsureCodeAvailableAsync(request.Code, null, ct);
        if (!duplicate.IsSuccess) return TResult<ProducerManagementResult>.Failure(duplicate.Error!, duplicate.ErrorCode);
        var producer = Producer.Create(request.Code, request.Name, request.LegalName, request.Description, request.WebsiteUrl);
        await unitOfWork.Repository<Producer>().InsertAsync(producer, ct);
        return TResult<ProducerManagementResult>.Success(ProducerManagementService.Result(producer));
    }
}
