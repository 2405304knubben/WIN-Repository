// Importeert data models en standaard namespaces
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    /// <summary>
    /// Statische klasse voor het initialiseren van de database met test data.
    /// Voegt automatisch klanten, producten, onderdelen en bestellingen toe bij eerste start.
    /// Gebaseerd op The Matrix thema voor consistent test data.
    /// </summary>
    public static class MatrixIncDbInitializer
    {
        /// <summary>
        /// Initialiseert de database met test data als deze nog leeg is.
        /// Controleert of er al klanten bestaan om dubbele initialisatie te voorkomen.
        /// </summary>
        /// <param name="context">Database context voor data toegang</param>
        public static void Initialize(MatrixIncDbContext context)
        {
            // Controleer of database al geïnitialiseerd is door te kijken naar klanten
            if (context.Customers.Any())
            {
                return;   // Database is al gevuld, stop hier
            }

            // *** KLANTEN AANMAKEN ***
            // Matrix geïnspireerde klanten voor test doeleinden
            var customers = new Customer[]
            {
                new Customer { Name = "Neo", Address = "123 Elm St", Active = true },
                new Customer { Name = "Morpheus", Address = "456 Oak St", Active = true },
                new Customer { Name = "Trinity", Address = "789 Pine St", Active = true },
                new Customer { Name = "Admin", Address = "Admin Street 1", Active = true }
            };
            context.Customers.AddRange(customers);

            // *** PRODUCTEN AANMAKEN ***
            // Matrix geïnspireerde producten met realistische prijzen
            var products = new Product[]
            {
                new Product 
                { 
                    Name = "Nebuchadnezzar", 
                    Description = "Het schip waarop Neo voor het eerst de echte wereld leert kennen", 
                    Price = 10000.00m,
                    Stock = 2  // Voeg voorraad toe
                },
                new Product 
                { 
                    Name = "Jack-in Chair", 
                    Description = "Stoel met een rugsteun en metalen armen waarin mensen zitten om ingeplugd te worden in de Matrix via een kabel in de nekpoort", 
                    Price = 500.50m,
                    Stock = 15
                },
                new Product 
                { 
                    Name = "EMP (Electro-Magnetic Pulse) Device", 
                    Description = "Wapentuig op de schepen van Zion", 
                    Price = 129.99m,
                    Stock = 8
                }
            };
            context.Products.AddRange(products);

            // *** WILLEKEURIGE BESTELLINGEN GENEREREN ***
            // Creëer realistische order history voor de afgelopen 30 dagen
            var random = new Random();
            var orders = new List<Order>();
            var now = DateTime.Now;
            
            // Loop door elke dag van de afgelopen 30 dagen
            for (var i = 30; i >= 0; i--)
            {
                var orderDate = now.AddDays(-i);
                var numOrders = random.Next(0, 4); // 0-3 orders per dag voor realistische spreiding
                
                // Genereer orders voor deze dag
                for (var j = 0; j < numOrders; j++)
                {
                    // Selecteer willekeurige klant
                    var customer = customers[random.Next(customers.Length)];
                    var order = new Order { Customer = customer, OrderDate = orderDate };
                    
                    // Voeg 1-3 producten toe aan elke order voor variatie
                    var numProducts = random.Next(1, 4);
                    for (var k = 0; k < numProducts; k++)
                    {
                        var product = products[random.Next(products.Length)];
                        var aantal = random.Next(1, 4); // 1-3 stuks per product
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

            // *** ONDERDELEN AANMAKEN ***
            // Technische componenten die gebruikt kunnen worden in producten
            var parts = new Part[]
            {
                new Part 
                { 
                    Name = "Tandwiel", 
                    Description = "Overdracht van rotatie in bijvoorbeeld de motor of luikmechanismen",
                    Stock = 100,
                    Price = 15.50m
                },
                new Part 
                { 
                    Name = "M5 Boutje", 
                    Description = "Bevestiging van panelen, buizen of interne modules",
                    Stock = 500,
                    Price = 0.25m
                },
                new Part 
                { 
                    Name = "Hydraulische cilinder", 
                    Description = "Openen/sluiten van zware luchtsluizen of bewegende onderdelen",
                    Stock = 25,
                    Price = 89.99m
                },
                new Part 
                { 
                    Name = "Koelvloeistofpomp", 
                    Description = "Koeling van de motor of elektronische systemen",
                    Stock = 12,
                    Price = 156.75m
                }
            };
            context.Parts.AddRange(parts);

            // Sla alle wijzigingen op in de database
            context.SaveChanges();

            // Zorg ervoor dat de database schema up-to-date is
            context.Database.EnsureCreated();
        }
    }
}
