using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Repositories;
using DataAccessLayer;
using DataAccessLayer.Models;
using KE03_INTDEV_SE_2_Base.ViewModels;
using Microsoft.EntityFrameworkCore;
using KE03_INTDEV_SE_2_Base.Helpers; // Add this using directive

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class VoorraadController : Controller
    {
        private readonly ILogger<VoorraadController> _logger;
        private readonly MatrixIncDbContext _context;

        public VoorraadController(ILogger<VoorraadController> logger, MatrixIncDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ProductOverviewViewModel
            {
                Products = await _context.Products.Include(p => p.Parts).ToListAsync(),
                Parts = await _context.Parts.Include(p => p.Products).ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Order()
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<OrderItemViewModel>>("OrderItems") ?? new List<OrderItemViewModel>();
            return View(orderItems);
        }

        public IActionResult Create(int id, string type = "product")
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems")
                ?? new List<VoorraadItemViewModel>();

            // Controleer of het item (op basis van Id én Type) nog niet in de lijst staat
            // Alleen toevoegen als id niet 0 is (0 wordt gebruikt om de pagina te tonen zonder nieuw item toe te voegen)
            if (id != 0 && !orderItems.Any(x => x.Id == id && x.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.ToLower() == "product")
                {
                    var product = _context.Products.Find(id);
                    if (product != null)
                    {
                        orderItems.Add(new VoorraadItemViewModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Type = "Product",
                            CurrentStock = product.Stock,
                            OrderAmount = 1,
                            Price = product.Price // Prijs meegeven,
                        });
                    }
                }
                else if (type.ToLower() == "part")
                {
                    var part = _context.Parts.Find(id);
                    if (part != null)
                    {
                        orderItems.Add(new VoorraadItemViewModel
                        {
                            Id = part.Id,
                            Name = part.Name,
                            Type = "Part",
                            CurrentStock = part.Stock,
                            OrderAmount = 1,
                            Price = part.Price // Prijs meegeven,
                            Price = part.Price
                        });
                    }
                }
                HttpContext.Session.SetObjectAsJson("VoorraadItems", orderItems);
            }
            // De view 'Create.cshtml' wordt gebruikt om de lijst te tonen.
            return View(orderItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ProductId, int Amount, string Reason)
        {
            // This is for inventory adjustments
            var product = await _context.Products.FindAsync(ProductId);
            if (product != null)
            {
                product.Stock += Amount;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Bijbestellen()
        {
            // This is for ordering new stock
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

        public IActionResult AddToOrder(int id, string type)
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<OrderItemViewModel>>("OrderItems") ?? new List<OrderItemViewModel>();

            if (type == "product")
            {
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

            HttpContext.Session.SetObjectAsJson("OrderItems", orderItems);
            return RedirectToAction(nameof(Order));
        }

        [HttpPost]
        public IActionResult AddToVoorraad(int id, string type)
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems")
                ?? new List<VoorraadItemViewModel>();

            // Check of item al in lijst zit op basis van Id en Type
            if (!orderItems.Any(x => x.Id == id && x.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.ToLower() == "product")
                {
                    var product = _context.Products.Find(id);
                    if (product != null)
                    {
                        orderItems.Add(new VoorraadItemViewModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Type = "Product",
                            CurrentStock = product.Stock,
                            OrderAmount = 1,
                            Price = product.Price // Prijs meegeven
                        });
                    }
                }
                else if (type.ToLower() == "part")
                {
                    var part = _context.Parts.Find(id);
                    if (part != null)
                    {
                        orderItems.Add(new VoorraadItemViewModel
                        {
                            Id = part.Id,
                            Name = part.Name,
                            Type = "Part",
                            CurrentStock = part.Stock,
                            OrderAmount = 1,
                            Price = part.Price // Prijs meegeven
                        });
                    }
                }
            }

            HttpContext.Session.SetObjectAsJson("VoorraadItems", orderItems);
            // Redirect naar de Create view om de bijgewerkte lijst te tonen.
            // id=0 en een leeg type zorgen ervoor dat er niet opnieuw een item wordt toegevoegd door de Create action.
            return RedirectToAction(nameof(Create), new { id = 0, type = "" });
        }

        [HttpPost]
        public IActionResult UpdateAmount([FromBody] UpdateAmountModel model)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems");
            var item = items?.FirstOrDefault(i => i.Id == model.Id);
            if (item != null)
            {
                item.OrderAmount = model.Amount;
                HttpContext.Session.SetObjectAsJson("VoorraadItems", items);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult RemoveItem([FromBody] RemoveItemModel model)
        {
            var items = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems");
            if (items != null)
            {
                // Verwijder item op basis van Id. Als Id's niet uniek zijn over Producten/Parts,
                // zou je hier ook op Type moeten filteren (model.Type meegeven vanuit JS).
                items.RemoveAll(i => i.Id == model.Id); 
                if (items.Any())
                {
                    HttpContext.Session.SetObjectAsJson("VoorraadItems", items);
                }
                else
                {
                    HttpContext.Session.Remove("VoorraadItems"); // Verwijder de sessie key als de lijst leeg is.
                }
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(Dictionary<int, int> quantities)
        {
            // Haal de actuele lijst van items uit de sessie om te verwerken.
            var itemsToProcess = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems") 
                                      ?? new List<VoorraadItemViewModel>();
            try
            {
                if (itemsToProcess.Any())
                {
                    foreach (var item in itemsToProcess)
                    {
                        // Zoek de bestelde hoeveelheid voor dit item.
                        // De 'quantities' dictionary komt van de name="quantities[@item.Id]" in de view.
                        if (quantities.TryGetValue(item.Id, out int quantityToOrder) && quantityToOrder > 0)
                        {
                            if (item.Type.Equals("Product", StringComparison.OrdinalIgnoreCase))
                            {
                                var product = await _context.Products.FindAsync(item.Id);
                                if (product != null)
                                {
                                    product.Stock += quantityToOrder; // Voorraad verhogen
                                    _logger.LogInformation($"Product {product.Name} (ID: {product.Id}) voorraad bijgewerkt met {quantityToOrder}. Nieuwe voorraad: {product.Stock}");
                                }
                            }
                            else if (item.Type.Equals("Part", StringComparison.OrdinalIgnoreCase))
                            {
                                var part = await _context.Parts.FindAsync(item.Id);
                                if (part != null)
                                {
                                    part.Stock += quantityToOrder; // Voorraad verhogen
                                    _logger.LogInformation($"Onderdeel {part.Name} (ID: {part.Id}) voorraad bijgewerkt met {quantityToOrder}. Nieuwe voorraad: {part.Stock}");
                                }
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    HttpContext.Session.Remove("VoorraadItems"); // Leeg de "winkelwagen" na succesvolle bestelling.
                    TempData["SuccessMessage"] = "Voorraad succesvol bijgewerkt!";
                    return RedirectToAction(nameof(Index)); // Na succes, terug naar het overzicht van producten/onderdelen.
                }
                else
                {
                    TempData["ErrorMessage"] = "Geen items in de lijst om te bestellen.";
                    // Stuur terug naar de (lege) Create pagina. id=0 voorkomt dat de Create action een item probeert toe te voegen.
                    return RedirectToAction(nameof(Create), new { id = 0 }); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout tijdens het verwerken van de voorraadbestelling.");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwerken van de bestelling.";
                // Stuur de gebruiker terug naar de create pagina met de items die ze probeerden te bestellen, zodat ze het opnieuw kunnen proberen.
                return View("Create", itemsToProcess);
            }
        }

        public class UpdateAmountModel
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        public class RemoveItemModel
        {
            public int Id { get; set; }
        }        [HttpGet]
        public async Task<IActionResult> Ordercreate(string type = "normaal")
        {
            // Initialize or get the session variable
            var orderItems = HttpContext.Session.GetObjectFromJson<List<OrderItemViewModel>>("OrderItems") 
                ?? new List<OrderItemViewModel>();
            
            ViewBag.OrderType = type;
            var viewModel = new OrderEditViewModel
            {
                OrderDate = DateTime.Now,
                AvailableItems = await (type.ToLower() == "normaal" ?
                    _context.Products
                        .Select(p => new OrderItemViewModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Stock = p.Stock,
                            Price = p.Price
                        })
                        .ToListAsync() :
                    _context.Parts
                        .Select(p => new OrderItemViewModel
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Stock = p.Stock,
                            Price = p.Price
                        })
                        .ToListAsync())
            };

            // Store the viewModel in session
            HttpContext.Session.SetObjectAsJson("CurrentOrder", viewModel);

            return View(viewModel);
        }

        public IActionResult OrderConfirm()
        {
            var viewModel = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("CurrentOrder");
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Ordercreate));
            }
            return View(viewModel);
        }
    }
}