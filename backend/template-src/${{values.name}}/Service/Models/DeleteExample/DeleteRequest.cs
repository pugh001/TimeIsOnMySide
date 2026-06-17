using System.ComponentModel.DataAnnotations;
using RestService.Common.Model;

namespace RestService.Service.Models.DeleteExample;

/// <summary>
/// Example model representing the request for an operation. 
/// </summary>
public class DeleteRequest : ValidRequest
{
    [Required(ErrorMessage = "The 'Id' field is required.")]
    public Guid Id { get; set; }
}