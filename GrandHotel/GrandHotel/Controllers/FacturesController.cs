using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GrandHotel.Models;

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
        /// Fonction GetFactures permet d'avoir la liste des factures à partir d'une date donnée
        /// Fonction prend trois parametres
        /// </summary>
        /// <param name="date1">un DateTime </param>
        /// <param name="date2">un DateTime </param>
        /// <param name="clientId">un integre correspondant l'id du client</param>
        /// <returns>
        /// BadRequest si les parametres ne sont pas renseignées
        /// NotFound si il y a pas des factures trouvées pour le client
        /// List<Facture> pour le client donné
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFactures([FromQuery] DateTime date1, [FromQuery] DateTime date2, [FromQuery] int clientId)
        {
            if (date1 == DateTime.MinValue && date2 == DateTime.MinValue)
                return BadRequest("Aucune date n'était donné!");
            if (clientId == 0)
                return BadRequest("Pas d'id client renseigné!");

            //Si le date2 n'est pas donné on fait un an glissant par defaut
            if (date2 == DateTime.MinValue)
            {
                date2 = date1;
                date1 = date2.AddYears(-1);
            }
            // Ici on s'arrange que toujours le date2 soit le plus grand pour que la requet de DbSet soit bonne
            if (date2<date1)
            {
                DateTime datetemp = date1;
                date1 = date2;
                date2 = datetemp;
            }
            var factures = await _context.Facture.Where(f => f.IdClient == clientId && (f.DateFacture >= date1 && f.DateFacture <= date2)).ToListAsync();

            if (!factures.Any())
            {
                return NotFound("Aucune factures trouvées pour ce client pour les dates renseignées!");
            }
            return factures;
        }

        // GET: Factures/5
        /// <summary>
        /// La fonction GetFacture permet d'avoir une facture et ses details à partire de son id
        /// Pour avoir les details (LigneFacture) un Include a été utilisé.
        /// </summary>
        /// <param name="id">Un integre correspondant à l'id du facture</param>
        /// <returns>
        /// NotFound si la facture n'existe pas 
        /// une Facture avec ses detatil LigneFacture
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
        /// Fonction PutFacture permet de mettre à jours les données (DateFacture et CodeMoePaiement) d'une factue donnée.
        /// elle prend deux parametres
        /// </summary>
        /// <param name="id">un integre correspondantt l'id de la facture</param>
        /// <param name="newFacture">une instance de Facture</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacture(int id, Facture newFacture)
        {
            if (newFacture.Id>0 && id != newFacture.Id)
                return BadRequest("L'Id donné en parametre ne correspond pas à celui de la Facture renseigné");

            var facture = await _context.Facture.FindAsync(id);
            if (facture == null)
            {
                return NotFound("L'Id donné ne correspond pas à aucunes des factures!");
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
                return BadRequest("une erreur est produite en effectuant la mis à jour.\nVerifier le valeurs des date et du codeModePaiement");
            }

            return Ok("Mise à jour est bien terminée");
        }

        // POST: Factures
        /// <summary>
        /// Fonction PostFacture permet de créer une nouvelle facteur 
        /// </summary>
        /// <param name="facture"> une instance de Facture</param>
        /// <returns>
        /// Lien pour la facture crée
        /// BadRequest si l'id de la facture est donnée car il est autoincrementé
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<Facture>> PostFacture(Facture facture)
        {
            if (facture.Id > 0)
                return BadRequest("L'id de la facture est autoincrementé donc ce n'est pas indispensable");
            _context.Facture.Add(facture);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetFacture", new { id = facture.Id }, facture);
        }

        /// <summary>
        /// La fonction PostLigneFacture permet de rajouter une ligneFacture pour une Facture
        /// </summary>
        /// <param name="id">un integre coresspondant l'id de la facture</param>
        /// <param name="lignefacture">une instance LigneFacture</param>
        /// <returns>
        /// NotFound si la facture n'exist pas 
        /// Ok si la ligneFacture est a bien été rajouté.
        /// </returns>
        [HttpPost("{id}")]
        public async Task<ActionResult<Facture>> PostLigneFacture(int id, LigneFacture lignefacture)
        {

            var facture = await _context.Facture.Include(f => f.LigneFacture).Where(f =>f.Id == id).FirstOrDefaultAsync();

            if (facture == null)
                return NotFound("L'Id donné ne correspond pas à aucunes des factures!");
          
            //Ici on recupere la derniere valeur de la clé primer de la ligneFacture pour la facture
            //Car il n'est pas autoincrementé.
            // et on l'increment de 1.
            if (facture.LigneFacture.Any())
            {
                lignefacture.NumLigne = facture.LigneFacture.LastOrDefault().NumLigne+1;
            }
            else
            {
                lignefacture.NumLigne = 1;
            }
            facture.LigneFacture.Add(lignefacture);
            await _context.SaveChangesAsync();
            return Ok("La ligneFacture est bien rajouté!");
        }

    }
}
