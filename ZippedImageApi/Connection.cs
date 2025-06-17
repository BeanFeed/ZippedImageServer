namespace ZippedImageApi;

public static class Connection
{
    private static string _hostname = "localhost";
    private static int _port = 5000;
    private static Protocols _protocol = Protocols.Http;
    private static string _origin = "http://localhost:5000";

    public static string Hostname
    {
        get => _hostname;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Hostname cannot be null or empty.");
            _hostname = value;
            _origin = $"{Enum.GetName(_protocol)}://{_hostname}:{_port}";
        }
    }

    public static Protocols Protocol
    {
        get => _protocol;
        set
        {
            _protocol = value;
            _origin = $"{Enum.GetName(_protocol)}://{_hostname}:{_port}";
        }
    }
    
    public static int Port
    {
        get => _port;
        set
        {
            if (value <= 0 || value > 65535)
                throw new ArgumentException("Port must be between 1 and 65535.");
            _port = value;
            _origin = $"{Enum.GetName(_protocol)}://{_hostname}:{_port}";
        }
    }

    public static string Origin
    {
        get => _origin;
    }
    
    public static string? JwtBearerToken { get; set; } = null;

    public static string? ApiKey { get; set; } = null;

    public enum Protocols
    {
        Http,
        Https
    }
}