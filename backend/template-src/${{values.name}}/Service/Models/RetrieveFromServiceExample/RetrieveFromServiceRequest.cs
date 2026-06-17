using System.ComponentModel.DataAnnotations;
using RestService.Common.Model;

namespace RestService.Service.Models.RetrieveFromServiceExample;

/// <summary>
/// Example model representing the request for an operation. 
/// </summary>
public class RetrieveFromServiceRequest : ValidRequest
{
    public string SomeField1 { get; set; }

    public string SomeField2 { get; set; }

    [Required(ErrorMessage = "The 'SomeField3' field is required.")]
    public string SomeField3 { get; set; }
}