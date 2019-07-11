using System;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Events.External
{
    [MessageNamespace("orders")]
    public class OrderCreated : IEvent
    {
        public Guid Id { get; }

        public OrderCreated(Guid id)
        {
            Id = id;
        }
    }
}