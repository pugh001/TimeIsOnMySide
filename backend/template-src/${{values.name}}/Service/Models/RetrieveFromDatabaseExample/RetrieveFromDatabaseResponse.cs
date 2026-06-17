namespace RestService.Service.Models.RetrieveFromDatabaseExample;

/// <summary>
/// Example model representing a response for an operation. 
/// </summary>
public record RetrieveFromDatabaseResponse
{
    public Guid Id { get; set; }

    public string SomeField1 { get; set; }

    public string SomeField2 { get; set; }

    public string SomeField3 { get; set; }
}