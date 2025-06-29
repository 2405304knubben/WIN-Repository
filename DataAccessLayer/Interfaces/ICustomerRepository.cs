// Importeert data model en standaard namespaces
using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    /// <summary>
    /// Interface voor klant gerelateerde database operaties.
    /// Definieert de contract voor CRUD operaties op Customer entiteiten.
    /// Implementeert het Repository pattern voor data abstractie.
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Haalt alle klanten op uit de database.
        /// </summary>
        /// <returns>Collectie van alle Customer objecten</returns>
        public IEnumerable<Customer> GetAllCustomers();

        /// <summary>
        /// Haalt een specifieke klant op via het ID.
        /// </summary>
        /// <param name="id">Unieke identifier van de klant</param>
        /// <returns>Customer object of null als niet gevonden</returns>
        public Customer? GetCustomerById(int id);

        /// <summary>
        /// Voegt een nieuwe klant toe aan de database.
        /// </summary>
        /// <param name="customer">Het Customer object om toe te voegen</param>
        public void AddCustomer(Customer customer);

        /// <summary>
        /// Werkt een bestaande klant bij in de database.
        /// </summary>
        /// <param name="customer">Het Customer object met bijgewerkte gegevens</param>
        public void UpdateCustomer(Customer customer);

        /// <summary>
        /// Verwijdert een klant uit de database.
        /// </summary>
        /// <param name="customer">Het Customer object om te verwijderen</param>
        public void DeleteCustomer(Customer customer);
    }
}
