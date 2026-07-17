namespace Ecom.Domain.Constants;

public static class TestAccounts
{
	// Sắp xếp theo thứ tự số tăng dần, 05 giữ nguyên làm Admin
	public const string UnassignedUser = "0900000000"; // Mới đăng ký (Chưa role)
	public const string Manager = "0900000001"; // Chủ trại
	public const string EmployeeManager = "0900000002"; // Quản lý ao
	public const string EmployeeWarehouse = "0900000003"; // Thủ kho
	public const string Farmer = "0900000004"; // Nông dân (App)
	public const string Admin = "0900000005"; // Admin 

	public static readonly IReadOnlyList<string> All =
	[
		Admin,
		Manager,
		EmployeeManager,
		EmployeeWarehouse,
		Farmer,
		UnassignedUser,
	];
}
