using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class Calendrier
    {
        public Calendrier()
        {
            Reservation = new HashSet<Reservation>();
        }

        public DateTime Jour { get; set; }

        internal virtual ICollection<Reservation> Reservation { get; set; }
    }
}
