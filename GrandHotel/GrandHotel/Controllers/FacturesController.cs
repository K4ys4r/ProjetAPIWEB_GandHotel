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
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFactures([FromQuery] DateTime date1, [FromQuery] DateTime date2, [FromQuery] int clientId)
        {
            if (date1 == DateTime.MinValue && date2 == DateTime.MinValue)
                return BadRequest("Aucune date n'était donné!");
            if (date2 == DateTime.MinValue)
            {
                date2 = date1;
                date1 = date2.AddYears(-1);
            }
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacture(int id, Facture newFacture)
        {

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

            return NoContent();
        }

        // POST: Factures
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Facture>> PostFacture(Facture facture)
        {
            _context.Facture.Add(facture);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetFacture", new { id = facture.Id }, facture);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<Facture>> PostFacture(int id, LigneFacture lignefacture)
        {

            var facture = await _context.Facture.Include(f => f.LigneFacture).Where(f =>f.Id == id).FirstOrDefaultAsync();

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
            return CreatedAtAction("GetFacture", new { id = facture.Id }, facture);
        }



/*        // DELETE: api/Factures/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Facture>> DeleteFacture(int id)
        {
            var facture = await _context.Facture.FindAsync(id);
            if (facture == null)
            {
                return NotFound();
            }

            _context.Facture.Remove(facture);
            await _context.SaveChangesAsync();

            return facture;
        }
*/
        private bool FactureExists(int id)
        {
            return _context.Facture.Any(e => e.Id == id);
        }
    }
}
