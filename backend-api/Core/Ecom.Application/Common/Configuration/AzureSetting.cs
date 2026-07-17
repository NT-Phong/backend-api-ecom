namespace Ecom.Application.Common.Configuration
{
    public class AzureSetting
    {
        public const string SectionName = "AzureSetting";
        public required BlobStorageConfig BlobStorage { get; set; } 

        public class BlobStorageConfig
        {
            public required string ConnectionString { get; set; }
        }
    }
}

