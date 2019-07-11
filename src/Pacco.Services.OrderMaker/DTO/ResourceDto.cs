using System;
using System.Collections.Generic;

namespace Pacco.Services.OrderMaker.DTO
{
    public class ResourceDto
    {
        public Guid Id { get; set; }
        public IEnumerable<ReservationDto> Reservations { get; set; }
    }

    public class ReservationDto
    {
        public DateTime DateTime { get; set; }
        public int Priority { get; set; }
    }
}