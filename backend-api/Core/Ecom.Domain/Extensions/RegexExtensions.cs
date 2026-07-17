using System.Text.RegularExpressions;

namespace Ecom.Domain.Extensions
{
    public static class RegexExtensions
    {
        public const string OtmCodeRegex = @"^[A-Z0-9./_-]{1,50}$";

        /// <summary>
        /// Tối đa 50 ký tự
        /// Chỉ được phép nhập chữ cái IN HOA (A-Z),
        /// chữ số (0-9), dấu gạch chéo "/",
        /// dấu chấm ".", dấu gạch ngang "-",
        /// dấu gạch dưới "_" (không chữ thường, dấu cách " ", và các ký tự đặc biệt cũng như UNICODE)
        /// </summary>
        /// 
        public static bool IsValidOtmCode(this string? input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return Regex.IsMatch(input, OtmCodeRegex);
        }
    }
}

