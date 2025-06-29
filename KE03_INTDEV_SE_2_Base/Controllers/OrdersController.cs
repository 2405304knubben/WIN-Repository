// Importeert alle benodigde namespaces voor diagnostiek, LINQ, collections, MVC, data toegang en ViewModels
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using DataAccessLayer.Models;
using KE03_INTDEV_SE_2_Base.ViewModels;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.EntityFrameworkCore;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    /// <summary>
    /// Controller voor het beheren van bestellingen in het MatrixInc systeem.
    /// Handelt het aanmaken, bewerken, weergeven en verwijderen van orders af.
    /// </summary>
    public class OrdersController : Controller
    {
        // Dependency injection van database context en logger
        private readonly MatrixIncDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        /// <summary>
        /// Constructor voor OrdersController. Injecteert database context en logger.
        /// </summary>
        /// <param name="logger">Logger service voor event logging</param>
        /// <param name="context">Database context voor data toegang</param>
        public OrdersController(ILogger<OrdersController> logger, MatrixIncDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Toont een overzicht van alle bestellingen in het systeem.
        /// Inclusief klantgegevens, producten en bestellingsdetails.
        /// Orders worden gesorteerd op datum (nieuwste eerst).
        /// </summary>
        /// <returns>Index view met OrderViewModel</returns>
        public async Task<IActionResult> Index()
        {
            // Haal alle orders op inclusief gerelateerde data via eager loading
            var allOrders = await _context.Orders
                .Include(o => o.Customer)              // Laad klant informatie
                .Include(o => o.OrderProducts)         // Laad order-product koppelingen
                    .ThenInclude(op => op.Product)     // Laad product informatie
                        .ThenInclude(p => p.Parts)     // Laad onderdelen van producten
                .OrderByDescending(o => o.OrderDate)   // Sorteer op datum (nieuwste eerst)
                .ToListAsync();
            
            // Transformeer data naar view model voor presentatie
            var viewModel = new OrderViewModel
            {
                OrderDataForCustomer = allOrders.Select(order => new OrderDetailViewModel
                {
                    Order = order,
                    // Maak product details met onderdeel indicatie
                    ProductDetails = order.OrderProducts.Select(op =>
                    {
                        var isPart = op.Product.Parts != null && op.Product.Parts.Any();
                        return new ProductDetailViewModel
                        {
                            ProductName = op.Product.Name + (isPart ? " (Onderdeel)" : ""),
                            Amount = op.Aantal,
                            Price = op.Product.Price,
                            Type = isPart ? "Onderdeel" : "Product"
                        };
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Toont het formulier voor het aanmaken van een nieuwe bestelling.
        /// Laadt beschikbare producten (alleen met voorraad) en actieve klanten.
        /// </summary>
        /// <returns>Create view met CreateOrderViewModel</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Creëer view model met beschikbare data voor nieuwe bestelling
            var viewModel = new CreateOrderViewModel
            {
                // Alleen producten met voorraad > 0
                Products = await _context.Products
                    .Where(p => p.Stock > 0)
                    .ToListAsync(),
                // Alleen actieve klanten
                Customers = await _context.Customers
                    .Where(c => c.Active)
                    .ToListAsync()
            };
            
            return View(viewModel);
        }

        /// <summary>
        /// Verwerkt het aanmaken van een nieuwe bestelling via POST request.
        /// Valideert klant, product beschikbaarheid en werkt voorraad bij.
        /// Beveiligd tegen CSRF attacks door ValidateAntiForgeryToken.
        /// </summary>
        /// <param name="customerId">ID van de klant voor deze bestelling</param>
        /// <param name="productQuantities">Dictionary met productId als key en aantal als value</param>
        /// <returns>Redirect naar Index bij succes, anders terug naar Create form</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery
        public async Task<IActionResult> Create([FromForm] int customerId, [FromForm] Dictionary<int, int> productQuantities)
        {
            // Valideer basis vereisten voor de bestelling
            if (customerId == 0 || !productQuantities.Any() || !ModelState.IsValid)
            {
                return RedirectToAction(nameof(Create));
            }

            // Controleer of de klant bestaat
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                ModelState.AddModelError("", "Selecteer een geldige klant.");
                return RedirectToAction(nameof(Create));
            }

            // Creëer nieuwe bestelling
            var order = new Order
            {
                OrderDate = DateTime.Now,
                CustomerId = customerId,
                Customer = customer
            };

            // Verwerk elk product in de bestelling
            foreach (var (productId, quantity) in productQuantities.Where(pq => pq.Value > 0))
            {
                var product = await _context.Products.FindAsync(productId);
                
                // Controleer product beschikbaarheid en voorraad
                if (product == null || product.Stock < quantity)
                {
                    ModelState.AddModelError("", $"Product {product?.Name ?? "Unknown"} is niet beschikbaar in de gevraagde hoeveelheid.");
                    return RedirectToAction(nameof(Create));
                }
                
                // Voeg product toe aan bestelling
                order.OrderProducts.Add(new OrderProduct
                {
                    OrdersId = order.Id,
                    ProductsId = product.Id,
                    Aantal = quantity,
                    Order = order,
                    Product = product
                });

                // Update product voorraad
                product.Stock -= quantity;
            }

            // Sla bestelling op in database
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit/5
        /// <summary>
        /// Toont het formulier voor het bewerken van een bestaande bestelling
        /// Haalt de bestelling op inclusief alle gerelateerde producten
        /// </summary>
        /// <param name="id">ID van de te bewerken bestelling</param>
        /// <returns>Edit view met OrderEditViewModel of NotFound bij ongeldige ID</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Haal bestelling op met eager loading van OrderProducts en Product informatie
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Converteer naar ViewModel voor de view
            var viewModel = OrderEditViewModel.FromOrder(order);
            return View(viewModel);
        }

        // POST: Orders/Edit/5
        /// <summary>
        /// Verwerkt de bewerking van een bestelling
        /// Hanteert complexe logica voor voorraad management bij wijzigingen
        /// </summary>
        /// <param name="id">ID van de bestelling</param>
        /// <param name="productQuantities">Dictionary met ProductID -> Nieuwe Hoeveelheid mapping</param>
        /// <returns>Redirect naar Index bij succes, of Edit view bij validatie fouten</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Dictionary<int, int> productQuantities)
        {
            // Haal bestelling op met alle gerelateerde data
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // STAP 1: Herstel oorspronkelijke voorraad hoeveelheden
                    // Dit is nodig om correcte voorraad validatie te doen
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        orderProduct.Product.Stock += orderProduct.Aantal;
                    }

                    // STAP 2: Verwijder alle bestaande orderproducten
                    order.OrderProducts.Clear();

                    // STAP 3: Voeg nieuwe orderproducten toe met validatie
                    foreach (var (productId, quantity) in productQuantities.Where(pq => pq.Value > 0))
                    {
                        var product = await _context.Products.FindAsync(productId);
                        
                        // Valideer product beschikbaarheid en voorraad
                        if (product == null || product.Stock < quantity)
                        {
                            ModelState.AddModelError("", $"Product {product?.Name ?? "Unknown"} is niet beschikbaar in de gevraagde hoeveelheid.");
                            return View(OrderEditViewModel.FromOrder(order));
                        }

                        // Voeg orderproduct toe
                        order.OrderProducts.Add(new OrderProduct
                        {
                            OrdersId = order.Id,
                            ProductsId = product.Id,
                            Aantal = quantity,
                            Product = product
                        });

                        // Update voorraad
                        product.Stock -= quantity;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Concurrency conflict handling
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Bij model validatie fouten, toon form opnieuw
            var viewModel = OrderEditViewModel.FromOrder(order);
            return View(viewModel);
        }

        // GET: Orders/Delete/5
        /// <summary>
        /// Toont bevestigingspagina voor het verwijderen van een bestelling
        /// Geeft alle bestelling details weer voor final review
        /// </summary>
        /// <param name="id">ID van de te verwijderen bestelling</param>
        /// <returns>Delete confirmation view of NotFound bij ongeldige ID</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Haal bestelling op met alle gerelateerde data voor weergave
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = OrderEditViewModel.FromOrder(order);
            return View(viewModel);
        }

        // POST: Orders/Delete/5
        /// <summary>
        /// Verwerkt de definitieve verwijdering van een bestelling
        /// Belangrijk: Herstelt voorraad hoeveelheden voordat bestelling wordt verwijderd
        /// Dit voorkomt voorraad inconsistenties
        /// </summary>
        /// <param name="id">ID van de te verwijderen bestelling</param>
        /// <returns>Redirect naar Index</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Haal bestelling op met product informatie
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order != null)
            {
                // KRITIEK: Herstel voorraad hoeveelheden voor alle producten in de bestelling
                // Dit moet gebeuren voordat de bestelling wordt verwijderd
                foreach (var orderProduct in order.OrderProducts)
                {
                    orderProduct.Product.Stock += orderProduct.Aantal;
                }

                // Verwijder de bestelling (cascade delete zorgt voor OrderProducts)
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================== UTILITY METHODS ====================

        /// <summary>
        /// Error handling voor onverwachte fouten in de Orders controller
        /// Toont een generieke error pagina met tracking informatie
        /// </summary>
        /// <returns>Error view met ErrorViewModel</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Hulpmethode om te controleren of een bestelling bestaat
        /// Gebruikt voor concurrency controle in Edit/Delete operaties
        /// </summary>
        /// <param name="id">ID van de bestelling om te controleren</param>
        /// <returns>True als de bestelling bestaat, anders false</returns>
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
