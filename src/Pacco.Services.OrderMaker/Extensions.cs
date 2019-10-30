using Chronicle;
using Convey;
using Convey.CQRS.Commands;
using Convey.CQRS.Events;
using Convey.Discovery.Consul;
using Convey.HTTP;
using Convey.LoadBalancing.Fabio;
using Convey.MessageBrokers.CQRS;
using Convey.MessageBrokers.RabbitMQ;
using Convey.Metrics.AppMetrics;
using Convey.Persistence.Redis;
using Convey.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Pacco.Services.OrderMaker.Events.External;
using Pacco.Services.OrderMaker.Services.Clients;

namespace Pacco.Services.OrderMaker
{
    public static class Extensions
    {
        public static IConveyBuilder AddApp(this IConveyBuilder builder)
        {
            builder.AddHttpClient()
                .AddConsul()
                .AddFabio()
                .AddCommandHandlers()
                .AddEventHandlers()
                .AddInMemoryCommandDispatcher()
                .AddInMemoryEventDispatcher()
                .AddRedis()
                .AddMetrics()
                .AddRabbitMq();

            builder.Services.AddChronicle();
            builder.Services.AddTransient<IAvailabilityServiceClient, AvailabilityServiceClient>();
            builder.Services.AddTransient<IVehiclesServiceClient, VehiclesServiceClient>();

            return builder;
        }

        public static IApplicationBuilder UseApp(this IApplicationBuilder app)
        {
            app.UseErrorHandler()
                .UseInitializers()
                .UseMetrics()
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