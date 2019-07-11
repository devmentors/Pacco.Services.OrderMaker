using System;
using Convey.CQRS.Events;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Events.External
{
    [MessageNamespace("orders")]
    public class ParcelAddedToOrder : IEvent
    {
        public Guid OrderId { get; }
        public Guid ParcelId { get; }

        public ParcelAddedToOrder(Guid orderId, Guid parcelId)
        {
            OrderId = orderId;
            ParcelId = parcelId;
        }
    }
}