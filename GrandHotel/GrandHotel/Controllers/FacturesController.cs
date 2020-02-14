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
    public class FacturesController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public FacturesController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET: Factures
        /// <summary>
        /// Fonction GetFactures permet d'avoir la liste des factures à partir d'une date donnée.
        /// Prend trois paramètres.
        /// </summary>
        /// <param name="date1">un DateTime </param>
        /// <param name="date2">un DateTime </param>
        /// <param name="clientId">un integer correspondant l'id du client</param>
        /// <returns>
        /// BadRequest si les paramètres ne sont pas renseignés.
        /// NotFound s'il y a pas de facture associée au client.
        /// Une liste List<Facture> pour le client renseigné.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFactures([FromQuery] DateTime date1, [FromQuery] DateTime date2, [FromQuery] int clientId)
        {
            if (date1 == DateTime.MinValue && date2 == DateTime.MinValue)
                return BadRequest("Aucune date n'était donné!");
            if (clientId == 0)
                return BadRequest("Pas d'id client renseigné!");

            //Si la date2 n'est pas renseigné, on fait un an glissant par défaut
            if (date2 == DateTime.MinValue)
            {
                date2 = date1;
                date1 = date2.AddYears(-1);
            }
            // Ici on s'arrange pour que la date2 soit toujours la plus grande pour que la requête de DbSet soit correcte
            if (date2 < date1)
            {
                DateTime datetemp = date1;
                date1 = date2;
                date2 = datetemp;
            }
            var factures = await _context.Facture.Where(f => f.IdClient == clientId && (f.DateFacture >= date1 && f.DateFacture <= date2)).ToListAsync();

            if (!factures.Any())
            {
                return NotFound("Aucune facture trouvée pour ce client aux dates renseignées!");
            }
            return factures;
        }

        // GET: Factures/5
        /// <summary>
        /// La fonction GetFacture permet d'avoir une facture et ses détails à partir de son id.
        /// Pour avoir les détails (LigneFacture) un Include a été utilisé.
        /// </summary>
        /// <param name="id">Un integer correspondant à l'id de la facture.</param>
        /// <returns>
        /// NotFound si la facture n'existe pas.
        /// Sinon Une Facture avec ses details (LigneFacture).
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Facture>> GetFacture(int id)
        {
            _context.Facture.Include(f => f.LigneFacture).Load();
            var facture = await _context.Facture.FindAsync(id);
            if (facture == null)
            {
                return NotFound("L'Id donné ne correspond pas à aucunes des factures!");
            }
            return facture;
        }



        // PUT: Factures/5
        /// <summary>
        /// Fonction PutFacture permet de mettre à jour les données (DateFacture et CodeModePaiement) d'une facture donnée.
        /// Elle prend deux paramètres.
        /// </summary>
        /// <param name="id">Un integer correspondant à l'id de la facture (de la route).</param>
        /// <param name="newFacture">Une instance de Facture (du corps).</param>
        /// <returns>
        /// BadRequest si l'Id ne correspond pas à la facture. Ou s'il y a une erreur lors de l'enregistrement.
        /// NotFound si l'Id ne correspond à aucune facture.
        /// Ok si mise à jour.
        /// </returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacture(int id, Facture newFacture)
        {
            if (!_context.Facture.Any(f => f.Id == id))
                return NotFound("La facture n'existe pas.");

            if (newFacture.Id > 0 && id != newFacture.Id)
                return BadRequest("L'Id donné en paramètre ne correspond pas à celui de la facture renseigné.");

            var facture = await _context.Facture.FindAsync(id);
            if (facture == null)
            {
                return NotFound("L'Id donné ne correspond pas à aucune des factures!");
            }
            facture.DateFacture = newFacture.DateFacture;
            facture.CodeModePaiement = newFacture.CodeModePaiement;
            _context.Entry(facture).Property("DateFacture").IsModified = true;
            _context.Entry(facture).Property("CodeModePaiement").IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Une erreur s'est produite lors de la mise à jour.\nVeuillez vérifier les valeurs de date et codeModePaiement");
            }

            return Ok("La mise à jour a bien été effectuée.");
        }

        // POST: Factures
        /// <summary>
        /// Fonction PostFacture permet de créer une nouvelle facture. 
        /// </summary>
        /// <param name="facture"> Une instance de Facture. </param>
        /// <returns>
        /// Lien de la facture créee.
        /// BadRequest si l'id de la facture est donnée car il est auto-incrementé.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<Facture>> PostFacture(Facture facture)
        {
            if (facture.Id > 0)
                return BadRequest("L'id de la facture est auto-incrementé donc ce n'est pas indispensable.");
            _context.Facture.Add(facture);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetFacture", new { id = facture.Id }, facture);
        }

        /// <summary>
        /// La fonction PostLigneFacture permet de rajouter une ligneFacture pour une Facture
        /// </summary>
        /// <param name="id"> Un integer correspondant à l'id de la facture. (route) </param>
        /// <param name="lignefacture"> Une instance LigneFacture. (corps) </param>
        /// <returns>
        /// NotFound si la facture n'existe pas.
        /// Ok si la ligneFacture a bien été rajouté.
        /// </returns>
        [HttpPost("{id}")]
        public async Task<ActionResult<Facture>> PostLigneFacture(int id, LigneFacture lignefacture)
        {

            var facture = await _context.Facture.Include(f => f.LigneFacture).Where(f => f.Id == id).FirstOrDefaultAsync();

            if (facture == null)
                return NotFound("L'Id donné ne correspond pas à aucune des factures!");

            /*Ici on récupère la dernière valeur de la clé primaire de la ligneFacture pour la facture.
            car il n'est pas autoincrementé.
            et on l'incrémente de 1.*/
            if (facture.LigneFacture.Any())
            {
                lignefacture.NumLigne = facture.LigneFacture.LastOrDefault().NumLigne + 1;
            }
            else
            {
                lignefacture.NumLigne = 1;
            }
            facture.LigneFacture.Add(lignefacture);
            await _context.SaveChangesAsync();
            return Ok("La ligneFacture a bien été rajouté!");
        }

    }
}
