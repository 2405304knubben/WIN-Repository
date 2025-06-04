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
        {
            // Voeg eerst een order toe voor vandaag
            var customer = _context.Customers.First();
            var product = _context.Products.First();
            
            var order = new Order 
            { 
                Customer = customer,
                OrderDate = DateTime.Now
            };

            var orderProduct = new OrderProduct
            {
                Order = order,
                Product = product,
                Aantal = 2
            };

            order.OrderProducts.Add(orderProduct);
            _context.Orders.Add(order);
            _context.SaveChanges();

            return Content("Test data toegevoegd!");
        }
    }
}
