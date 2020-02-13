using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class LigneFacture
    {
        public int IdFacture { get; set; }
        public int NumLigne { get; set; }
        public short Quantite { get; set; }
        public decimal MontantHt { get; set; }
        public decimal TauxTva { get; set; }
        public decimal TauxReduction { get; set; }

        internal virtual Facture IdFactureNavigation { get; set; }
    }
}
