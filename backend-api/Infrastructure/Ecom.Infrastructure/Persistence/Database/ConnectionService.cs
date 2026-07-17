using Ecom.Application.Common.Configuration;

namespace Ecom.Infrastructure.Persistence.Database;

public interface IConnectionService
{
    string GetReadConnectionString();
    string GetWriteConnectionString();
}

public class ConnectionService : IConnectionService
{
    private readonly ConnectionSettings _connectionSettings;
    
    public ConnectionService(IOptions<ConnectionSettings> connectionOptions)
    {
        _connectionSettings = connectionOptions.Value;
    }
    
    public string GetReadConnectionString()
    {
        if (!string.IsNullOrEmpty(_connectionSettings.DefaultConnection))
            return _connectionSettings.DefaultConnection;
            
        throw new InvalidOperationException("No database connection string configured");
    }
    
    public string GetWriteConnectionString()
    {
        if (!string.IsNullOrEmpty(_connectionSettings.DefaultConnection))
            return _connectionSettings.DefaultConnection;
            
        throw new InvalidOperationException("No database connection string configured");
    }
}
