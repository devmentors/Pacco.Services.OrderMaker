using System;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Commands.External
{
    [MessageNamespace("orders")]
    public class CancelOrder : ICommand
    {
        public Guid Id { get; }
        public string Reason { get; }

        public CancelOrder(Guid id, string reason)
        {
            Id = id;
            Reason = reason;
        }
    }
}