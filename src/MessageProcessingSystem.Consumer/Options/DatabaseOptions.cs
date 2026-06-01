namespace MessageProcessingSystem.Consumer.Options;

public class DatabaseOptions
{
    public string ConnectionString { get; set; } =
        "Server=localhost,1433;Database=MessageProcessingSystem;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
}