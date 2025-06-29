// Importeert data access layer models
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het aanmaken van nieuwe bestellingen.
    /// Bevat alle benodigde data voor het order creation formulier.
    /// </summary>
    public class CreateOrderViewModel
    {
        /// <summary>
        /// Collectie van beschikbare producten om aan de bestelling toe te voegen.
        /// Alleen producten met voorraad > 0 worden getoond.
        /// </summary>
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        
        /// <summary>
        /// Collectie van actieve klanten die een bestelling kunnen plaatsen.
        /// Alleen actieve klanten worden getoond in de dropdown.
        /// </summary>
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
    }
}
