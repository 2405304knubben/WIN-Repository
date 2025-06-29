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
    /// Interface voor product gerelateerde database operaties.
    /// Definieert de contract voor CRUD operaties op Product entiteiten.
    /// Implementeert het Repository pattern voor data abstractie.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Haalt alle producten op uit de database.
        /// </summary>
        /// <returns>Collectie van alle Product objecten</returns>
        public IEnumerable<Product> GetAllProducts();

        /// <summary>
        /// Haalt een specifiek product op via het ID.
        /// </summary>
        /// <param name="id">Unieke identifier van het product</param>
        /// <returns>Product object of null als niet gevonden</returns>
        public Product? GetProductById(int id);

        /// <summary>
        /// Voegt een nieuw product toe aan de database.
        /// </summary>
        /// <param name="product">Het Product object om toe te voegen</param>
        public void AddProduct(Product product);

        /// <summary>
        /// Werkt een bestaand product bij in de database.
        /// </summary>
        /// <param name="product">Het Product object met bijgewerkte gegevens</param>
        public void UpdateProduct(Product product);

        /// <summary>
        /// Verwijdert een product uit de database.
        /// </summary>
        /// <param name="product">Het Product object om te verwijderen</param>
        public void DeleteProduct(Product product);
    }
}
