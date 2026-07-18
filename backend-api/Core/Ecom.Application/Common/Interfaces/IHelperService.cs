namespace Ecom.Application.Common.Interfaces;

public interface IHelperService
{
    string GetRandomString(int length = 16, TextLetterCase letterCase = TextLetterCase.Normal);
    string GetRandomStringNumber(int length = 8);
    string GetPublicPhrase(string secretKey, string secretPhrase);
    byte[] ConvertBase64ToBinaries(string base64String);
}

public enum TextLetterCase { Normal, Upper, Lower }
