using System;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Events.External
{
    [MessageNamespace("orders")]
    public class OrderApproved : IEvent
    {
        public Guid OrderId { get; }

        public OrderApproved(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}