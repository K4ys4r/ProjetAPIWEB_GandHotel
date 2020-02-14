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
        /// <summary>
        /// Fonction qui affiche les informations des clients
        /// </summary>
        /// <returns> Renvoie une liste de type List<Client> </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClient()
        {
            return await _context.Client.ToListAsync();
        }

        // GET: Clients/5
        /// <summary>
        /// La fonction récupère son paramètre dans la route
        /// pour avoir ses informations. La fonction Include a été utilisé pour récupérer téléphones et adresse.
        /// </summary>
        /// <param name="id"> Un integer qui correspond à l'id du client </param>
        /// <returns> La fonction renvoie un client avec la liste des numéros de telephone ainsi que son adresse </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
//            _context.Client.Include(c => c.Telephone).Include(c => c.Adresse).Include(c => c.Reservation).Load();
            _context.Client.Include(c => c.Telephone).Include(c => c.Adresse).Load();
            var client = await _context.Client.FindAsync(id);
            if (client == null)
            {
                return NotFound("Le client n'est pas enregistré dans notre base de données!");
            }
            return client;
        }

        // POST: Clients/5
        /// <summary>
        /// La fonction PostClient permet de créer un numéro de téléphone pour un client donné.
        /// La fonction prend deux paramètres: id à partir de la route et le téléphone à partir du corps.
        /// </summary>
        /// <param name="id"> Integer correspondant  à l'id du client </param>
        /// <param name="tel"> Instance de Telephone </param>
        /// <returns> NoContent si le numéro du téléphone a été créé. 
        ///           NotFound si le client n'est pas listé dans la base de données.
        ///           BadRequest si le numero de telephone (sa clé primaire) existe déjà. </returns>
        [HttpPost("{id}")]
        public async Task<IActionResult> PostClient(int id, Telephone tel)
        {
            if (ClientExists(id))
            {
                if (tel.IdClient != id)
                    tel.IdClient = id;
                _context.Telephone.Add(tel);
            }
            else
            {
                return NotFound("L'Id donné ne correspond à aucun de nos clients!");
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            
            catch (DbUpdateException)
            {
                return BadRequest("Un conflit s'est produit lors de la mise à jour de la clé primaire. Celle-ci est déjà utilisée.");
            }
            return NoContent();
        }

        // POST: Clients
        /// <summary>
        /// La fonction PostClient permet de créer un nouveau client avec son adresse. 
        /// </summary>
        /// <param name="client"> Une instance Client et sa propriété adresse (instance Adresse) </param>
        /// <returns> Renvoie un lien pour le client qui a été crée </returns>
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
        /// <summary>
        /// La fonction DeleteClient permet de supprimer un client, son adresse et sa liste de numéro de téléphone de la base de données
        /// si celui-ci n'est pas assosié à des factures ou à des reservations
        /// </summary>
        /// <param name="id"> Un integer correspondant à l'id du client. </param>
        /// <returns>
        /// La fonction renvoie NotFound si le client n'est pas trouvé.
        /// Renvoie BadRequest si le client est associé à des factures ou à des reservations.
        /// Renvoie Ok si le client, son adresse et ses téléphones ont été bien supprimés.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Client>> DeleteClient(int id)
        {
            var client = await _context.Client.Include(c => c.Adresse).Include(t => t.Telephone).Where(c => c.Id == id).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound("L'Id donné ne correspond à aucun de nos clients!");
            }

            if (!_context.Facture.Any(c => c.IdClient == client.Id) && !_context.Reservation.Any(r => r.IdClient == client.Id))
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
                return BadRequest("Le client ne peut pas être supprimé car il est associé à des factures ou des réservations!");
            }

            await _context.SaveChangesAsync();

            return Ok("Le client a bien été supprimé!");
        }

        private bool ClientExists(int id)
        {
            return _context.Client.Any(e => e.Id == id);
        }
    }
}
