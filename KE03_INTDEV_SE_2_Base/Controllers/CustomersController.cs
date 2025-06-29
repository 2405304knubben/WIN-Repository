// Importeert alle benodigde namespaces voor collections, LINQ, async, MVC, Entity Framework en data models
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer;
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base
{
    /// <summary>
    /// Controller voor klantbeheer in het MatrixInc systeem.
    /// Biedt volledige CRUD functionaliteit voor het beheren van klantgegevens.
    /// Volgt standaard MVC patronen voor consistente gebruikerservaring.
    /// </summary>
    public class CustomersController : Controller
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor CustomersController. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public CustomersController(MatrixIncDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Toont een overzicht van alle klanten in het systeem.
        /// Haalt alle klanten op uit de database voor weergave in een tabel.
        /// </summary>
        /// <returns>Index view met lijst van alle klanten</returns>
        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        /// <summary>
        /// Toont gedetailleerde informatie van een specifieke klant.
        /// Inclusief alle gerelateerde bestellingen en order history.
        /// </summary>
        /// <param name="id">Unieke identifier van de klant</param>
        /// <returns>Details view met Customer model of NotFound</returns>
        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Valideer of ID parameter is meegegeven
            if (id == null)
            {
                return NotFound();
            }

            // Zoek klant in database
            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            
            // Controleer of klant bestaat
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        /// <summary>
        /// Toont het formulier voor het aanmaken van een nieuwe klant.
        /// </summary>
        /// <returns>Create view met leeg Customer model</returns>
        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Verwerkt het aanmaken van een nieuwe klant via POST request.
        /// Valideert klantgegevens en slaat deze op in de database.
        /// Beveiligd tegen CSRF attacks en overposting door specifieke property binding.
        /// </summary>
        /// <param name="customer">Customer object met alleen toegestane properties (Id, Name, Address, Active)</param>
        /// <returns>Redirect naar Index bij succes, anders Create view met validation errors</returns>
        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery
        public async Task<IActionResult> Create([Bind("Id,Name,Address,Active")] Customer customer)
        {
            // Valideer model state (data annotations, required fields, etc.)
            if (ModelState.IsValid)
            {
                // Voeg klant toe aan database
                _context.Add(customer);
                await _context.SaveChangesAsync();
                
                // Redirect naar overzicht pagina na succesvolle creatie
                return RedirectToAction(nameof(Index));
            }
            
            // Bij validatie fouten, toon create form opnieuw met errors
            return View(customer);
        }

        /// <summary>
        /// Toont het formulier voor het bewerken van een bestaande klant.
        /// Laadt de huidige klantgegevens voor modificatie.
        /// </summary>
        /// <param name="id">Unieke identifier van de klant om te bewerken</param>
        /// <returns>Edit view met Customer model of NotFound</returns>
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Valideer of ID parameter is meegegeven
            if (id == null)
            {
                return NotFound();
            }

            // Zoek klant in database
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Active")] Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
