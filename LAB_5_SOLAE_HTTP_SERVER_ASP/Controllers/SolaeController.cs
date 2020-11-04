using LAB_5_SOLAE_HTTP_SERVER_ASP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace LAB_5_SOLAE_HTTP_SERVER_ASP.Controllers
{
    [ApiController]
    [Route("api/solaes")]
    public class SolaeController : Controller
    {
        private ApplicationsContext _db;

        public SolaeController(ApplicationsContext db)
        {
            _db = db;

            if (!_db.Solaes.Any())
            {
                _db.Solaes.Add(new Solae{ Name = "Matrix_#1", Value = "{{1, 2, 3}, {4, 5, 6}, {7, 8, 9}}" });
                _db.Solaes.Add(new Solae{ Name = "Matrix_#2", Value = "{{-1, -2, -3}, {-4, -5, -6}, {-7, -8, -9}}" });
                _db.Solaes.Add(new Solae{ Name = "Matrix_#3", Value = "{{-1, 0, 1}, {0, 0, 0}, {1, 0, -1}}" });
                _db.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<Solae> Get()
        {
            return _db.Solaes.ToList();
        }

        [HttpGet("{id}")]
        public Solae Get(int id)
        {
            return _db.Solaes.FirstOrDefault(x => x.Id == id);
        }

        [HttpPost]
        public IActionResult Post(Solae solae)
        {
            if (ModelState.IsValid)
            {
                _db.Solaes.Add(solae);
                _db.SaveChanges();

                return Ok(solae);
            }

            return BadRequest(ModelState);
        }

        [HttpPut]
        public IActionResult Put(Solae solae)
        {
            if (ModelState.IsValid)
            {
                _db.Update(solae);
                _db.SaveChanges();

                return Ok(solae);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var solae = _db.Solaes.FirstOrDefault(x => x.Id == id);

            if (solae != null)
            {
                _db.Solaes.Remove(solae);
                _db.SaveChanges();
            }

            return Ok(solae);
        }
    }
}
