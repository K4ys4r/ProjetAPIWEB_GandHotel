using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class Facture
    {
        public Facture()
        {
            LigneFacture = new HashSet<LigneFacture>();
        }

        public int Id { get; set; }
        public int IdClient { get; set; }
        public DateTime DateFacture { get; set; }
        public DateTime? DatePaiement { get; set; }
        public string CodeModePaiement { get; set; }

        public virtual ModePaiement CodeModePaiementNavigation { get; set; }
        public virtual Client IdClientNavigation { get; set; }
        public virtual ICollection<LigneFacture> LigneFacture { get; set; }
    }
}
