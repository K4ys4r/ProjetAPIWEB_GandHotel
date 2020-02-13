using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class TarifChambre
    {
        public short NumChambre { get; set; }
        public string CodeTarif { get; set; }

        public virtual Tarif CodeTarifNavigation { get; set; }
        public virtual Chambre NumChambreNavigation { get; set; }
    }
}
