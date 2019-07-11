using System;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Events.External
{
    [MessageNamespace("orders")]
    public class OrderApproved : IEvent
    {
        public Guid Id { get; }

        public OrderApproved(Guid id)
        {
            Id = id;
        }
    }
}