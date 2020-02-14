using GrandHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrandHotel.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public ReservationsController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET: Reservations?date=2017-01-01
        /// <summary>
        /// Fonction GetReservations permet de récupérer toutes les réservations pour une date donnée.
        /// </summary>
        /// <param name="date"> un DateTime </param>
        /// <returns>
        /// BadRequest si le paramète date n'est pas renseigné.
        /// Sinon List<Reservation>.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations([FromQuery] DateTime date)
        {
            if (date == DateTime.MinValue)
                return BadRequest("Veuillez rajouter une date dans ce format: (Reservations?date=2017-01-01)");
            
            var reservations = await _context.Reservation.Where(r => r.Jour == date).ToListAsync();
            if (!reservations.Any())
                return NotFound("Aucune réservation trouvée.");

            return reservations;
        }
        
        // PUT: Reservations/Clients?id=5
        /// <summary>
        /// Fonction GetReservations permet d'avoir la liste des réservations d'un client.
        /// </summary>
        /// <param name="id"> Un integer correspondant à l'id du client.</param>
        /// <returns>
        /// NotFound si le client n'existe pas.
        /// BadRequest si le paramètre id n'est pas renseigné.
        /// NotFound si aucune réservation trouvée.
        /// Sinon List<Reservation>
        /// </returns>
        [HttpGet("Clients")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations([FromQuery] int id)
        {
            if (id == 0)
                return BadRequest("Pas d'Id client renseigné.");
            
            if (!_context.Client.Any(c => c.Id == id))
                return NotFound("Le client ayant l'id : " + id + " n'est pas enregistré dans notre base de données.");
         
            var reservations =  await _context.Reservation.Where(r => r.IdClient == id).ToListAsync();
            if (!reservations.Any())
                return NotFound("Aucune réservation trouvée.");

            return reservations;
        }

        // PUT: Reservations?clientId=5
        /// <summary>
        /// Fonction PutReservation permet de mettre à jour une reservation.
        /// </summary>
        /// <param name="clientId"> Un integer correspondant à l'id du client. </param>
        /// <param name="reservation"> Une instance Reservation. </param>
        /// <returns>
        /// BadRequest si le paramètre id n'est pas renseigné.
        /// NotFound si le client n'existe pas.
        /// NotFound si le client n'a pas reservé la chambre pour une date donnée.
        /// Ok si les modifications sont bien faites.
        /// </returns>
        [HttpPut]
        public async Task<IActionResult> PutReservation([FromQuery] int clientId, Reservation reservation)
        {
            if (clientId == 0)
                return BadRequest("Pas d'Id client renseigné");
            if (reservation.Jour == DateTime.MinValue || reservation.NumChambre == 0)
                return BadRequest("Le numero du chambre et la date doivent être renseignés");
      
            if (!_context.Client.Any(c => c.Id == clientId))
                return NotFound("Le client avec l'id : " + clientId + " n'est pas enregistré dans notre base de données!");

            var clientReservation = _context.Reservation.Where(r => r.Jour == reservation.Jour && r.NumChambre == reservation.NumChambre).FirstOrDefault();
            if (clientReservation == null)
            {
                return NotFound("Le client n'a pas reservé la chambre " + reservation.NumChambre + " pour la date " + reservation.Jour);
            }
            clientReservation.NbPersonnes = reservation.NbPersonnes;
            clientReservation.HeureArrivee = reservation.HeureArrivee;
            clientReservation.Travail = reservation.Travail;
            _context.Entry(clientReservation).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok("Les changements ont été pris en compte");
        }

        // POST: Reservations?clientId=5
        /// <summary>
        /// Fonction PosteReservation permet de creer une reservation pour un client donné
        /// </summary>
        /// <param name="clientId">Un integre coresspondant l'id du client</param>
        /// <param name="reservation">Une instance Reservation</param>
        /// <returns>
        /// BadRequest si le parametre id ne pas renseigné
        /// NotFound si le client n'existe pas 
        /// BadRequest si la chambre n'est pas disponible
        /// BadRequest si le numerode la chambre n'exist pas
        /// un lien pour la reservation créée
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation([FromQuery] int clientId, Reservation reservation)
        {
            if (clientId == 0)
                return BadRequest("Pas d'Id client renseigné");

            if (!_context.Client.Any(c => c.Id == clientId))
                return NotFound("Le client avec l'id : " + clientId + " n'est pas enregistré dans notre base de données!");
            

            var chambreReserve = await _context.Reservation.Where(r => r.NumChambre == reservation.NumChambre && r.Jour == reservation.Jour).FirstOrDefaultAsync();
            if (chambreReserve != null)
            {
                return BadRequest("Le chambre " + reservation.NumChambre + " n'est pas disponible pour la date " + reservation.Jour);
            }

            //Si le Jour n'existe pas dans Calendrier on le crée.
            if (!_context.Calendrier.Any(d => d.Jour == reservation.Jour))
            {
                var newDateCalendrier = new Calendrier() { Jour = reservation.Jour };
                _context.Calendrier.Add(newDateCalendrier);
            }

            if (reservation.IdClient != clientId)
                reservation.IdClient = clientId;

            _context.Reservation.Add(reservation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!_context.Chambre.Any(c => c.Numero == reservation.NumChambre))
                {
                    return BadRequest("Il y a pas une chambre qui porte le numero " + reservation.NumChambre);
                }
            }
            return CreatedAtAction("GetReservations", new { id = reservation.NumChambre }, reservation);
        }

        // DELETE: Reservations?clientId
        /// <summary>
        /// Fonction DeleteReservation permet de supprimer une reservation pour un client donné
        /// </summary>
        /// <param name="clientId">Un integre coresspondant l'id du client</param>
        /// <param name="reservation">Une instance Reservation</param>
        /// <returns>
        /// BadRequest si le parametre id ne pas renseigné
        /// NotFound si le client n'existe pas ou la reservation n'est pas trouvée
        /// Ok si la supprission est faite
        /// </returns>
        [HttpDelete]
        public async Task<ActionResult<Reservation>> DeleteReservation([FromQuery] int clientId, Reservation reservation)
        {
            if (clientId == 0)
                return BadRequest("Pas d'Id client renseigné");

            if (!_context.Client.Any(c => c.Id == clientId))
                return NotFound("Le client avec l'id : " + clientId + " n'est pas enregistré dans notre base de données!");

            var clientReservation = _context.Reservation.Where(r => r.IdClient == clientId && r.Jour == reservation.Jour && r.NumChambre == reservation.NumChambre).FirstOrDefault();
            if (clientReservation == null)
            {
                return NotFound("La reservation n'est pas trouvée!");
            }
            _context.Reservation.Remove(clientReservation);
            await _context.SaveChangesAsync();
            return Ok("La reservation a bien été supprimée");
        }

    }
}
