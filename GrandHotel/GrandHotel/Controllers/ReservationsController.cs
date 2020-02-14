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

        // GET: Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation([FromQuery] DateTime date)
        {
            if (date == DateTime.MinValue)
                return BadRequest("Erreur dans le format du parametre date ex:(Facture?date=2017-01-01)");
            return await _context.Reservation.Where(r => r.Jour == date).ToListAsync();
        }
        [HttpGet("Clients")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservation([FromQuery] int id)
        {
            return await _context.Reservation.Where(r => r.IdClient == id).ToListAsync();
        }

        /*        // GET: Reservations/5
                [HttpGet("{id}")]
                public async Task<ActionResult<Reservation>> GetReservation(int id)
                {
                    var reservation = await _context.Reservation.FindAsync(id);

                    if (reservation == null)
                    {
                        return NotFound();
                    }

                    return reservation;
                }
        */


        // PUT: Reservations?clientId
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut]
        public async Task<IActionResult> PutReservation([FromQuery] int clientId, Reservation reservation)
        {
            var client = await _context.Client.Include(c => c.Reservation).Where(c => c.Id == clientId).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound("Le client avec l'id : " + clientId + " n'est pas enregistré dans notre base de données!");
            }
            var clientReservation = client.Reservation.Where(r => r.Jour == reservation.Jour && r.NumChambre == reservation.NumChambre).FirstOrDefault();
            if (clientReservation == null)
            {
                return NotFound("Le client n'a pas reservé la chambre " + reservation.NumChambre + " pour la date " + reservation.Jour);
            }
            clientReservation.NbPersonnes = reservation.NbPersonnes;
            clientReservation.HeureArrivee = reservation.HeureArrivee;
            clientReservation.Travail = reservation.Travail;
            _context.Entry(clientReservation).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: Reservations?clientId
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation([FromQuery] int clientId, Reservation reservation)
        {
            var client = await _context.Client.Include(c => c.Reservation).Where(c => c.Id == clientId).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound("Le client avec l'id : " + clientId + " n'est pas enregistré dans notre base de données!");
            }
            var chambreReserve = await _context.Reservation.Where(r => r.NumChambre == reservation.NumChambre && r.Jour == reservation.Jour).FirstOrDefaultAsync();
            if (chambreReserve != null)
            {
                return BadRequest("Le chambre " + reservation.NumChambre + " n'est pas disponible pour la date " + reservation.Jour);
            }

            if (!_context.Calendrier.Any(d => d.Jour == reservation.Jour))
            {
                var newDateCalendrier = new Calendrier() { Jour = reservation.Jour };
                _context.Calendrier.Add(newDateCalendrier);
            }
            client.Reservation.Add(reservation);

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

            return CreatedAtAction("GetReservation", new { id = reservation.NumChambre }, reservation);
        }

        // DELETE: Reservations?clientId
        [HttpDelete]
        public async Task<ActionResult<Reservation>> DeleteReservation([FromQuery] int clientId, Reservation reservation)
        {
            var clientReservation = _context.Reservation.Where(r => r.IdClient == clientId && r.Jour == reservation.Jour && r.NumChambre == reservation.NumChambre).FirstOrDefault();
            if (clientReservation == null)
            {
                return NotFound("La reservation n'est pas trouvée!");
            }
            _context.Reservation.Remove(clientReservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        private bool ReservationExists(short id)
        {
            return _context.Reservation.Any(e => e.NumChambre == id);
        }
    }
}
