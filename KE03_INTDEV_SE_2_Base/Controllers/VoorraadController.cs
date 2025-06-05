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

        public IActionResult Create(int id, string type = "product") // Add type parameter
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems") 
                ?? new List<VoorraadItemViewModel>();
            
            // Voeg het geselecteerde product toe als het nog niet in de lijst staat
            if (!orderItems.Any(x => x.Id == id))
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
                            OrderAmount = 1
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
                            OrderAmount = 1
                        });
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
        public IActionResult AddToVoorraad(int id, string type)
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems") 
                ?? new List<VoorraadItemViewModel>();
            
            // Check of item al in lijst zit
            if (!orderItems.Any(x => x.Id == id && x.Type == type))
            {
                if (type == "product")
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
                            OrderAmount = 1
                        });
                    }
                }
                // Voeg hier later part logica toe als nodig
            }

            HttpContext.Session.SetObjectAsJson("VoorraadItems", orderItems);
            return RedirectToAction(nameof(Create));
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
            items?.RemoveAll(i => i.Id == model.Id);
            HttpContext.Session.SetObjectAsJson("VoorraadItems", items);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(Dictionary<int, int> quantities)
        {
            var orderItems = HttpContext.Session.GetObjectFromJson<List<VoorraadItemViewModel>>("VoorraadItems");
            if (orderItems != null)
            {
                foreach (var item in orderItems)
                {
                    if (item.Type.ToLower() == "product")
                    {
                        var product = await _context.Products.FindAsync(item.Id);
                        if (product != null && quantities.ContainsKey(item.Id))
                        {
                            product.Stock += quantities[item.Id];
                        }
                    }
                    else if (item.Type.ToLower() == "part")
                    {
                        var part = await _context.Parts.FindAsync(item.Id);
                        if (part != null && quantities.ContainsKey(item.Id))
                        {
                            part.Stock += quantities[item.Id];
                        }
                    }
                }
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("VoorraadItems");
            }
            return RedirectToAction("Index", "Home");
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
    }
}