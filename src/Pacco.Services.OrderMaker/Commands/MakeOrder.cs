using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Convey.CQRS.Commands;

namespace Pacco.Services.OrderMaker.Commands
{
    public class MakeOrder : ICommand
    {
        public MakeOrder(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }
        public Guid CustomerId { get; }
        public IEnumerable<Guid> ParcelIds { get; }
    }
}