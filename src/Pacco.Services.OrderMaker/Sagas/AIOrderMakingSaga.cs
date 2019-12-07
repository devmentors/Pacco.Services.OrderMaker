using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Convey.MessageBrokers;
using Microsoft.Extensions.Logging;
using Pacco.Services.OrderMaker.Commands;
using Pacco.Services.OrderMaker.Commands.External;
using Pacco.Services.OrderMaker.Events;
using Pacco.Services.OrderMaker.Events.External;
using Pacco.Services.OrderMaker.Events.Rejected;
using Pacco.Services.OrderMaker.Services.Clients;

namespace Pacco.Services.OrderMaker.Sagas
{
    public class AIOrderMakingSaga : Saga<AIMakingOrderData>,
        ISagaStartAction<MakeOrder>,
        ISagaAction<OrderCreated>,
        ISagaAction<ParcelAddedToOrder>,
        ISagaAction<VehicleAssignedToOrder>,
        ISagaAction<OrderApproved>
    {
        private const string SagaHeader = "Saga";
        private readonly IBusPublisher _publisher;
        private readonly ICorrelationContextAccessor _accessor;
        private readonly IAvailabilityServiceClient _client;
        private readonly IVehiclesServiceClient _vehiclesServiceClient;
        private readonly ILogger<AIOrderMakingSaga> _logger;

        public AIOrderMakingSaga(IBusPublisher publisher, ICorrelationContextAccessor accessor,
            IAvailabilityServiceClient client, IVehiclesServiceClient vehiclesServiceClient,
            ILogger<AIOrderMakingSaga> logger)
        {
            _publisher = publisher;
            _accessor = accessor;
            _client = client;
            _vehiclesServiceClient = vehiclesServiceClient;
            _logger = logger;
            _accessor.CorrelationContext = new CorrelationContext
            {
                User = new CorrelationContext.UserContext()
            };
        }

        public override SagaId ResolveId(object message, ISagaContext context)
            => message switch
            {
                MakeOrder m => (SagaId) m.OrderId.ToString(),
                OrderCreated m => (SagaId) m.OrderId.ToString(),
                ParcelAddedToOrder m => (SagaId) m.OrderId.ToString(),
                VehicleAssignedToOrder m => (SagaId) m.OrderId.ToString(),
                OrderApproved m => m.OrderId.ToString(),
                _ => base.ResolveId(message, context)
            };

        public async Task HandleAsync(MakeOrder message, ISagaContext context)
        {
            _logger.LogInformation($"Started a saga for order: {message.OrderId}, customer: {message.CustomerId}," +
                                   $"parcels: {message.ParcelId}");
            
            Data.ParcelIds.Add(message.ParcelId);
            Data.OrderId = message.OrderId;
            Data.CustomerId = message.CustomerId;
            
            await _publisher.PublishAsync(new CreateOrder(Data.OrderId, message.CustomerId),
                messageContext: _accessor.CorrelationContext,
                headers: new Dictionary<string, object>
                {
                    [SagaHeader] = SagaStates.Pending.ToString()
                });
        }

        public async Task HandleAsync(OrderCreated message, ISagaContext context)
        {
            var tasks = Data.ParcelIds.Select(id =>
                _publisher.PublishAsync(new AddParcelToOrder(Data.OrderId, id, Data.CustomerId),
                    messageContext: _accessor.CorrelationContext,
                    headers: new Dictionary<string, object>
                    {
                        [SagaHeader] = SagaStates.Pending.ToString()
                    }));

            await Task.WhenAll(tasks);
        }

        public async Task HandleAsync(ParcelAddedToOrder message, ISagaContext context)
        {
            Data.AddedParcelIds.Add(message.ParcelId);

            if (Data.AllPackagesAddedToOrder)
            {
                _logger.LogInformation("Searching for a vehicle...");
                var vehicles = await _vehiclesServiceClient.FindAsync();
                var vehicle = vehicles.Items.FirstOrDefault(); // typical AI in startups
                if (vehicle is null)
                {
                    const string reason = "Vehicle was not found.";
                    const string code = "vehicle_not_found";
                    _logger.LogError(reason);
                    
                    await _publisher.PublishAsync(new MakeOrderRejected(Data.OrderId, reason, code),
                        messageContext: _accessor.CorrelationContext,
                        headers: new Dictionary<string, object>
                        {
                            [SagaHeader] = SagaStates.Rejected.ToString()
                        });

                    await RejectAsync();
                    return;
                }

                _logger.LogInformation($"Found a vehicle: {vehicle.Brand}, {vehicle.Model} for " +
                                       $"{vehicle.PricePerService}$ [id: {vehicle.Id}]");
                
                Data.VehicleId = vehicle.Id;
                var resource = await _client.GetResourceReservationsAsync(Data.VehicleId);
                var latestReservation = resource.Reservations.Any()
                    ? resource.Reservations.OrderBy(r => r.DateTime).Last()
                    : null;

                Data.ReservationDate = latestReservation?.DateTime.AddDays(1) ?? DateTime.UtcNow.AddDays(5);

                await _publisher.PublishAsync(
                    new AssignVehicleToOrder(Data.OrderId, Data.VehicleId, Data.ReservationDate),
                    messageContext: _accessor.CorrelationContext,
                    headers: new Dictionary<string, object>
                    {
                        [SagaHeader] = SagaStates.Pending.ToString()
                    });
            }
        }

        public Task HandleAsync(VehicleAssignedToOrder message, ISagaContext context)
            => _publisher.PublishAsync(new ReserveResource(Data.VehicleId, Data.ReservationDate, 9999, Data.CustomerId),
                messageContext: _accessor.CorrelationContext,
                headers: new Dictionary<string, object>
                {
                    [SagaHeader] = SagaStates.Pending.ToString()
                });

        public async Task HandleAsync(OrderApproved message, ISagaContext context)
        {
            _logger.LogInformation($"Completed a saga for order: {Data.OrderId}, customer: {Data.CustomerId}," +
                                   $"parcels: {string.Join(", ", Data.ParcelIds)}");

            await _publisher.PublishAsync(new MakeOrderCompleted(message.OrderId),
                messageContext: _accessor.CorrelationContext,
                headers: new Dictionary<string, object>
                {
                    [SagaHeader] = SagaStates.Completed.ToString()
                });

            await CompleteAsync();
        }

        public Task CompensateAsync(MakeOrder message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(OrderCreated message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(ParcelAddedToOrder message, ISagaContext context)
            => _publisher.PublishAsync(new CancelOrder(message.OrderId, "Because I'm saga"),
                messageContext: _accessor.CorrelationContext,
                headers: new Dictionary<string, object>
                {
                    [SagaHeader] = SagaStates.Rejected.ToString()
                });

        public Task CompensateAsync(VehicleAssignedToOrder message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(OrderApproved message, ISagaContext context)
            => Task.CompletedTask;
    }
}