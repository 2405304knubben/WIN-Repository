// Importeert Entity Framework Core en de data modellen
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    /// <summary>
    /// Database context klasse voor de MatrixInc applicatie.
    /// Definieert alle database tabellen (DbSets) en hun relaties.
    /// Fungert als de brug tussen C# objecten en de database.
    /// </summary>
    public class MatrixIncDbContext : DbContext
    {
        /// <summary>
        /// Constructor die de database configuratie ontvangt via dependency injection.
        /// </summary>
        /// <param name="options">Database configuratie opties (connection string, provider, etc.)</param>
        public MatrixIncDbContext(DbContextOptions<MatrixIncDbContext> options) : base(options)
        {
        }

        // *** DATABASE TABELLEN (DbSets) ***
        // Elke DbSet representeert een tabel in de database
        
        /// <summary>
        /// Klanten tabel - bevat alle klantgegevens
        /// </summary>
        public DbSet<Customer> Customers { get; set; }
        
        /// <summary>
        /// Bestellingen tabel - bevat alle order informatie
        /// </summary>
        public DbSet<Order> Orders { get; set; }
        
        /// <summary>
        /// Producten tabel - bevat alle product informatie
        /// </summary>
        public DbSet<Product> Products { get; set; }
        
        /// <summary>
        /// Onderdelen tabel - bevat alle part/component informatie
        /// </summary>
        public DbSet<Part> Parts { get; set; }
        
        /// <summary>
        /// Koppeltabel tussen Orders en Products - bevat welke producten in welke orders zitten
        /// </summary>
        public DbSet<OrderProduct> OrderProducts { get; set; }

        /// <summary>
        /// Configureert de database model relaties en constraints.
        /// Deze methode wordt automatisch aangeroepen door Entity Framework
        /// om de database schema te definiëren.
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder voor het configureren van entiteit relaties</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // *** KLANT - BESTELLING RELATIE ***
            // Een klant kan meerdere bestellingen hebben (One-to-Many)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)                    // Een Customer heeft vele Orders
                .WithOne(o => o.Customer)                  // Elke Order heeft één Customer
                .HasForeignKey(o => o.CustomerId)          // Foreign key is CustomerId in Order tabel
                .IsRequired();                             // CustomerId is verplicht (niet null)

            // *** ORDER - PRODUCT KOPPELTABEL CONFIGURATIE ***
            // Configureer de many-to-many relatie tussen Orders en Products via OrderProduct
            modelBuilder.Entity<OrderProduct>()
                .ToTable("orderProduct")                   // Expliciete tabel naam
                .HasKey(op => new { op.OrdersId, op.ProductsId }); // Samengestelde primary key

            // OrderProduct -> Order relatie (Many-to-One)
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)                    // OrderProduct heeft één Order
                .WithMany(o => o.OrderProducts)           // Order heeft vele OrderProducts
                .HasForeignKey(op => op.OrdersId)         // Foreign key naar Order
                .OnDelete(DeleteBehavior.Cascade);        // Verwijder OrderProducts als Order wordt verwijderd

            // OrderProduct -> Product relatie (Many-to-One)
            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)                 // OrderProduct heeft één Product
                .WithMany(p => p.OrderProducts)          // Product heeft vele OrderProducts
                .HasForeignKey(op => op.ProductsId)       // Foreign key naar Product
                .OnDelete(DeleteBehavior.Cascade);        // Verwijder OrderProducts als Product wordt verwijderd

            // *** PART - PRODUCT MANY-TO-MANY RELATIE ***
            // Een part kan in meerdere producten zitten, een product kan meerdere parts hebben
            modelBuilder.Entity<Part>()
                .HasMany(p => p.Products)                 // Part heeft vele Products
                .WithMany(p => p.Parts);                  // Product heeft vele Parts
                                                          // EF Core maakt automatisch een koppeltabel

            // Roep de base implementatie aan voor extra configuraties
            base.OnModelCreating(modelBuilder);
        }
    }
}
