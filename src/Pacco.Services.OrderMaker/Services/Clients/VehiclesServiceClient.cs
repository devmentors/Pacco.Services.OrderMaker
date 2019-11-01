using System.Threading.Tasks;
using Convey.CQRS.Queries;
using Convey.HTTP;
using Pacco.Services.OrderMaker.DTO;

namespace Pacco.Services.OrderMaker.Services.Clients
{
    public class VehiclesServiceClient : IVehiclesServiceClient
    {
        private readonly IHttpClient _httpClient;
        private readonly string _url;

        public VehiclesServiceClient(IHttpClient httpClient, HttpClientOptions options)
        {
            _httpClient = httpClient;
            _url = options.Services["vehicles"];
        }

        public Task<PagedResult<VehicleDto>> FindAsync()
            => _httpClient.GetAsync<PagedResult<VehicleDto>>($"{_url}/vehicles");
    }
}