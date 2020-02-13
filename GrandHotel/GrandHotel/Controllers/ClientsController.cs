using GrandHotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrandHotel.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public ClientsController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET: Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClient()
        {
            return await _context.Client.ToListAsync();
        }

        // GET: Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            _context.Client.Include(c => c.Telephone).Load();
            _context.Client.Include(c => c.Adresse).Load();
            var client = await _context.Client.FindAsync(id);
            if (client == null)
            {
                return NotFound("Le client n'est pas enregistré dans notre base de données!");
            }
            return client;
        }

        // PUT: Clients/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient(int id, Telephone tel)
        {
            if (ClientExists(id))
            {
                Client client = await _context.Client.FindAsync(id);
                client.Telephone.Add(tel);
                _context.Telephone.Add(client.Telephone.FirstOrDefault());
                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound("L'Id donné ne correspond pas à aucuns de nos clients!");
            }
            return NoContent();
            /*            try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ClientExists(id))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
            return NoContent();
            */
        }

        // POST: Clients
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            _context.Client.Add(client);
            if (client.Adresse != null)
            {
                _context.Adresse.Add(client.Adresse);
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClient", new { id = client.Id }, client);
        }

        // DELETE: Clients/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Client>> DeleteClient(int id)
        {
            var client = await _context.Client.Include(c => c.Adresse).Include(t => t.Telephone).Where(c => c.Id == id).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound("L'Id donné ne correspond pas à aucun de nos clients!");
            }

            if (!_context.Facture.Any(c => c.IdClient == client.Id) || !_context.Reservation.Any(r => r.IdClient == client.Id))
            {
                if (client.Telephone.Any())
                {
                    _context.Telephone.RemoveRange(client.Telephone);
                }
                if (client.Adresse != null)
                {
                    _context.Adresse.Remove(client.Adresse);
                }
                _context.Client.Remove(client);
            }
            else
            {
                return BadRequest("Le client ne peut pas être supprimer car il est associé à des factures ou des réservations!");
            }

            await _context.SaveChangesAsync();

            return Ok("Le client a bien été supprimé");
        }

        private bool ClientExists(int id)
        {
            return _context.Client.Any(e => e.Id == id);
        }
    }
}
