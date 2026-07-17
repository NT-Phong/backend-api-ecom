using System.Linq.Expressions;
using PolicyEntity = Ecom.Domain.Entities.Policy;

namespace Ecom.Application.Features.Identity.Queries.GetPolicies;

public class GetPoliciesQueryHandler : IRequestHandler<GetPoliciesQuery, TResult<List<PoliciesDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PolicyEntity> _logger;
    public GetPoliciesQueryHandler(IUnitOfWork unitOfWork, ILogger<PolicyEntity> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<TResult<List<PoliciesDto>>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // ===========================================
            // 1. Get Policies
            // ===========================================
            var filterPolicies = new List<Expression<Func<PolicyEntity, bool>>>();
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var searchText = request.SearchText.Trim().ToLower();
                filterPolicies.Add(q => q.Name.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrEmpty(request.ModuleName))
            {
                filterPolicies.Add(q => q.Module == request.ModuleName);
            }
            var policies = await _unitOfWork.Repository<PolicyEntity>().FindResultAsync(
                filters: [.. filterPolicies],
                orderBy: request.OrderBy
            );

            // ===========================================
            // 2. Map DTO PoliciesDto
            // ===========================================
            var result = policies.Items.GroupBy(p => p.Module)
                .Select(g => new PoliciesDto
                {
                    ModuleGroup = GetModuleGroup(g.Key ?? string.Empty),
                    Module = g.Key ?? string.Empty,
                    ModuleName = GetModuleName(g.Key ?? string.Empty),
                    Policies = g.Select(p => new PolicyCode
                    {
                        CodeId = p.Id,
                        CodeValue = p.Code,
                        CodeName = p.Name,
                        CodeNo = p.No,
                        Type = GetTypeFromAction(p.Code)
                    }).ToList()
                })
                .ToList();

            return TResult<List<PoliciesDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while get polies.");
            return TResult<List<PoliciesDto>>.Failure("An error occurred while retrieving devices.", ErrorCodes.SERVER_ERROR);
        }
    }

    private static string GetModuleName(string moduleKey)
    {
        return moduleKey switch
        {
            "Users" or "UsersManage" => "Người dùng",
            "Roles" => "Vai trò",
            "RolePolicies" => "Quyền hạn vai trò",
            "User" => "Thông tin cá nhân",
            _ => moduleKey
        };
    }

    private static string GetModuleGroup(string moduleKey)
    {
        return moduleKey switch
        {
            "User" or "Users" or "UsersManage" or "Roles" or "RolePolicies" => "Cài đặt và quản lý người dùng",
            _ => "Khác"
        };
    }

    private static string GetTypeFromAction(string code)
    {
        if (string.IsNullOrEmpty(code)) return string.Empty;

        var action = code.ToLower();
        return action switch
        {
            _ when action.Contains("read") => "Xem",
            _ when action.Contains("create") => "Thêm",
            _ when action.Contains("update") => "Sửa",
            _ when action.Contains("delete") => "Xóa",
            _ when action.Contains("manage") => "Duyệt",
            _ when action.Contains("export") => "Xuất",
            _ => "Khác"
        };
    }
}
    
