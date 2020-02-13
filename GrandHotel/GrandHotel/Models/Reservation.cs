using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class Reservation
    {
        public short NumChambre { get; set; }
        public DateTime Jour { get; set; }
        public int IdClient { get; set; }
        public byte NbPersonnes { get; set; }
        public byte HeureArrivee { get; set; }
        public bool? Travail { get; set; }

        public virtual Client IdClientNavigation { get; set; }
        public virtual Calendrier JourNavigation { get; set; }
        public virtual Chambre NumChambreNavigation { get; set; }
    }
}
