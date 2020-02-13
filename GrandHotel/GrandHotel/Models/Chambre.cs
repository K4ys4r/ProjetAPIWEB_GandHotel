using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class Chambre
    {
        public Chambre()
        {
            Reservation = new HashSet<Reservation>();
            TarifChambre = new HashSet<TarifChambre>();
        }

        public short Numero { get; set; }
        public byte Etage { get; set; }
        public bool Bain { get; set; }
        public bool? Douche { get; set; }
        public bool? Wc { get; set; }
        public byte NbLits { get; set; }
        public short? NumTel { get; set; }

        public virtual ICollection<Reservation> Reservation { get; set; }
        public virtual ICollection<TarifChambre> TarifChambre { get; set; }
    }
}
