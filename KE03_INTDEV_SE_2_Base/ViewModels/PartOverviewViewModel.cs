using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het overzicht van onderdelen en producten
    /// Combineert beide entiteiten voor een geïntegreerde weergave in de view
    /// Gebruikt voor overzichtspagina's waar zowel producten als onderdelen getoond worden
    /// </summary>
    public class PartOverviewViewModel
    {
        /// <summary>
        /// Collectie van alle beschikbare producten
        /// Geïnitialiseerd als lege lijst om null reference exceptions te voorkomen
        /// </summary>
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        
        /// <summary>
        /// Collectie van alle beschikbare onderdelen
        /// Geïnitialiseerd als lege lijst om null reference exceptions te voorkomen
        /// </summary>
        public IEnumerable<Part> Parts { get; set; } = new List<Part>();
    }
}
