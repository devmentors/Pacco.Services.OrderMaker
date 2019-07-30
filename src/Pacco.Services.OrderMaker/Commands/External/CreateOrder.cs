using System;
using Convey.CQRS.Commands;
using Convey.MessageBrokers;

namespace Pacco.Services.OrderMaker.Commands.External
{
    [MessageNamespace("orders")]
    public class CreateOrder : ICommand
    {
        public Guid OrderId { get; }
        public Guid CustomerId { get; }

        public CreateOrder(Guid id, Guid customerId)
        {
            OrderId = id == Guid.Empty ? Guid.NewGuid() : id;
            CustomerId = customerId;
        }
    }
}