// Importeert interfaces, models, Entity Framework en standaard namespaces
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// Implementatie van IProductRepository voor product gerelateerde database operaties.
    /// Biedt volledige CRUD functionaliteit voor Product entiteiten.
    /// Gebruikt Entity Framework Core met eager loading voor gerelateerde Parts.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor ProductRepository. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public ProductRepository(MatrixIncDbContext context) 
        {
            _context = context;
        }

        /// <summary>
        /// Voegt een nieuw product toe aan de database en slaat wijzigingen direct op.
        /// </summary>
        /// <param name="product">Het Product object om toe te voegen</param>
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Verwijdert een product uit de database en slaat wijzigingen direct op.
        /// Let op: Dit kan foreign key constraints veroorzaken als product in orders staat.
        /// </summary>
        /// <param name="product">Het Product object om te verwijderen</param>
        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Haalt alle producten op inclusief hun gekoppelde onderdelen.
        /// Gebruikt eager loading om N+1 query problemen te voorkomen.
        /// </summary>
        /// <returns>Collectie van alle Product objecten met Parts</returns>
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products.Include(p => p.Parts);
        }

        /// <summary>
        /// Haalt een specifiek product op via ID inclusief gekoppelde onderdelen.
        /// </summary>
        /// <param name="id">Unieke identifier van het product</param>
        /// <returns>Product object met Parts of null als niet gevonden</returns>
        public Product? GetProductById(int id)
        {
            return _context.Products.Include(p => p.Parts).FirstOrDefault(p => p.Id == id);
        }

        /// <summary>
        /// Werkt een bestaand product bij in de database en slaat wijzigingen direct op.
        /// Entity Framework tracking zorgt voor automatische change detection.
        /// </summary>
        /// <param name="product">Het Product object met bijgewerkte gegevens</param>
        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges(); // Direct opslaan in database
        }
    }
}
