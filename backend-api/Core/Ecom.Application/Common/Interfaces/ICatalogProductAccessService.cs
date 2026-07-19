namespace Ecom.Application.Common.Interfaces;

public interface ICatalogProductAccessService
{
    TResult Ensure(string permission);
}
