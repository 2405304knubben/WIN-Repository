// Importeert data access layer models
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het weergeven van product en onderdeel overzichten.
    /// Combineert beide entiteit types in een uniforme interface voor voorraad en inventory management.
    /// Wordt gebruikt door zowel Product als Voorraad controllers.
    /// </summary>
    public class ProductOverviewViewModel
    {
        /// <summary>
        /// Collectie van alle producten in het systeem.
        /// Inclusief voorraad informatie en gerelateerde onderdelen.
        /// </summary>
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        
        /// <summary>
        /// Collectie van alle onderdelen in het systeem.
        /// Inclusief voorraad informatie en gerelateerde producten.
        /// </summary>
        public IEnumerable<Part> Parts { get; set; } = new List<Part>();
    }
}
