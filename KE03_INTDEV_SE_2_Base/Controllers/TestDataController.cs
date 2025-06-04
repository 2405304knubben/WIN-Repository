using Microsoft.AspNetCore.Mvc;
using DataAccessLayer;
using DataAccessLayer.Models;
using System;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class TestDataController : Controller
    {
        private readonly MatrixIncDbContext _context;

        public TestDataController(MatrixIncDbContext context)
        {
            _context = context;
        }

        public IActionResult AddTestData()
        {            var random = new Random();
            
            // Haal alle customers en products op
            var customers = _context.Customers.ToList();
            var products = _context.Products.ToList();
            var aantal = random.Next(1, 6); // Random aantal tussen 1 en 5 (Next is exclusive op de upper bound)
            
            // Kies random een customer en product
            var customer = customers[random.Next(customers.Count)];
            var product = products[random.Next(products.Count)];
            
            var order = new Order 
            { 
                Customer = customer,
                OrderDate = DateTime.Now
            };            var orderProduct = new OrderProduct
            {
                Order = order,
                Product = product,
                Aantal = aantal
            };

            order.OrderProducts.Add(orderProduct);
            _context.Orders.Add(order);
            _context.SaveChanges();

            return Content("Test data toegevoegd!");
        }
    }
}
