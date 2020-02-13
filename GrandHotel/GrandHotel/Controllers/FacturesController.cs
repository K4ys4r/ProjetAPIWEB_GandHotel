﻿using System;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetFacture([FromQuery] DateTime date1, [FromQuery] DateTime date2, [FromQuery] int client)
        {
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
            var factures = await _context.Facture.Where(f => f.IdClient == client && (f.DateFacture >= date1 && f.DateFacture <= date2)).ToListAsync();

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



        // PUT: api/Factures/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFacture(int id, Facture facture)
        {
            if (id != facture.Id)
            {
                return BadRequest();
            }

            _context.Entry(facture).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FactureExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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




        // DELETE: api/Factures/5
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

        private bool FactureExists(int id)
        {
            return _context.Facture.Any(e => e.Id == id);
        }
    }
}