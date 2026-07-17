namespace Ecom.Application.Common.Configuration
{
    public class ApplicationSetting
    {
        public const string SectionName = "ApplicationSetting";
        public string? SelfUrl { get; set; }
        public required string DataFolderPath { get; set; }
        public required string StorageFolderPath { get; set; }
    }
}

