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
    public class CustomerRepository : ICustomerRepository
    {
        private readonly MatrixIncDbContext _context;

        public CustomerRepository(MatrixIncDbContext context)
        {
            _context = context;
        }

        public void AddCustomer(Customer customer)
        {
            _context.Customers.Add(customer);
            _context.SaveChanges();
        }

        public void DeleteCustomer(Customer customer)
        {
            _context.Customers.Remove(customer);
            _context.SaveChanges();
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            return _context.Customers.Include(c => c.Orders);
        }

        public Customer? GetCustomerById(int id)
        {
            return _context.Customers.Include(c => c.Orders).FirstOrDefault(c => c.Id == id);
        }

        public void UpdateCustomer(Customer customer)
        {
            _context.Customers.Update(customer);
            _context.SaveChanges();
        }
    }
}
