using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface ICustomerRepository
    {
        public IEnumerable<Customer> GetAllCustomers();

        public Customer? GetCustomerById(int id);

        public void AddCustomer(Customer customer);

        public void UpdateCustomer(Customer customer);

        public void DeleteCustomer(Customer customer);
    }
}
