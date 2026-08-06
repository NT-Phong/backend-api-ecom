namespace Ecom.Domain.Constants;

/// <summary>
/// Định nghĩa tất cả Permissions/Policies trong hệ thống
/// Sử dụng với [Authorize(Policy = Permissions.Users.Read)]
/// </summary>
public static class Permissions
{
    #region Users Module

    public static class User
    {
        public const string Read = "user.read";
        public const string Update = "user.update";
        public const string Delete = "user.delete";
    }

    public static class UsersManage // Admin
    {
        public const string Read = "users_manage.read";
        public const string Create = "users_manage.create";
        public const string Update = "users_manage.update";
        public const string Delete = "users_manage.delete";
    }

    #endregion

    #region Media

    public static class Media
    {
        public const string Upload = "media.upload";
        public const string Read = "media.read";
        public const string Delete = "media.delete";
        public const string Manage = "media.manage";
    }

    #endregion

    #region Catalog Categories

    public static class CatalogCategories
    {
        public const string Read = "catalog.categories.read";
        public const string Create = "catalog.categories.create";
        public const string Update = "catalog.categories.update";
        public const string Publish = "catalog.categories.publish";
        public const string Deactivate = "catalog.categories.deactivate";
    }

    #endregion

    #region Catalog Products

    public static class CatalogProducts
    {
        public const string Read = "catalog.products.read";
        public const string Create = "catalog.products.create";
        public const string Update = "catalog.products.update";
        public const string Publish = "catalog.products.publish";
        public const string Discontinue = "catalog.products.discontinue";
    }

    public static class Orders
    {
        public const string Read = "orders.read";
        public const string Manage = "orders.manage";
    }

    public static class Payments
    {
        public const string Verify = "payments.verify";
        public const string Refund = "payments.refund";
    }

    public static class Shipments
    {
        public const string Manage = "shipments.manage";
    }

    #endregion

    #region Roles Module

    public static class Roles // Admin
    {
        public const string Read = "roles.read";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string AssignRole = "roles.assign_role";

        // Role Codes (used for seeding and authorization)
        public const string SystemAdmin = "SYSTEM_ADMIN";
        public const string Admin = "ADMIN";
        public const string Manager = "MANAGER";
        public const string User = "USER";
    }

    #endregion

    #region RolePolicy Module

    public static class RolePolicies // Admin
    {
        public const string Read = "role_policies.read";
    }

    #endregion

    public static List<PermissionDefinition> GetAll()
    {
        return new List<PermissionDefinition>
        {
            // User
            new(User.Read, "Xem thông tin cá nhân", "User"),
            new(User.Update, "Cập nhật thông tin cá nhân", "User"),
            new(User.Delete, "Xóa tài khoản", "User"),

            // UsersManage
            new(UsersManage.Read, "Xem danh sách người dùng", "UsersManage"),
            new(UsersManage.Create, "Tạo người dùng mới", "UsersManage"),
            new(UsersManage.Update, "Cập nhật thông tin người dùng", "UsersManage"),
            new(UsersManage.Delete, "Xóa người dùng", "UsersManage"),

            // Roles
            new(Roles.Read, "Xem danh sách vai trò", "Roles"),
            new(Roles.Create, "Tạo vai trò mới", "Roles"),
            new(Roles.Update, "Cập nhật vai trò", "Roles"),
            new(Roles.Delete, "Xóa vai trò", "Roles"),
            new(Roles.AssignRole, "Gán vai trò cho người dùng", "Roles"),

            // RolePolicies
            new(RolePolicies.Read, "Xem quyền của vai trò", "RolePolicies"),

            // Catalog Products
            new(CatalogProducts.Read, "Xem quản trị sản phẩm", "CatalogProducts"),
            new(CatalogProducts.Create, "Tạo sản phẩm", "CatalogProducts"),
            new(CatalogProducts.Update, "Cập nhật sản phẩm", "CatalogProducts"),
            new(CatalogProducts.Publish, "Xuất bản sản phẩm", "CatalogProducts"),
            new(CatalogProducts.Discontinue, "Ngừng kinh doanh sản phẩm", "CatalogProducts"),
            new(Media.Upload, "Tải ảnh sản phẩm", "Media"),
            new(Media.Read, "Xem metadata media", "Media"),
            new(Media.Delete, "Xóa media chưa sử dụng", "Media"),
            new(Media.Manage, "Quản trị toàn bộ media", "Media")
            ,new(Orders.Read, "Xem và tra cứu đơn hàng", "Orders")
            ,new(Orders.Manage, "Quản lý trạng thái đơn hàng", "Orders")
            ,new(Payments.Verify, "Xác minh thanh toán", "Payments")
            ,new(Payments.Refund, "Hoàn tiền thanh toán", "Payments")
            ,new(Shipments.Manage, "Quản lý giao hàng", "Shipments")
            ,new(CatalogCategories.Read, "Xem quản trị danh mục", "CatalogCategories")
            ,new(CatalogCategories.Create, "Tạo danh mục", "CatalogCategories")
            ,new(CatalogCategories.Update, "Cập nhật danh mục", "CatalogCategories")
            ,new(CatalogCategories.Publish, "Xuất bản danh mục", "CatalogCategories")
            ,new(CatalogCategories.Deactivate, "Ẩn danh mục", "CatalogCategories")
        };
    }
}

/// <summary>
/// DTO chứa thông tin Permission để seed database
/// </summary>
public record PermissionDefinition(string Code, string Name, string Module);

