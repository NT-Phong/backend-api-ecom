namespace Ecom.Domain.Constants;

public enum ErrorCodes
{
    BAD_REQUEST = 400,
    UNAUTHORIZED = 401,
    FORBIDDEN = 403,
    NOT_FOUND = 404,
    NOT_ACTIVE = 405,
    CHANGE_PASSWORD = 406,
    TIME_OUT = 408,
    ALREADY_EXISTS = 409,
    CHOOSE_ACCOUNT = 410,
    UNPROCESSABLE_ENTITY = 422,
    INTERNAL_SERVER_ERROR = 500,
    SERVER_ERROR = 500,
    EXTERNAL_SERVICE_ERROR = 502,
    SERVICE_UNAVAILABLE = 503
}

public static class MessageKey
{
    #region Common
    public const string InternalError = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại.";
    public const string ValidationFailed = "Dữ liệu không hợp lệ.";
    public const string Unauthorized = "Bạn chưa đăng nhập.";
    public const string Forbidden = "Bạn không có quyền truy cập.";
    #endregion

    #region User / Auth
    public const string AccountLockedWithMinutes = "Tài khoản của bạn đã bị khóa tạm thời. Vui lòng thử lại sau {0} phút.";
    public const string OtpInvalidWithAttempts = "Mã OTP bạn nhập không đúng. Bạn còn {0} lần thử.";
    public const string OtpResendWait = "Vui lòng đợi {0} giây để yêu cầu mã mới.";
    public const string OtpInvalid = "Không tìm thấy OTP hợp lệ. Vui lòng yêu cầu OTP mới.";
    public const string OtpExpired = "OTP đã hết hạn. Vui lòng yêu cầu OTP mới.";
    public const string OtpBlocked = "Bạn đã nhập sai OTP quá nhiều lần. Vui lòng yêu cầu OTP mới sau 15 phút.";
    public const string VerificationFailed = "Xác thực OTP thất bại. Vui lòng thử lại.";
    public const string DeletionReasonRequired = "Vui lòng chọn ít nhất một lý do để xóa tài khoản.";
    public const string PhoneNumberMismatch = "Số điện thoại không khớp với tài khoản hiện tại.";
    public const string LoginRequired = "Vui lòng đăng nhập để thực hiện hành động này.";
    public const string OtpInvalidOrExpired = "OTP không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu OTP mới.";
    public const string PhoneNumberInvalid = "Số điện thoại không hợp lệ.";
    public const string PhoneNumberRequired = "Số điện thoại không được để trống.";
    public const string PhoneNumberNotFound = "Số điện thoại chưa được đăng ký. Vui lòng đăng ký tài khoản mới.";
    public const string PhoneNumberAlreadyExists = "Số điện thoại đã được đăng ký.";
    public const string AccountScopedToMobile = "Tài khoản này chỉ có thể sử dụng trên ứng dụng di động.";
    public const string RegisterSuccess = "Đăng ký thành công. Vui lòng xác thực OTP để kích hoạt tài khoản.";
    public const string UserNotFound = "Không tìm thấy người dùng.";
    public const string DeleteAccountSuccess = "Xóa tài khoản thành công.";
    public const string UserEmailAlreadyExists = "Email đã được sử dụng.";
    public const string UserNotActive = "Tài khoản của bạn chưa được kích hoạt. Vui lòng xác thực OTP để kích hoạt tài khoản.";
    public const string UserInvalidCredentials = "Thông tin đăng nhập không chính xác.";
    public const string UserAccountLocked = "Tài khoản đã bị khóa.";
    public const string UserAccountDisabled = "Tài khoản của bạn đã bị vô hiệu hóa.";
    public const string UserProfileUpdateSuccess = "Cập nhật thông tin cá nhân thành công.";
    public const string UserProfileUpdateFailed = "Không thể cập nhật thông tin cá nhân. Vui lòng thử lại.";
    public const string UserProfileCompleteSuccess = "Hoàn thiện thông tin cá nhân thành công.";
    public const string OtpSentSuccess = "OTP đã được gửi thành công đến số điện thoại của bạn.";
    public const string OtpNotTrue = "OTP không đúng cho tài khoản Test. Vui lòng thử lại.";
    public const string RoleNotFound = "Không tìm thấy vai trò (Role).";
    public const string UpdateRoleSuccess = "Cập nhật quyền hạn thành công.";
    public const string CannotUpdateOwnRole = "Bạn không thể tự thay đổi quyền hạn của chính mình.";
    public const string InsufficientPermissions = "Bạn không có đủ quyền để thực hiện hành động này.";
    public const string UserCannotDeleteOwnAccount = "Bạn không thể tự xóa tài khoản của chính mình.";
    public const string RoleAlreadyExists = "Quyền hạn đã tồn tại.";
    public const string DeleteSystemRoleFailed = "Không thể xóa quyền hạn mặc định của hệ thống.";
    public const string PleaseMoveRoleToOtherUser = "Vui lòng chuyển người dùng sang quyền hạn khác trước khi xóa.";
    #endregion

    #region Validation
    public const string InvalidQuantity = "Số lượng phải lớn hơn 0.";
    public const string InvalidPrice = "Giá không được âm.";
    public const string InvalidStatus = "Trạng thái không hợp lệ.";
    #endregion

    #region Concurrency
    public const string DataHasBeenChanged = "Dữ liệu đã được thay đổi. Vui lòng tải lại và thử lại.";
    #endregion

    #region Policy
    public const string PolicyIsRequired = "Danh sách quyền không được rỗng.";
    public const string PolicyNotFound = "Danh sách quyền không được rỗng.";
    #endregion
}

