using System;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Events.External
{
    [MessageNamespace("orders")]
    public class OrderCreated : IEvent
    {
        public Guid OrderId { get; }

        public OrderCreated(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}