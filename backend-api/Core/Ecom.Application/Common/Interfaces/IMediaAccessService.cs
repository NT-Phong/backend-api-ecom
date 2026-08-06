using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

public interface IMediaAccessService
{
    TResult EnsureOwnerOrManager(MediaAsset mediaAsset);
}
