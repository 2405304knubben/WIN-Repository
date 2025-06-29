// Importeert data access layer models
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// Hoofdklasse ViewModel voor het weergeven van order overzichten.
    /// Bevat een collectie van gedetailleerde order informatie voor lijst weergaves.
    /// </summary>
    public class OrderViewModel
    {
        /// <summary>
        /// Collectie van gedetailleerde order informatie voor alle bestellingen.
        /// Elke OrderDetailViewModel bevat volledige order informatie inclusief producten.
        /// </summary>
        public IEnumerable<OrderDetailViewModel> OrderDataForCustomer { get; set; } = new List<OrderDetailViewModel>();
    }

    /// <summary>
    /// ViewModel voor gedetailleerde weergave van een individuele bestelling.
    /// Bevat order informatie, totaalprijs berekening en product details.
    /// </summary>
    public class OrderDetailViewModel
    {
        /// <summary>
        /// De order entiteit met basis informatie (datum, klant, etc.).
        /// </summary>
        public Order Order { get; set; }
        
        /// <summary>
        /// Berekende eigenschap voor de totale prijs van deze bestelling.
        /// Sommeert alle product prijzen × aantallen in de bestelling.
        /// </summary>
        public decimal TotalOrderPrice => ProductDetails.Sum(p => p.TotalPrice);
        
        /// <summary>
        /// Lijst van product details voor alle items in deze bestelling.
        /// Bevat naam, aantal, prijs en type informatie per product.
        /// </summary>
        public List<ProductDetailViewModel> ProductDetails { get; set; } = new List<ProductDetailViewModel>();
    }

    /// <summary>
    /// ViewModel voor individuele product informatie binnen een bestelling.
    /// Bevat product details, aantal, prijs en type classificatie.
    /// </summary>
    public class ProductDetailViewModel
    {
        /// <summary>
        /// Naam van het product of onderdeel.
        /// Kan een suffix bevatten zoals "(Onderdeel)" voor type indicatie.
        /// </summary>
        public string ProductName { get; set; }
        
        /// <summary>
        /// Aantal stuks van dit product in de bestelling.
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// Prijs per stuk van dit product.
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Berekende eigenschap voor totale prijs van dit product (aantal × prijs).
        /// </summary>
        public decimal TotalPrice => Amount * Price;
        
        /// <summary>
        /// Type classificatie van het item: "Product" of "Onderdeel".
        /// Gebruikt voor filtering en weergave styling.
        /// </summary>
        public string Type { get; set; } = "Product";
    }
}
