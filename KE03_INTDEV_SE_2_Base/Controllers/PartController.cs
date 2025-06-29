// Importeert alle benodigde namespaces voor collections, diagnostiek, I/O, LINQ, async, data toegang, models en MVC
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer;
using DataAccessLayer.Models;
using KE03_INTDEV_SE_2_Base.Models;
using KE03_INTDEV_SE_2_Base.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    /// <summary>
    /// Controller voor onderdelen (parts) beheer in het MatrixInc systeem.
    /// Handelt het bekijken, aanmaken, bewerken en verwijderen van onderdelen af.
    /// Onderdelen kunnen gekoppeld worden aan producten in een many-to-many relatie.
    /// </summary>
    public class PartController : Controller
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor PartController. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public PartController(MatrixIncDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Toont een overzicht van alle onderdelen in het systeem.
        /// Redirect naar ProductController voor uniforme overzicht weergave.
        /// </summary>
        /// <returns>Redirect naar Product Index</returns>
        // GET: Parts
        public async Task<IActionResult> Index()
        {
            // Creëer view model met onderdelen en gerelateerde producten
            var viewModel = new PartOverviewViewModel
            {
                Products = await _context.Products.Include(p => p.Parts).ToListAsync(),
                Parts = await _context.Parts.Include(p => p.Products).ToListAsync()
            };

            // Redirect naar ProductController voor consistente weergave
            return RedirectToAction("Index", "Product");
        }

        /// <summary>
        /// Toont gedetailleerde informatie van een specifiek onderdeel.
        /// Inclusief technische specificaties en gekoppelde producten.
        /// </summary>
        /// <param name="id">Unieke identifier van het onderdeel</param>
        /// <returns>Details view met Part model of NotFound</returns>
        // GET: Parts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Valideer of ID parameter is meegegeven
            if (id == null)
            {
                return NotFound();
            }

            // Zoek onderdeel in database
            var part = await _context.Parts
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Controleer of onderdeel bestaat
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        /// <summary>
        /// Toont het formulier voor het aanmaken van een nieuw onderdeel.
        /// </summary>
        /// <returns>Create view met leeg Part model</returns>
        // GET: Parts/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Verwerkt het aanmaken van een nieuw onderdeel via POST request.
        /// Valideert onderdeel gegevens en slaat deze op in de database.
        /// Beveiligd tegen CSRF attacks en overposting door specifieke property binding.
        /// </summary>
        /// <param name="part">Part object met alleen toegestane properties (Id, Name, Description, Price, Stock)</param>
        /// <returns>Redirect naar Index bij succes, anders Create view met validation errors</returns>
        // POST: Parts/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Stock")] Part part)
        {
            // Valideer model state (data annotations, required fields, etc.)
            if (ModelState.IsValid)
            {
                // Voeg onderdeel toe aan database
                _context.Add(part);
                await _context.SaveChangesAsync();
                
                // Redirect naar overzicht pagina na succesvolle creatie
                return RedirectToAction(nameof(Index));
            }
            
            // Bij validatie fouten, toon create form opnieuw met errors
            return View(part);
        }

        /// <summary>
        /// Toont het formulier voor het bewerken van een bestaand onderdeel.
        /// Laadt de huidige onderdeel gegevens voor modificatie.
        /// </summary>
        /// <param name="id">Unieke identifier van het onderdeel om te bewerken</param>
        /// <returns>Edit view met Part model of NotFound</returns>
        // GET: Parts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Valideer of ID parameter is meegegeven
            if (id == null)
            {
                return NotFound();
            }

            // Zoek onderdeel in database
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            
            return View(part);
        }

        // POST: Parts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Stock")] Part part)
        {
            if (id != part.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPart = await _context.Parts.FindAsync(id);
                    if (existingPart == null)
                    {
                        return NotFound();
                    }

                    // Update all fields except image
                    existingPart.Name = part.Name;
                    existingPart.Description = part.Description;
                    existingPart.Price = part.Price;
                    existingPart.Stock = part.Stock;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartExists(part.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(part);
        }

        // GET: Parts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var part = await _context.Parts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (part == null)
            {
                return NotFound();
            }

            return View(part);
        }

        // POST: Parts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part != null)
            {
                _context.Parts.Remove(part);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Parts/UploadImage/5
        [HttpPost]
        public async Task<IActionResult> UploadImage(int id, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("Geen afbeelding geüpload");
            }

            var part = await _context.Parts.FindAsync(id);
            if (part == null)
            {
                return NotFound();
            }

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                part.Image = memoryStream.ToArray();
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool PartExists(int id)
        {
            return _context.Parts.Any(e => e.Id == id);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
