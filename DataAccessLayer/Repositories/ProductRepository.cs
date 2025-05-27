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
    public class ProductRepository : IProductRepository
    {
        private readonly MatrixIncDbContext _context;

        public ProductRepository(MatrixIncDbContext context) 
        {
            _context = context;
        }
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products.Include(p => p.Parts);
        }

        public Product? GetProductById(int id)
        {
            return _context.Products.Include(p => p.Parts).FirstOrDefault(p => p.Id == id);
        }

        public void UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }
    }
}
