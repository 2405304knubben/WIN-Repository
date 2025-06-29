// Importeert alle benodigde namespaces voor collections, diagnostiek, LINQ, async, data toegang, models, en MVC
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Controller voor product gerelateerde functionaliteit in het MatrixInc systeem.
    /// Handelt het bekijken, aanmaken, bewerken en verwijderen van producten af.
    /// Werkt samen met de VoorraadController voor voorraad overzichten.
    /// </summary>
    public class ProductController : Controller
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor ProductController. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public ProductController(MatrixIncDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Toont een overzicht van alle producten en onderdelen.
        /// Redirect naar VoorraadController voor consistente voorraad weergave.
        /// </summary>
        /// <returns>Redirect naar Voorraad Index</returns>
        // GET: Products
        public async Task<IActionResult> Index()
        {
            // Haal producten en onderdelen op inclusief gerelateerde data
            var viewModel = new ProductOverviewViewModel
            {
                Products = await _context.Products.Include(p => p.Parts).ToListAsync(),
                Parts = await _context.Parts.Include(p => p.Products).ToListAsync()
            };

            // Redirect naar VoorraadController voor uniforme voorraad weergave
            return RedirectToAction("Index", "Voorraad");
        }

        /// <summary>
        /// Toont gedetailleerde informatie van een specifiek product.
        /// Gebruikt voor product detail pagina's.
        /// </summary>
        /// <param name="id">Unieke identifier van het product</param>
        /// <returns>Details view met Product model of NotFound</returns>
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Valideer of ID is meegegeven
            if (id == null)
            {
                return NotFound();
            }

            // Zoek product in database
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Controleer of product bestaat
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        /// <summary>
        /// Toont het formulier voor het aanmaken van een nieuw product.
        /// </summary>
        /// <returns>Create view met leeg Product model</returns>
        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Verwerkt het aanmaken van een nieuw product via POST request.
        /// Valideert input data en slaat het product op in de database.
        /// Retourneert JSON response voor AJAX calls of redirect voor normale requests.
        /// Beveiligd tegen CSRF attacks en overposting.
        /// </summary>
        /// <param name="product">Product object met alleen veilige properties (Name, Description, Price, Stock)</param>
        /// <returns>JSON response met success status en product ID, of View bij fout</returns>
        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Stock")] Product product)
        {
            // Valideer model state (data annotations, required fields, etc.)
            if (ModelState.IsValid)
            {
                // Voeg product toe aan database
                _context.Add(product);
                await _context.SaveChangesAsync();
                
                // Retourneer JSON response voor AJAX calls met success status
                return Json(new { success = true, productId = product.Id });
            }
            
            // Bij validatie fouten, toon create form opnieuw met errors
            return View(product);
        }

        /// <summary>
        /// Toont het formulier voor het bewerken van een bestaand product.
        /// </summary>
        /// <param name="id">Unieke identifier van het product om te bewerken</param>
        /// <returns>Edit view met Product model of NotFound</returns>
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Valideer of ID is meegegeven
            // Valideer of ID is meegegeven
            if (id == null)
            {
                return NotFound();
            }

            // Zoek product in database
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }

        /// <summary>
        /// Verwerkt het bewerken van een bestaand product via POST request.
        /// Gebruikt selectieve property binding om overposting attacks te voorkomen.
        /// Bewaart bestaande product afbeelding tijdens het update proces.
        /// </summary>
        /// <param name="id">ID van het product om te bewerken</param>
        /// <param name="product">Product object met safe properties voor binding</param>
        /// <returns>JSON response voor AJAX calls of View bij validatie fouten</returns>
        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen CSRF attacks
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Stock")] Product product)
        {
            // Valideer dat ID's overeenkomen (anti-tampering)
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Haal bestaand product op uit database
                    var existingProduct = await _context.Products.FindAsync(id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Update alleen de noodzakelijke velden, behoud product afbeelding
                    // Dit voorkomt verlies van binary data tijdens form submissions
                    existingProduct.Name = product.Name;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Stock = product.Stock;

                    await _context.SaveChangesAsync();
                    
                    // Retourneer JSON response voor AJAX calls
                    return Json(new { success = true });
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw voor andere concurrency issues
                    }
                }
            }
            
            // Bij model validatie fouten, toon edit form opnieuw
            return View(product);
        }

        /// <summary>
        /// Toont bevestigingspagina voor het verwijderen van een product.
        /// Geeft product details weer voor final review.
        /// </summary>
        /// <param name="id">ID van het product om te verwijderen</param>
        /// <returns>Delete confirmation view of NotFound</returns>
        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Haal product op voor weergave in bevestiging
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        /// <summary>
        /// Verwerkt de definitieve verwijdering van een product.
        /// LET OP: Controleert niet op afhankelijke OrderProducts! 
        /// In productie zou foreign key constraint errors kunnen optreden.
        /// </summary>
        /// <param name="id">ID van het product om te verwijderen</param>
        /// <returns>Redirect naar Index</returns>
        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // TODO: In productie - controleer op afhankelijke bestellingen
                // voor referential integrity
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==================== UTILITY METHODS ====================

        /// <summary>
        /// Hulpmethode om te controleren of een product bestaat.
        /// Gebruikt voor concurrency controle in Edit operaties.
        /// </summary>
        /// <param name="id">ID van het product om te controleren</param>
        /// <returns>True als product bestaat, anders false</returns>
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        /// <summary>
        /// Error handling voor onverwachte fouten in de Products controller.
        /// Toont generieke error pagina met tracking informatie.
        /// </summary>
        /// <returns>Error view met ErrorViewModel</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
