using System;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Commands.External
{
    [MessageNamespace("availability")]
    public class ReserveResource : ICommand
    {
        public Guid Id { get; }
        public DateTime DateTime { get; }
        public int Priority { get; }

        public ReserveResource(Guid id, DateTime dateTime, int priority)
        {
            Id = id;
            DateTime = dateTime;
            Priority = priority;
        }
    }
}