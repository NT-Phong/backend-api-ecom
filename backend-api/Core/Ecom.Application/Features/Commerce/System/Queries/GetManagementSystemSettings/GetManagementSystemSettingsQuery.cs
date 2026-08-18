using Ecom.Domain.Entities;

namespace Ecom.Application.Features.Commerce.System.Queries.GetManagementSystemSettings;
public sealed record GetManagementSystemSettingsQuery : IRequest<TResult<CheckoutShippingSettingDto>>;
public sealed class GetManagementSystemSettingsQueryHandler(IUnitOfWork uow, ICurrentUser currentUser) : IRequestHandler<GetManagementSystemSettingsQuery, TResult<CheckoutShippingSettingDto>>
{
 public async Task<TResult<CheckoutShippingSettingDto>> Handle(GetManagementSystemSettingsQuery request, CancellationToken ct)
 {
  if(!currentUser.IsAuthenticated)return TResult<CheckoutShippingSettingDto>.Failure(MessageKey.Unauthorized,ErrorCodes.UNAUTHORIZED);
  if(!currentUser.HasPolicy(Permissions.Settings.Read))return TResult<CheckoutShippingSettingDto>.Failure(MessageKey.Forbidden,ErrorCodes.FORBIDDEN);
  var setting=await uow.Repository<SystemSetting>().FindOneAsync([x=>x.SettingKey=="checkout.shipping.standardFeeVnd"]);
  if(setting is null)return TResult<CheckoutShippingSettingDto>.Success(new(0m,false,null));
  var raw=setting.Value.Trim().Trim('"');
  if(!decimal.TryParse(raw,global::System.Globalization.NumberStyles.Number,global::System.Globalization.CultureInfo.InvariantCulture,out var fee)||fee<0)
   return TResult<CheckoutShippingSettingDto>.Failure("The checkout shipping setting is invalid.",ErrorCodes.UNPROCESSABLE_ENTITY);
  return TResult<CheckoutShippingSettingDto>.Success(new(fee,true,setting.ConcurrencyStamp));
 }
}
