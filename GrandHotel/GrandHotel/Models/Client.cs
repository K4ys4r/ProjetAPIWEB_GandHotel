using System;
using System.Collections.Generic;

namespace GrandHotel.Models
{
    public partial class Client
    {
        public Client()
        {
            Facture = new HashSet<Facture>();
            Reservation = new HashSet<Reservation>();
            Telephone = new HashSet<Telephone>();
        }

        public int Id { get; set; }
        public string Civilite { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }
        public bool CarteFidelite { get; set; }
        public string Societe { get; set; }

        public virtual Adresse Adresse { get; set; }
        public virtual ICollection<Facture> Facture { get; set; }
        public virtual ICollection<Reservation> Reservation { get; set; }
        public virtual ICollection<Telephone> Telephone { get; set; }
    }
}
