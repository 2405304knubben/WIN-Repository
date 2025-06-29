// Importeert alle benodigde namespaces voor diagnostiek, MVC, data toegang, ViewModels en session helpers
using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Repositories;
using DataAccessLayer;
using DataAccessLayer.Models;
using KE03_INTDEV_SE_2_Base.ViewModels;
using Microsoft.EntityFrameworkCore;
using KE03_INTDEV_SE_2_Base.Helpers; // Voor session management helpers

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    /// <summary>
    /// Controller voor voorraadbeheer in het MatrixInc systeem.
    /// Handelt het bekijken, bijwerken en bestellen van voorraad af voor producten en onderdelen.
    /// Gebruikt sessie opslag voor tijdelijke voorraad orders.
    /// </summary>
    public class VoorraadController : Controller
    {
        // Dependency injection van logger en database context
        private readonly ILogger<VoorraadController> _logger;
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor VoorraadController. Injecteert logger en database context.
        /// </summary>
        /// <param name="logger">Logger service voor event logging</param>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public VoorraadController(ILogger<VoorraadController> logger, MatrixIncDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Toont het hoofdoverzicht van alle voorraad (producten en onderdelen).
        /// Laadt beide entiteit types inclusief hun onderlinge relaties voor complete weergave.
        /// </summary>
        /// <returns>Index view met ProductOverviewViewModel</returns>
        public async Task<IActionResult> Index()
        {
            // Creëer view model met alle voorraad data
            var viewModel = new ProductOverviewViewModel
            {
                // Producten inclusief gerelateerde onderdelen
                Products = await _context.Products.Include(p => p.Parts).ToListAsync(),
                // Onderdelen inclusief gerelateerde producten
                Parts = await _context.Parts.Include(p => p.Products).ToListAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Beheert het aanmaken van voorraad bestellingen via sessie opslag.
        /// Voegt items toe aan een tijdelijke bestelling die in de sessie wordt opgeslagen.
        /// Ondersteunt zowel producten als onderdelen.
        /// </summary>
        /// <param name="id">ID van het product of onderdeel om toe te voegen</param>
        /// <param name="type">Type van het item: "product" of "part"</param>
        /// <returns>Create view met huidige sessie items</returns>
        public IActionResult Create(int id, string type = "product")
        {
            // Haal huidige voorraad items op uit sessie (of maak nieuwe lijst)
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems")
                ?? new List<VoorraadItemViewModel>();

            // Voeg nieuw item toe als het nog niet bestaat in de huidige bestelling
            if (id != 0 && !orderItems.Any(x => x.Id == id && x.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.ToLower() == "product")
                {
                    // Verwerk product toevoeging
                    var product = _context.Products.Find(id);
                    if (product != null)
                    {
                        var savedOrder = new VoorraadItemViewModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Type = "Product",
                            CurrentStock = product.Stock,
                            OrderAmount = 1, // Standaard bestel hoeveelheid
                            Price = product.Price
                        };
                        orderItems.Add(savedOrder);
                    }
                }
                else if (type.ToLower() == "part")
                {
                    // Verwerk onderdeel toevoeging
                    var part = _context.Parts.Find(id);
                    if (part != null)
                    {
                        var savedOrder = new VoorraadItemViewModel
                        {
                            Id = part.Id,
                            Name = part.Name,
                            Type = "Part",
                            CurrentStock = part.Stock,
                            OrderAmount = 1, // Standaard bestel hoeveelheid
                            Price = part.Price
                        };
                        orderItems.Add(savedOrder);
                    }
                }
                
                // Sla bijgewerkte lijst op in sessie
                HttpContext.Session.SetObjectAsJson("VoorraadItems", orderItems);
            }

            return View(orderItems);
        }

        /// <summary>
        /// Verwerkt handmatige voorraad aanpassingen via POST request.
        /// Gebruikt voor correcties, tellingen of andere voorraad wijzigingen.
        /// Beveiligd tegen CSRF attacks door ValidateAntiForgeryToken.
        /// </summary>
        /// <param name="ProductId">ID van het product waarvan de voorraad wordt aangepast</param>
        /// <param name="Amount">Aantal om toe te voegen (positief) of af te trekken (negatief)</param>
        /// <param name="Reason">Reden voor de voorraad aanpassing (audit trail)</param>
        /// <returns>Redirect naar Index pagina na succesvolle verwerking</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery
        public async Task<IActionResult> Create(int ProductId, int Amount, string Reason)
        {
            // Zoek het product in de database
            var product = await _context.Products.FindAsync(ProductId);
            if (product != null)
            {
                // Pas voorraad aan met het opgegeven aantal
                product.Stock += Amount;
                
                // Sla wijziging op in database
                await _context.SaveChangesAsync();
            }
            
            // Redirect naar voorraad overzicht
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Toont de bijbestellen pagina met producten die mogelijk bijbesteld moeten worden.
        /// Toont alleen producten met lage voorraad of andere bestel criteria.
        /// </summary>
        /// <returns>Bijbestellen view met BijbestellenViewModel lijst</returns>
        public IActionResult Bijbestellen()
        {
            // Haal producten op die mogelijk bijbesteld moeten worden
            ViewBag.Products = _context.Products
                .Select(p => new BijbestellenViewModel
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.Stock,
                    OrderAmount = 0
                })
                .ToList();
            return View("Order");
        }

        /// <summary>
        /// Voegt items toe aan de actieve bestelling sessie voor latere verwerking.
        /// Ondersteunt zowel producten als onderdelen via type parameter.
        /// Items worden tijdelijk opgeslagen in de sessie tot definitieve verwerking.
        /// </summary>
        /// <param name="id">ID van het product of onderdeel om toe te voegen</param>
        /// <param name="type">Type van het item: "product" of "part"</param>
        /// <returns>Redirect naar Order action</returns>
        public IActionResult AddToOrder(int id, string type)
        {
            // Haal huidige bestelling items op uit sessie
            var orderItems = HttpContext.Session.GetObjectFromJson<List<OrderItemViewModel>>("OrderItems") 
                ?? new List<OrderItemViewModel>();

            if (type == "product")
            {
                // Verwerk product toevoeging
                var product = _context.Products.Find(id);
                if (product != null)
                {
                    orderItems.Add(new OrderItemViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Type = "Product",
                        Price = product.Price,
                        Stock = product.Stock
                    });
                }
            }
            else if (type == "part")
            {
                // Verwerk onderdeel toevoeging
                var part = _context.Parts.Find(id);
                if (part != null)
                {
                    orderItems.Add(new OrderItemViewModel
                    {
                        Id = part.Id,
                        Name = part.Name,
                        Type = "Part",
                        Price = part.Price,
                        Stock = part.Stock
                    });
                }
            }

            // Sla bijgewerkte bestelling op in sessie
            HttpContext.Session.SetObjectAsJson("OrderItems", orderItems);
            return RedirectToAction(nameof(Order));
        }

        /// <summary>
        /// API endpoint voor het bijwerken van bestel hoeveelheden via AJAX.
        /// Gebruikt voor real-time updates in de gebruikersinterface zonder page refresh.
        /// </summary>
        /// <param name="model">Model met Item ID en nieuwe hoeveelheid</param>
        /// <returns>JSON response met success status</returns>
        [HttpPost]
        public IActionResult UpdateAmount([FromBody] UpdateAmountModel model)
        {
            _logger.LogInformation($"Updating amount for item {model.Id} to {model.Amount}");
            
            // Haal huidige voorraad items op uit sessie
            var items = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems");

            if (items != null)
            {
                // Zoek het specifieke item en update de hoeveelheid
                var item = items.FirstOrDefault(i => i.Id == model.Id);
                if (item != null)
                {
                    item.OrderAmount = model.Amount;
                    HttpContext.Session.SetObjectAsJson("VoorraadItems", items);
                    _logger.LogInformation($"Successfully updated amount for {item.Name} to {model.Amount}");
                    return Ok(new { success = true });
                }
            }

            return BadRequest(new { success = false, message = "Item not found in session" });
        }

        /// <summary>
        /// API endpoint voor het verwijderen van items uit de voorraad bestelling.
        /// Gebruikt AJAX voor seamless user experience.
        /// </summary>
        /// <param name="model">Model met Item ID om te verwijderen</param>
        /// <returns>OK status bij success</returns>
        [HttpPost]
        public IActionResult RemoveItem([FromBody] RemoveItemModel model)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems");
            if (items != null)
            {
                // Verwijder item op basis van ID
                // BELANGRIJK: Als IDs niet uniek zijn tussen producten/parts,
                // zou hier ook op Type gefilterd moeten worden
                items.RemoveAll(i => i.Id == model.Id);
                
                if (items.Any())
                {
                    // Update sessie met resterende items
                    HttpContext.Session.SetObjectAsJson("VoorraadItems", items);
                }
                else
                {
                    // Verwijder sessie key als lijst leeg is
                    HttpContext.Session.Remove("VoorraadItems");
                }
            }
            return Ok();
        }

        /// <summary>
        /// Verwerkt de definitieve voorraad bestelling en werkt voorraad bij.
        /// Dit is het eindpunt waar alle sessie items worden verwerkt en voorraad wordt aangepast.
        /// Ondersteunt zowel producten als onderdelen.
        /// </summary>
        /// <param name="quantities">Dictionary met Item ID -> Bestel hoeveelheid mapping</param>
        /// <returns>Redirect naar Index bij succes, of terug naar Create bij fouten</returns>
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(Dictionary<int, int> quantities)
        {
            // Haal de actuele lijst van items uit de sessie om te verwerken
            var itemsToProcess = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems")
                                      ?? new List<VoorraadItemViewModel>();
            try
            {
                if (itemsToProcess.Any())
                {
                    // Verwerk elk item in de bestelling
                    foreach (var item in itemsToProcess)
                    {
                        // Zoek de bestelde hoeveelheid voor dit item
                        // De 'quantities' dictionary komt van name="quantities[@item.Id]" in de view
                        if (quantities.TryGetValue(item.Id, out int quantityToOrder) && quantityToOrder > 0)
                        {
                            if (item.Type.Equals("Product", StringComparison.OrdinalIgnoreCase))
                            {
                                // Update product voorraad
                                var product = await _context.Products.FindAsync(item.Id);
                                if (product != null)
                                {
                                    product.Stock += quantityToOrder; // Voorraad verhogen
                                    _logger.LogInformation($"Product {product.Name} (ID: {product.Id}) voorraad bijgewerkt met {quantityToOrder}. Nieuwe voorraad: {product.Stock}");
                                }
                            }
                            else if (item.Type.Equals("Part", StringComparison.OrdinalIgnoreCase))
                            {
                                // Update onderdeel voorraad
                                var part = await _context.Parts.FindAsync(item.Id);
                                if (part != null)
                                {
                                    part.Stock += quantityToOrder; // Voorraad verhogen
                                    _logger.LogInformation($"Onderdeel {part.Name} (ID: {part.Id}) voorraad bijgewerkt met {quantityToOrder}. Nieuwe voorraad: {part.Stock}");
                                }
                            }
                        }
                    }
                    
                    // Sla alle wijzigingen op in database
                    await _context.SaveChangesAsync();
                    
                    // Wis alle bestelling-gerelateerde sessie data
                    HttpContext.Session.Remove("VoorraadItems");
                    HttpContext.Session.Remove("OrderItems");
                    HttpContext.Session.Remove("CurrentOrder");
                    
                    TempData["SuccessMessage"] = "Voorraad succesvol bijgewerkt!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Geen items in de lijst om te bestellen.";
                    // Stuur terug naar de (lege) Create pagina
                    // id=0 voorkomt dat de Create action een item probeert toe te voegen
                    return RedirectToAction(nameof(Create), new { id = 0 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout tijdens het verwerken van de voorraadbestelling.");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwerken van de bestelling.";
                // Stuur de gebruiker terug naar de create pagina met de items
                // Stuur de gebruiker terug naar de create pagina met de items
                return View("Create", itemsToProcess);
            }
        }

        /// <summary>
        /// Upload afbeelding voor product of onderdeel via file upload.
        /// Ondersteunt zowel producten als onderdelen door type parameter.
        /// Afbeeldingen worden als binary data opgeslagen in de database.
        /// </summary>
        /// <param name="id">ID van het product/onderdeel</param>
        /// <param name="type">Type: "product" of "part"</param>
        /// <param name="image">Upload bestand</param>
        /// <returns>Redirect naar Index of BadRequest bij fouten</returns>
        [HttpPost]
        public async Task<IActionResult> UploadImage(int id, string type, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image was uploaded.");

            // Converteer upload naar binary data
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                if (type.ToLower() == "product")
                {
                    // Update product afbeelding
                    var product = await _context.Products.FindAsync(id);
                    if (product != null)
                    {
                        product.Image = imageData;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type.ToLower() == "part")
                {
                    // Update onderdeel afbeelding
                    var part = await _context.Parts.FindAsync(id);
                    if (part != null)
                    {
                        part.Image = imageData;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ==================== MODEL CLASSES ====================

        /// <summary>
        /// Model voor AJAX requests om bestel hoeveelheden bij te werken
        /// </summary>
        public class UpdateAmountModel
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        /// <summary>
        /// Model voor AJAX requests om items te verwijderen uit bestelling
        /// </summary>
        public class RemoveItemModel
        {
            public int Id { get; set; }
        }

        /// <summary>
        /// Toont het formulier voor het aanmaken van nieuwe bestellingen.
        /// Ondersteunt verschillende bestelling types: normaal (producten) of bulk (onderdelen).
        /// Gebruikt sessie om bestelling state te behouden tussen requests.
        /// </summary>
        /// <param name="type">Type bestelling: "normaal" voor producten, andere waarde voor onderdelen</param>
        /// <returns>Ordercreate view met OrderEditViewModel</returns>
        [HttpGet]
        public async Task<IActionResult> Ordercreate(string type = "normaal")
        {
            // Haal bestaande bestelling op uit sessie indien aanwezig
            var existingOrder = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            var existingQuantities = existingOrder?.AvailableItems
                .ToDictionary(i => i.Id, i => i.Quantity) ?? new Dictionary<int, int>();

            ViewBag.OrderType = type;

            // Maak nieuwe view model met beschikbare items
            var items = type.ToLower() == "normaal" ?
                // Normaal: haal producten op
                await _context.Products
                    .Select(p => new OrderItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Stock = p.Stock,
                        Price = p.Price,
                        Quantity = 0
                    })
                    .ToListAsync() :
                // Bulk: haal onderdelen op
                await _context.Parts
                    .Select(p => new OrderItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Stock = p.Stock,
                        Price = p.Price,
                        Quantity = 0
                    })
                    .ToListAsync();

            // Herstel hoeveelheden uit sessie voor continuïteit
            foreach (var item in items)
            {
                if (existingQuantities.TryGetValue(item.Id, out int quantity))
                {
                    item.Quantity = quantity;
                }
            }

            var viewModel = new OrderEditViewModel
            {
                OrderDate = DateTime.Now,
                AvailableItems = items
            };

            // Sla viewModel op in sessie voor latere gebruik
            HttpContext.Session.SetObjectAsJson("OrderItems", viewModel);

            return View(viewModel);
        }

        /// <summary>
        /// Toont bevestigingspagina voor bestelling voordat deze wordt verwerkt.
        /// Filtert items met hoeveelheid 0 uit voor cleaner weergave.
        /// </summary>
        /// <returns>OrderConfirm view of redirect naar Ordercreate bij lege bestelling</returns>
        public IActionResult OrderConfirm()
        {
            var viewModel = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Ordercreate));
            }

            // Filter items met hoeveelheid 0 uit
            viewModel.AvailableItems = viewModel.AvailableItems.Where(i => i.Quantity > 0).ToList();

            // Als geen items met hoeveelheid > 0, ga terug naar create
            if (!viewModel.AvailableItems.Any())
            {
                TempData["ErrorMessage"] = "Geen items geselecteerd voor bestelling.";
                return RedirectToAction(nameof(Ordercreate));
            }

            return View(viewModel);
        }

        /// <summary>
        /// Verwerkt bestelling bevestiging met bijgewerkte hoeveelheden.
        /// Update sessie data met finale bestelling informatie.
        /// </summary>
        /// <param name="orderType">Type bestelling voor processing context</param>
        /// <param name="Items">Lijst van items met finale hoeveelheden</param>
        /// <returns>OrderConfirm view met bijgewerkte bestelling</returns>
        [HttpPost]
        public IActionResult OrderConfirm(string orderType, List<OrderItemViewModel> Items)
        {
            if (Items == null || !Items.Any(i => i.Quantity > 0))
            {
                TempData["ErrorMessage"] = "Geen items geselecteerd voor bestelling.";
                return RedirectToAction(nameof(Ordercreate));
            }

            var order = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            if (order != null)
            {
                // Update hoeveelheden van form submission
                foreach (var item in Items.Where(i => i.Quantity > 0))
                {
                    var savedItem = order.AvailableItems.FirstOrDefault(i => i.Id == item.Id);
                    if (savedItem != null)
                    {
                        savedItem.Quantity = item.Quantity;
                    }
                }

                // Filter items met hoeveelheid 0 uit
                order.AvailableItems = order.AvailableItems.Where(i => i.Quantity > 0).ToList();
                HttpContext.Session.SetObjectAsJson("OrderItems", order);
            }
            else
            {
                // Maak nieuwe bestelling als sessie data verloren is
                order = new OrderEditViewModel
                {
                    OrderDate = DateTime.Now,
                    AvailableItems = Items.Where(i => i.Quantity > 0).ToList()
                };
                HttpContext.Session.SetObjectAsJson("OrderItems", order);
            }

            TempData["OrderType"] = orderType;
            return View(order);
        }

        /// <summary>
        /// Model voor AJAX requests om order hoeveelheden bij te werken
        /// </summary>
        public class UpdateQuantityModel
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
        }

        /// <summary>
        /// API endpoint voor het bijwerken van bestelling hoeveelheden via AJAX.
        /// Gebruikt voor real-time updates zonder page refresh.
        /// </summary>
        /// <param name="model">Model met Item ID en nieuwe hoeveelheid</param>
        /// <returns>OK status</returns>
        [HttpPost]
        public IActionResult UpdateOrderQuantity([FromBody] UpdateQuantityModel model)
        {
            var order = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            if (order?.AvailableItems != null)
            {
                var existingItem = order.AvailableItems.FirstOrDefault(i => i.Id == model.Id);
                if (existingItem != null)
                {
                    // Update alleen de hoeveelheid
                    existingItem.Quantity = model.Quantity;

                    // Sla terug op in sessie
                    HttpContext.Session.SetObjectAsJson("OrderItems", order);
                }
            }
            return Ok();
        }

        /// <summary>
        /// Verwerkt de finale bestelling en werkt voorraad bij.
        /// Maakt Admin klant aan indien nodig voor interne bestellingen.
        /// Gebruikt database transacties voor data consistentie.
        /// </summary>
        /// <param name="orderItems">Dictionary van bestelde items</param>
        /// <param name="orderType">Type bestelling voor correct processing</param>
        /// <param name="totalAmount">Totaal bedrag van bestelling</param>
        /// <returns>Redirect naar geschikte pagina na verwerking</returns>
        [HttpPost]
        public async Task<IActionResult> ProcessOrder(Dictionary<int, OrderItemViewModel> orderItems, string orderType, decimal totalAmount)
        {
            if (!orderItems.Any())
            {
                TempData["ErrorMessage"] = "Geen items geselecteerd voor bestelling.";
                return RedirectToAction(nameof(Ordercreate));
            }

            // Begin database transactie voor atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Zoek of maak Admin klant voor interne bestellingen
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == "admin");

                if (customer == null)
                {
                    // Maak Admin klant aan als deze niet bestaat
                    customer = new Customer
                    {
                        Name = "Admin",
                        Address = "Matrix Inc. HQ",
                        Active = true
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Maak nieuwe bestelling aan
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerId = customer.Id,
                    Customer = customer
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Verwerk elk bestelling item
                foreach (var kvp in orderItems)
                {
                    var item = kvp.Value;
                    if (orderType.ToLower() == "normaal")
                    {
                        // Normale bestelling: update product voorraad
                        var product = await _context.Products.FindAsync(item.Id);
                        if (product != null)
                        {
                            // Verhoog de voorraad met de bestelde hoeveelheid
                            product.Stock += item.Quantity;
                            _logger.LogInformation($"Product {product.Name} voorraad verhoogd met {item.Quantity}. Nieuwe voorraad: {product.Stock}");

                            var orderProduct = new OrderProduct
                            {
                                OrdersId = order.Id,
                                ProductsId = product.Id,
                                Aantal = item.Quantity
                            };
                            _context.OrderProducts.Add(orderProduct);
                        }
                    }
                    else // Bulk bestelling voor onderdelen
                    {
                        // Vind het onderdeel en update alleen de voorraad
                        var part = await _context.Parts.FindAsync(item.Id);
                        if (part != null)
                        {
                            // Verhoog de voorraad met de bestelde hoeveelheid
                            part.Stock += item.Quantity;
                            _logger.LogInformation($"Onderdeel {part.Name} voorraad verhoogd met {item.Quantity}. Nieuwe voorraad: {part.Stock}");

                            // Registreer de bestelling met een verwijzing naar het onderdeel
                            var orderProduct = new OrderProduct
                            {
                                OrdersId = order.Id,
                                ProductsId = item.Id,  // Gebruik part.Id als ProductsId
                                Aantal = item.Quantity
                            };
                            _context.OrderProducts.Add(orderProduct);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear all order-related session data
                HttpContext.Session.Remove("CurrentOrder");
                HttpContext.Session.Remove("OrderItems");
                HttpContext.Session.Remove("VoorraadItems");

                // Check for return URL
                var returnUrl = Request.Query["returnUrl"].ToString();
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Voorraad");
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(OrderConfirm));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing order");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwerken van de bestelling.";

                // In case of error, keep the session data for retry
                var viewModel = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("CurrentOrder");
                return View(nameof(OrderConfirm), viewModel);
            }
        }
        public IActionResult Order()
        {
            return View();
        }
    }
}