using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using DataAccessLayer.Models;
using System;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    /// <summary>
    /// Controller voor het genereren van test data tijdens ontwikkeling
    /// BELANGRIJK: Deze controller moet alleen gebruikt worden in development omgeving
    /// In productie moet deze controller uitgeschakeld of verwijderd worden
    /// </summary>
    public class TestDataController : Controller
    {
        // ==================== FIELDS & CONSTRUCTOR ====================
        
        /// <summary>
        /// Database context voor toegang tot de MatrixInc database
        /// </summary>
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor met dependency injection van database context
        /// </summary>
        /// <param name="context">Database context instance</param>
        public TestDataController(MatrixIncDbContext context)
        {
            _context = context;
        }

        // ==================== TEST DATA ACTIONS ====================

        /// <summary>
        /// Genereert willekeurige test data voor het testen van de applicatie
        /// Maakt een nieuwe bestelling aan met een random klant, product en hoeveelheid
        /// 
        /// BEVEILIGINGSRISICO: Deze methode heeft geen autorisatie controle
        /// In productie moet deze methode beveiligd of verwijderd worden
        /// </summary>
        /// <returns>Bevestiging dat test data is toegevoegd</returns>
        public IActionResult AddTestData()
        {
            // Initialiseer random generator voor willekeurige selecties
            var random = new Random();
            
            // Haal alle beschikbare klanten en producten op uit de database
            var customers = _context.Customers.ToList();
            var products = _context.Products.ToList();
            
            // Genereer willekeurige hoeveelheid tussen 1 en 5
            // Next(1, 6) genereert 1-5 omdat upper bound exclusief is
            var aantal = random.Next(1, 6);
            
            // Selecteer willekeurig een klant en product
            var customer = customers[random.Next(customers.Count)];
            var product = products[random.Next(products.Count)];
            
            // Maak nieuwe bestelling aan
            var order = new Order 
            { 
                Customer = customer,
                OrderDate = DateTime.Now  // Huidige datum/tijd als besteldatum
            };

            // Maak orderproduct relatie aan
            var orderProduct = new OrderProduct
            {
                Order = order,
                Product = product,
                Aantal = aantal
            };

            // Voeg orderproduct toe aan de bestelling
            order.OrderProducts.Add(orderProduct);
            
            // Sla wijzigingen op in database
            _context.Orders.Add(order);
            _context.SaveChanges();

            // Retourneer eenvoudige bevestiging
            return Content("Test data toegevoegd!");
        }
    }
}
