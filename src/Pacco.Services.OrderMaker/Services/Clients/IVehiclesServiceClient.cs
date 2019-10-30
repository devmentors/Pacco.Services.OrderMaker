using System.Collections.Generic;
using System.Threading.Tasks;
using Pacco.Services.OrderMaker.DTO;

namespace Pacco.Services.OrderMaker.Services.Clients
{
    public interface IVehiclesServiceClient
    {
        Task<IEnumerable<VehicleDto>> FindAsync();
    }
}