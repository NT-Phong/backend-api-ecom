using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.System.Commands.UpsertSystemSetting;
public sealed record UpsertSystemSettingCommand(decimal StandardFeeVnd, Guid? ConcurrencyStamp)
 : IRequest<TResult<CheckoutShippingSettingDto>>, ITransactionalRequest;
public sealed class UpsertSystemSettingCommandValidator:AbstractValidator<UpsertSystemSettingCommand>
{public UpsertSystemSettingCommandValidator(){RuleFor(x=>x.StandardFeeVnd).InclusiveBetween(0m,10000000m);}}
public sealed class UpsertSystemSettingCommandHandler(IUnitOfWork uow, ICurrentUser currentUser):IRequestHandler<UpsertSystemSettingCommand,TResult<CheckoutShippingSettingDto>>
{
 public async Task<TResult<CheckoutShippingSettingDto>> Handle(UpsertSystemSettingCommand request,CancellationToken ct)
 {
  if(!currentUser.IsAuthenticated)return TResult<CheckoutShippingSettingDto>.Failure(MessageKey.Unauthorized,ErrorCodes.UNAUTHORIZED);
  if(!currentUser.HasPolicy(Permissions.Settings.Update))return TResult<CheckoutShippingSettingDto>.Failure(MessageKey.Forbidden,ErrorCodes.FORBIDDEN);
  var value=request.StandardFeeVnd.ToString(global::System.Globalization.CultureInfo.InvariantCulture);var setting=await uow.Repository<SystemSetting>().FindOneAsync([x=>x.SettingKey=="checkout.shipping.standardFeeVnd"]);
  if(setting is null){if(request.ConcurrencyStamp.HasValue)return TResult<CheckoutShippingSettingDto>.Failure(MessageKey.DataHasBeenChanged,ErrorCodes.ALREADY_EXISTS);setting=SystemSetting.Create("checkout.shipping.standardFeeVnd",value,false,"Standard checkout shipping fee in VND.");await uow.Repository<SystemSetting>().InsertAsync(setting,ct);}
  else {if(!request.ConcurrencyStamp.HasValue||setting.ConcurrencyStamp!=request.ConcurrencyStamp.Value)return TResult<CheckoutShippingSettingDto>.Failure(MessageKey.DataHasBeenChanged,ErrorCodes.ALREADY_EXISTS);setting.UpdateValue(value);setting.ConcurrencyStamp=Guid.NewGuid();await uow.Repository<SystemSetting>().UpdateAsync(setting,ct);}
  return TResult<CheckoutShippingSettingDto>.Success(new(request.StandardFeeVnd,true,setting.ConcurrencyStamp));
 }
}
