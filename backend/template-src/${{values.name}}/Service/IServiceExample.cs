using RestService.Service.Models.CreateExample;
using RestService.Service.Models.DeleteExample;
using RestService.Service.Models.RetrieveFromDatabaseExample;
using RestService.Service.Models.RetrieveFromServiceExample;

namespace RestService.Service;

public interface IServiceExample
{
    Task Create(CreateRequest request);

    Task<bool> Delete(DeleteRequest request);

    Task<RetrieveFromDatabaseResponse> RetrieveFromDatabase(RetrieveFromDatabaseRequest request);

    Task<RetrieveFromServiceResponse> RetrieveFromService(RetrieveFromServiceRequest request);
}