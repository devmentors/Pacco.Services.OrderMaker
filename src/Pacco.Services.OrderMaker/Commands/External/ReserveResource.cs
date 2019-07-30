using System;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Commands.External
{
    [MessageNamespace("availability")]
    public class ReserveResource : ICommand
    {
        public Guid OrderId { get; }
        public DateTime DateTime { get; }
        public int Priority { get; }

        public ReserveResource(Guid orderId, DateTime dateTime, int priority)
        {
            OrderId = orderId;
            DateTime = dateTime;
            Priority = priority;
        }
    }
}