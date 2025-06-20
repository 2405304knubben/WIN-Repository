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

        public IActionResult Create(int id, string type = "product")
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems")
                ?? new List<VoorraadItemViewModel>();

            if (id != 0 && !orderItems.Any(x => x.Id == id && x.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
            {
                if (type.ToLower() == "product")
                {
                    var product = _context.Products.Find(id);
                    if (product != null)
                    {
                        var savedOrder = new VoorraadItemViewModel
                        {
                            Id = product.Id,
                            Name = product.Name,
                            Type = "Product",
                            CurrentStock = product.Stock,
                            OrderAmount = 1,
                            Price = product.Price
                        };
                        orderItems.Add(savedOrder);
                    }
                }
                else if (type.ToLower() == "part")
                {
                    var part = _context.Parts.Find(id);
                    if (part != null)
                    {
                        var savedOrder = new VoorraadItemViewModel
                        {
                            Id = part.Id,
                            Name = part.Name,
                            Type = "Part",
                            CurrentStock = part.Stock,
                            OrderAmount = 1,
                            Price = part.Price
                        };
                        orderItems.Add(savedOrder);
                    }
                }
                HttpContext.Session.SetObjectAsJson("VoorraadItems", orderItems);
            }

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
        public IActionResult UpdateAmount([FromBody] UpdateAmountModel model)
        {
            _logger.LogInformation($"Updating amount for item {model.Id} to {model.Amount}");
            var items = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems");

            if (items != null)
            {
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
                    // Clear all order-related session data
                    HttpContext.Session.Remove("VoorraadItems");
                    HttpContext.Session.Remove("OrderItems");
                    HttpContext.Session.Remove("CurrentOrder");
                    TempData["SuccessMessage"] = "Voorraad succesvol bijgewerkt!";
                    return RedirectToAction(nameof(Index));
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

        [HttpPost]
        public async Task<IActionResult> UploadImage(int id, string type, IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image was uploaded.");

            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                if (type.ToLower() == "product")
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product != null)
                    {
                        product.Image = imageData;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (type.ToLower() == "part")
                {
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

        public class UpdateAmountModel
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        public class RemoveItemModel
        {
            public int Id { get; set; }
        }
        [HttpGet]
        public async Task<IActionResult> Ordercreate(string type = "normaal")
        {
            // Get existing order from session if it exists
            var existingOrder = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            var existingQuantities = existingOrder?.AvailableItems
                .ToDictionary(i => i.Id, i => i.Quantity) ?? new Dictionary<int, int>();

            ViewBag.OrderType = type;

            // Create new view model with available items
            var items = type.ToLower() == "normaal" ?
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

            // Restore quantities from session
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

            // Store the viewModel in session
            HttpContext.Session.SetObjectAsJson("OrderItems", viewModel);

            return View(viewModel);
        }

        public IActionResult OrderConfirm()
        {
            var viewModel = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Ordercreate));
            }

            // Filter out items with quantity 0
            viewModel.AvailableItems = viewModel.AvailableItems.Where(i => i.Quantity > 0).ToList();

            // If no items with quantity > 0, redirect back
            if (!viewModel.AvailableItems.Any())
            {
                TempData["ErrorMessage"] = "Geen items geselecteerd voor bestelling.";
                return RedirectToAction(nameof(Ordercreate));
            }

            return View(viewModel);
        }

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
                // Update quantities from the form submission
                foreach (var item in Items.Where(i => i.Quantity > 0))
                {
                    var savedItem = order.AvailableItems.FirstOrDefault(i => i.Id == item.Id);
                    if (savedItem != null)
                    {
                        savedItem.Quantity = item.Quantity;
                    }
                }

                // Filter out items with quantity 0
                order.AvailableItems = order.AvailableItems.Where(i => i.Quantity > 0).ToList();
                HttpContext.Session.SetObjectAsJson("OrderItems", order);
            }
            else
            {
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

        public class UpdateQuantityModel
        {
            public int Id { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost]
        public IActionResult UpdateOrderQuantity([FromBody] UpdateQuantityModel model)
        {
            var order = HttpContext.Session.GetObjectFromJson<OrderEditViewModel>("OrderItems");
            if (order?.AvailableItems != null)
            {
                var existingItem = order.AvailableItems.FirstOrDefault(i => i.Id == model.Id);
                if (existingItem != null)
                {
                    // Only update the quantity
                    existingItem.Quantity = model.Quantity;

                    // Save back to session
                    HttpContext.Session.SetObjectAsJson("OrderItems", order);
                }
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(Dictionary<int, OrderItemViewModel> orderItems, string orderType, decimal totalAmount)
        {
            if (!orderItems.Any())
            {
                TempData["ErrorMessage"] = "Geen items geselecteerd voor bestelling.";
                return RedirectToAction(nameof(Ordercreate));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {                // Get the Admin customer or create one if it doesn't exist
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == "admin");

                if (customer == null)
                {
                    // Create Admin customer if it doesn't exist
                    customer = new Customer
                    {
                        Name = "Admin",
                        Address = "Matrix Inc. HQ",
                        Active = true
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Create new order
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerId = customer.Id,
                    Customer = customer
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Process each order item
                foreach (var kvp in orderItems)
                {
                    var item = kvp.Value;
                    if (orderType.ToLower() == "normaal")
                    {
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
                    else // Parts (bulk)
                    {
                        var part = await _context.Parts
                            .Include(p => p.Products)
                            .FirstOrDefaultAsync(p => p.Id == item.Id);

                        if (part != null)
                        {
                            // Verhoog de voorraad met de bestelde hoeveelheid
                            part.Stock += item.Quantity;
                            _logger.LogInformation($"Onderdeel {part.Name} voorraad verhoogd met {item.Quantity}. Nieuwe voorraad: {part.Stock}");

                            // Zoek een bestaand product voor dit onderdeel
                            var existingProduct = part.Products?.FirstOrDefault();

                            // Als er geen bestaand product is, maak een nieuwe aan
                            if (existingProduct == null)
                            {
                                var newProduct = new Product
                                {
                                    Name = $"{part.Name} (Onderdeel)",
                                    Price = part.Price,
                                    Stock = 0  // Voorraad wordt bijgehouden in het onderdeel
                                };

                                // Voeg het nieuwe product toe aan de database
                                _context.Products.Add(newProduct);
                                await _context.SaveChangesAsync();

                                // Voeg het nieuwe product toe aan de parts-products relatie
                                _context.Entry(part)
                                    .Collection(p => p.Products)
                                    .Load();
                                part.Products?.Add(newProduct);

                                existingProduct = newProduct;
                            }

                            var orderProduct = new OrderProduct
                            {
                                OrdersId = order.Id,
                                ProductsId = existingProduct.Id,
                                Aantal = item.Quantity
                            };
                            _context.OrderProducts.Add(orderProduct);
                        }
                    }
                }
                await _context.SaveChangesAsync(); await transaction.CommitAsync();

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