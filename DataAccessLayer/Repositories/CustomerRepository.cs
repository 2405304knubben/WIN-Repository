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
    /// Implementatie van ICustomerRepository voor klant gerelateerde database operaties.
    /// Gebruikt Entity Framework Core voor data toegang en implements het Repository pattern.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor CustomerRepository. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public CustomerRepository(MatrixIncDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Voegt een nieuwe klant toe aan de database en slaat wijzigingen direct op.
        /// </summary>
        /// <param name="customer">Het Customer object om toe te voegen</param>
        public void AddCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Verwijdert een klant uit de database en slaat wijzigingen direct op.
        /// Let op: Dit kan foreign key constraints veroorzaken als klant orders heeft.
        /// </summary>
        /// <param name="customer">Het Customer object om te verwijderen</param>
        public void DeleteCustomer(Customer customer)
        {
            _context.Customers.Remove(customer);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Haalt alle klanten op inclusief hun gekoppelde bestellingen.
        /// Gebruikt eager loading om N+1 query problemen te voorkomen.
        /// </summary>
        /// <returns>Collectie van alle Customer objecten met Orders</returns>
        public IEnumerable<Customer> GetAllCustomers()
        {
            return _context.Customers.Include(c => c.Orders);
        }

        /// <summary>
        /// Haalt een specifieke klant op via ID inclusief gekoppelde bestellingen.
        /// </summary>
        /// <param name="id">Unieke identifier van de klant</param>
        /// <returns>Customer object met Orders of null als niet gevonden</returns>
        public Customer? GetCustomerById(int id)
        {
            return _context.Customers.Include(c => c.Orders).FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Werkt een bestaande klant bij in de database en slaat wijzigingen direct op.
        /// Entity Framework tracking zorgt voor automatische change detection.
        /// </summary>
        /// <param name="customer">Het Customer object met bijgewerkte gegevens</param>
        public void UpdateCustomer(Customer customer)
        {
            _context.Customers.Update(customer);
            _context.SaveChanges(); // Direct opslaan in database
        }
    }
}
