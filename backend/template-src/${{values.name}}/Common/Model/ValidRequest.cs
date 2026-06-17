using System.ComponentModel.DataAnnotations;

namespace RestService.Common.Model;

public class ValidRequest
{
    public void Validate()
    {
        var context = new ValidationContext(this, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(this, context, results, true))
        {
            var validationErrors = from error in results select error;

            foreach (var error in validationErrors)
            {
                throw new ValidationException(error.ErrorMessage);
            }
        }
    }
}