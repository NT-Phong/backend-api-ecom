using Ecom.Application.Features.Identity.Queries.GetPolicies;
using System.Linq.Expressions;
using PolicyEntity = Ecom.Domain.Entities.Policy;
using RolePolicyEntity = Ecom.Domain.Entities.RolePolicy;

namespace Ecom.Application.Features.Identity.Queries.GetPoliciesByRole;

public class GetPoliciesByRoleQueryHandler : IRequestHandler<GetPoliciesByRoleQuery, TResult<List<PoliciesByRoleDto>>>
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PolicyEntity> _logger;
    public GetPoliciesByRoleQueryHandler(IUnitOfWork unitOfWork, ILogger<PolicyEntity> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<TResult<List<PoliciesByRoleDto>>> Handle(GetPoliciesByRoleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // ===========================================
            // 1. Get Role Policy
            // ===========================================
            var rolePolicies = await _unitOfWork.Repository<RolePolicyEntity>().FindResultAsync(
                filters: [e => e.RoleId == request.RoleId]
            );
            var policyIds = rolePolicies.Items.Select(e => e.PolicyId).ToHashSet();

            // Lấy tất cả policies trong hệ thống
            var policies = await _unitOfWork.Repository<PolicyEntity>().FindResultAsync();

            // ===========================================
            // 2. Map DTO PoliciesDto
            // ===========================================
            var result = policies.Items.GroupBy(p => p.Module)
                .Select(g => new PoliciesByRoleDto
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
                        Type = GetTypeFromAction(p.Code),
                        HasPermission = policyIds.Contains(p.Id)
                    }).ToList()
                })
                .ToList();

            return TResult<List<PoliciesByRoleDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while get polies.");
            return TResult<List<PoliciesByRoleDto>>.Failure("An error occurred while retrieving devices.", ErrorCodes.SERVER_ERROR);
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

