using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class MatrixIncDbInitializer
    {
        public static void Initialize(MatrixIncDbContext context)
        {
            // Look for any customers.
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            // TODO: Hier moet ik nog wat namen verzinnen die betrekking hebben op de matrix.
            // - Denk aan de m3 boutjes, moertjes en ringetjes.
            // - Denk aan namen van schepen
            // - Denk aan namen van vliegtuigen            
            var customers = new Customer[]
            {
                new Customer { Name = "Neo", Address = "123 Elm St" , Active=true},
                new Customer { Name = "Morpheus", Address = "456 Oak St", Active = true },
                new Customer { Name = "Trinity", Address = "789 Pine St", Active = true },
                new Customer { Name = "Admin", Address = "Admin Street 1", Active = true }
            };
            context.Customers.AddRange(customers);            var products = new Product[]
            {
                new Product { Name = "Nebuchadnezzar", Description = "Het schip waarop Neo voor het eerst de echte wereld leert kennen", Price = 10000.00m },
                new Product { Name = "Jack-in Chair", Description = "Stoel met een rugsteun en metalen armen waarin mensen zitten om ingeplugd te worden in de Matrix via een kabel in de nekpoort", Price = 500.50m },
                new Product { Name = "EMP (Electro-Magnetic Pulse) Device", Description = "Wapentuig op de schepen van Zion", Price = 129.99m }
            };
            context.Products.AddRange(products);

            // Genereer wat orders voor de laatste 30 dagen
            var random = new Random();
            var orders = new List<Order>();
            var now = DateTime.Now;
            
            for (var i = 30; i >= 0; i--)
            {
                var orderDate = now.AddDays(-i);
                var numOrders = random.Next(0, 4); // 0-3 orders per dag
                
                for (var j = 0; j < numOrders; j++)
                {
                    var customer = customers[random.Next(customers.Length)];
                    var order = new Order { Customer = customer, OrderDate = orderDate };
                    
                    // Voeg 1-3 producten toe aan elke order
                    var numProducts = random.Next(1, 4);
                    for (var k = 0; k < numProducts; k++)
                    {
                        var product = products[random.Next(products.Length)];
                        var aantal = random.Next(1, 4);
                        order.OrderProducts.Add(new OrderProduct 
                        { 
                            Product = product,
                            Aantal = aantal
                        });
                    }
                    
                    orders.Add(order);
                }
            }
              context.Orders.AddRange(orders);

            var parts = new Part[]
            {
                new Part { Name = "Tandwiel", Description = "Overdracht van rotatie in bijvoorbeeld de motor of luikmechanismen"},
                new Part { Name = "M5 Boutje", Description = "Bevestiging van panelen, buizen of interne modules"},
                new Part { Name = "Hydraulische cilinder", Description = "Openen/sluiten van zware luchtsluizen of bewegende onderdelen"},
                new Part { Name = "Koelvloeistofpomp", Description = "Koeling van de motor of elektronische systemen."}
            };
            context.Parts.AddRange(parts);

            context.SaveChanges();

            context.Database.EnsureCreated();
        }
    }
}
