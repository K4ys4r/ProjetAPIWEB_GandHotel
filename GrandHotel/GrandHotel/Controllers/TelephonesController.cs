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
    public class TelephonesController : ControllerBase
    {
        private readonly GrandHotelContext _context;

        public TelephonesController(GrandHotelContext context)
        {
            _context = context;
        }

        // GET: api/Telephones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Telephone>>> GetTelephone()
        {
            return await _context.Telephone.ToListAsync();
        }

        // GET: api/Telephones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Telephone>> GetTelephone(string id)
        {
            var telephone = await _context.Telephone.FindAsync(id);

            if (telephone == null)
            {
                return NotFound();
            }

            return telephone;
        }

        // PUT: api/Telephones/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTelephone(string id, Telephone telephone)
        {
            if (id != telephone.Numero)
            {
                return BadRequest();
            }

            _context.Entry(telephone).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TelephoneExists(id))
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

        // POST: api/Telephones
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Telephone>> PostTelephone(Telephone telephone)
        {
            _context.Telephone.Add(telephone);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TelephoneExists(telephone.Numero))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTelephone", new { id = telephone.Numero }, telephone);
        }

        // DELETE: api/Telephones/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Telephone>> DeleteTelephone(string id)
        {
            var telephone = await _context.Telephone.FindAsync(id);
            if (telephone == null)
            {
                return NotFound();
            }

            _context.Telephone.Remove(telephone);
            await _context.SaveChangesAsync();

            return telephone;
        }

        private bool TelephoneExists(string id)
        {
            return _context.Telephone.Any(e => e.Numero == id);
        }
    }
}
