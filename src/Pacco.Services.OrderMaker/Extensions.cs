using Chronicle;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.HTTP;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.OrderMaker.Events.External;
using Pacco.Services.OrderMaker.Services.Clients;

namespace Pacco.Services.OrderMaker
{
    public static class Extensions
    {
        public static IConveyBuilder AddApp(this IConveyBuilder builder)
        {
            builder
                .AddHttpClient()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddInMemoryCommandDispatcher()
                .AddInMemoryEventDispatcher()
                .AddRabbitMq<CorrelationContext>();

            builder.Services.AddChronicle();
            builder.Services.AddTransient<IAvailabilityServiceClient, AvailabilityServiceClient>();

                
            return builder;
        }
        
        public static IApplicationBuilder UseApp(this IApplicationBuilder app)
        {
            app
                .UseRabbitMq()
                .SubscribeEvent<OrderApproved>()
                .SubscribeEvent<OrderCreated>()
                .SubscribeEvent<ParcelAddedToOrder>()
                .SubscribeEvent<ResourceReserved>()
                .SubscribeEvent<VehicleAssignedToOrder>();
            
            return app;

        }
    }
}