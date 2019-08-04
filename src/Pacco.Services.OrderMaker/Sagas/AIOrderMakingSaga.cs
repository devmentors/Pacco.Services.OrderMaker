using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chronicle;
using Convey.MessageBrokers;
using Convey.MessageBrokers.CQRS;
using Microsoft.Extensions.Logging;
using Pacco.Services.OrderMaker.Commands;
using Pacco.Services.OrderMaker.Commands.External;
using Pacco.Services.OrderMaker.Events.External;
using Pacco.Services.OrderMaker.Services.Clients;

namespace Pacco.Services.OrderMaker.Sagas
{
    public class AIMakingOrderData
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime ReservationDate { get; set; }
        public List<Guid> ParcelIds { get; set; } = new List<Guid>();
        public List<Guid> AddedParcelIds { get; set; } = new List<Guid>();
        public bool AllPackagesAddedToOrder => AddedParcelIds.All(ParcelIds.Contains);
    }
    
    public class AIOrderMakingSaga : Saga<AIMakingOrderData>,
        ISagaStartAction<MakeOrder>,
        ISagaAction<OrderCreated>,
        ISagaAction<ParcelAddedToOrder>,
        ISagaAction<VehicleAssignedToOrder>,
        ISagaAction<OrderApproved>
    {
        private readonly IBusPublisher _publisher;
        private readonly ICorrelationContextAccessor _accessor;
        private readonly IAvailabilityServiceClient _client;
        private readonly ILogger<AIOrderMakingSaga> _logger;

        private const string VehicleId = "7546b838-d2c9-49e4-be04-da6d9ec889a5";

        public AIOrderMakingSaga(IBusPublisher publisher, ICorrelationContextAccessor accessor,
            IAvailabilityServiceClient client, ILogger<AIOrderMakingSaga> logger)
        {
            _publisher = publisher;
            _accessor = accessor;
            _client = client;
            _logger = logger;
        }

        public override SagaId ResolveId(object message, ISagaContext context)
        {
            switch (message)
            {
                case MakeOrder m: return m.OrderId.ToString();
                case OrderCreated m: return m.OrderId.ToString();
                case ParcelAddedToOrder m: return m.OrderId.ToString();
                case VehicleAssignedToOrder m: return m.OrderId.ToString();
                case OrderApproved m: return m.OrderId.ToString();
            }

            return base.ResolveId(message, context);
        }

        public async Task HandleAsync(MakeOrder message, ISagaContext context)
        {
            _logger.LogInformation($"Started a saga for order: {message.OrderId}, customer: {message.CustomerId}," +
                                   $"parcels: {message.ParcelId}");
            Data.ParcelIds.Add(message.ParcelId);
            Data.OrderId = message.OrderId;
            Data.CustomerId = message.CustomerId;
            await _publisher.SendAsync(new CreateOrder(Data.OrderId, message.CustomerId), _accessor.CorrelationContext);
        }

        public async Task HandleAsync(OrderCreated message, ISagaContext context)
        {
            var tasks = Data.ParcelIds.Select(id =>
                _publisher.SendAsync(new AddParcelToOrder(Data.OrderId, id, Data.CustomerId),
                    _accessor.CorrelationContext));

            await Task.WhenAll(tasks);
        }

        public async Task HandleAsync(ParcelAddedToOrder message, ISagaContext context)
        {
            Data.AddedParcelIds.Add(message.ParcelId);

            if (Data.AllPackagesAddedToOrder)
            {
                Data.VehicleId = true? new Guid(VehicleId) : Guid.Empty; // typical AI in startups

                var resource = await _client.GetResourceReservationsAsync(Data.VehicleId);
                var latestReservation = resource.Reservations.Any() 
                    ? resource.Reservations.OrderBy(r => r.DateTime).Last() : null;
                
                Data.ReservationDate = latestReservation?.DateTime.AddDays(1) ?? DateTime.UtcNow.AddDays(5);

                await _publisher.SendAsync(new AssignVehicleToOrder(Data.OrderId, Data.VehicleId, Data.ReservationDate),
                    _accessor.CorrelationContext);
            }
        }

        public Task HandleAsync(VehicleAssignedToOrder message, ISagaContext context)
            => _publisher.SendAsync(new ReserveResource(Data.VehicleId, Data.ReservationDate, 9999),
                _accessor.CorrelationContext);

        public Task HandleAsync(OrderApproved message, ISagaContext context)
        {
            _logger.LogInformation($"Completed a saga for order: {Data.OrderId}, customer: {Data.CustomerId}," +
                                   $"parcels: {string.Join(", ", Data.ParcelIds)}");

            return CompleteAsync();
        }

        public Task CompensateAsync(MakeOrder message, ISagaContext context)
            => Task.CompletedTask;
        
        public Task CompensateAsync(OrderCreated message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(ParcelAddedToOrder message, ISagaContext context)
            => _publisher.SendAsync(new CancelOrder(message.OrderId, "Because I'm saga"), 
                _accessor.CorrelationContext);

        public Task CompensateAsync(VehicleAssignedToOrder message, ISagaContext context)
            => Task.CompletedTask;
        
        public Task CompensateAsync(OrderApproved message, ISagaContext context)
            => Task.CompletedTask;

    }
}