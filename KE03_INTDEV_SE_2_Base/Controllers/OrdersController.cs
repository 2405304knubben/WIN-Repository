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
    public class OrdersController : Controller
    {
        private readonly MatrixIncDbContext _context;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ILogger<OrdersController> logger, MatrixIncDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {            var allOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            var viewModel = new OrderViewModel
            {
                OrderDataForCustomer = allOrders.Select(order => new OrderDetailViewModel
                {
                    Order = order,
                    ProductDetails = order.OrderProducts.Select(op => new ProductDetailViewModel
                    {
                        ProductName = op.Product.Name,
                        Amount = op.Aantal,
                        Price = op.Product.Price
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new CreateOrderViewModel
            {
                Products = await _context.Products
                    .Where(p => p.Stock > 0)
                    .ToListAsync(),
                Customers = await _context.Customers
                    .Where(c => c.Active)
                    .ToListAsync()
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] int customerId, [FromForm] Dictionary<int, int> productQuantities)
        {
            // Validate customer and quantities
            if (customerId == 0 || !productQuantities.Any() || !ModelState.IsValid)
            {
                return RedirectToAction(nameof(Create));
            }

            // Get the customer
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                ModelState.AddModelError("", "Selecteer een geldige klant.");
                return RedirectToAction(nameof(Create));
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                CustomerId = customerId,
                Customer = customer
            };

            foreach (var (productId, quantity) in productQuantities.Where(pq => pq.Value > 0))
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null || product.Stock < quantity)
                {
                    ModelState.AddModelError("", $"Product {product?.Name ?? "Unknown"} is niet beschikbaar in de gevraagde hoeveelheid.");
                    return RedirectToAction(nameof(Create));
                }
                
                order.OrderProducts.Add(new OrderProduct
                {
                    OrdersId = order.Id,
                    ProductsId = product.Id,
                    Aantal = quantity,
                    Order = order,
                    Product = product
                });

                product.Stock -= quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = OrderEditViewModel.FromOrder(order);
            return View(viewModel);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Dictionary<int, int> productQuantities)
        {
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
                    // First, restore the original stock quantities
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        orderProduct.Product.Stock += orderProduct.Aantal;
                    }

                    // Clear existing order products
                    order.OrderProducts.Clear();

                    // Add new order products
                    foreach (var (productId, quantity) in productQuantities.Where(pq => pq.Value > 0))
                    {
                        var product = await _context.Products.FindAsync(productId);                        if (product == null || product.Stock < quantity)
                        {
                            ModelState.AddModelError("", $"Product {product?.Name ?? "Unknown"} is niet beschikbaar in de gevraagde hoeveelheid.");
                            return View(OrderEditViewModel.FromOrder(order));
                        }

                        order.OrderProducts.Add(new OrderProduct
                        {
                            OrdersId = order.Id,
                            ProductsId = product.Id,
                            Aantal = quantity,
                            Product = product
                        });

                        product.Stock -= quantity;
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
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

            var viewModel = OrderEditViewModel.FromOrder(order);
            return View(viewModel);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order != null)
            {
                // Restore stock quantities before deleting
                foreach (var orderProduct in order.OrderProducts)
                {
                    orderProduct.Product.Stock += orderProduct.Aantal;
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
