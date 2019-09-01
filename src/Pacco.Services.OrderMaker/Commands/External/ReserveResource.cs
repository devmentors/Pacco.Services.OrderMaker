using System;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Commands.External
{
    [MessageNamespace("availability")]
    public class ReserveResource : ICommand
    {
        public Guid ResourceId { get; }
        public Guid CustomerId { get; }
        public DateTime DateTime { get; }
        public int Priority { get; }

        public ReserveResource(Guid resourceIdd, DateTime dateTime, int priority, Guid customerId)
        {
            ResourceId = resourceIdd;
            DateTime = dateTime;
            Priority = priority;
            CustomerId = customerId;
        }
    }
}