using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class Tarif
    {
        public Tarif()
        {
            TarifChambre = new HashSet<TarifChambre>();
        }

        public string Code { get; set; }
        public DateTime DateDebut { get; set; }
        public decimal Prix { get; set; }

        public virtual ICollection<TarifChambre> TarifChambre { get; set; }
    }
}
