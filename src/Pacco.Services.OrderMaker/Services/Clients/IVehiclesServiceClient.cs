using System.Threading.Tasks;
using Convey.CQRS.Queries;
using Pacco.Services.OrderMaker.DTO;

namespace Pacco.Services.OrderMaker.Services.Clients
{
    public interface IVehiclesServiceClient
    {
        Task<PagedResult<VehicleDto>> FindAsync();
    }
}