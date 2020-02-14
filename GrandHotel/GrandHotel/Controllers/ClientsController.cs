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
        /// Fonction get les informations des clients
        /// </summary>
        /// <returns> renvoie une liste de List<Client></Client>s</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClient()
        {
            return await _context.Client.ToListAsync();
        }

        // GET: Clients/5
        /// <summary>
        /// La fonction prend un seul parametre du Header
        /// pour avoir les information de Telephone et l'Adresse, un Include a été utilisé
        /// </summary>
        /// <param name="id">Un integre qui correspond è l'id du client</param>
        /// <returns>La fonction renvoie un client avec la liste de numeros de telephones ainsi que son addresse</returns>
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
        /// La fonction PostClient premet de creer un numero de telephone à un client donné
        /// La fonction prend deux parametres
        /// </summary>
        /// <param name="id">integre correspond à l'id du client</param>
        /// <param name="tel">une instance de Telephone</param>
        /// <returns>Nocontent si le numero du telephone a été créé. 
        ///          NotFound si le client n'est pas listé dans la base de données
        ///          BadRequest si le numero de telephone (sa clé primaire) est déjà utilisé</returns>
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
                return NotFound("L'Id donné ne correspond pas à aucuns de clients!");
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            
            catch (DbUpdateException)
            {
                return BadRequest("un conflit est produit lors de la mise à jours du à clé primaire qui est déjà utilisée");
            }
            return NoContent();
        }

        // POST: Clients
        /// <summary>
        /// La fonction PostClient permet de créer un nouveau client et son addresse 
        /// </summary>
        /// <param name="client"> un instance Client et sa proprité addresse une instance Adresse</param>
        /// <returns>renvoie un lien pour le client qui a été crée</returns>
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
        /// La fonction DeleteClient permet de supprimer un client, son addresse et sa liste des telephones de la base de données
        /// si il est pas assosié à des factures ou des reservation
        /// </summary>
        /// <param name="id">un integre correspond l'id du client</param>
        /// <returns>
        /// la fonction renvoie NotFound si le client n'est pas trouvé
        /// renvoie BadRequest si le client est associé à des factures ou à des reservations
        /// renvoie Ok si le client, son addresse et ses telephones  ont été bien supprimés
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Client>> DeleteClient(int id)
        {
            var client = await _context.Client.Include(c => c.Adresse).Include(t => t.Telephone).Where(c => c.Id == id).FirstOrDefaultAsync();
            if (client == null)
            {
                return NotFound("L'Id donné ne correspond pas à aucun de nos clients!");
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
