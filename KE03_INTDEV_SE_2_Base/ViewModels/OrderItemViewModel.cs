using System;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het weergeven van bestelde items in dropdown lijsten en selectie-interfaces
    /// Kan zowel producten als onderdelen vertegenwoordigen door gebruik van een Type property
    /// Gebruikt bij het maken en bewerken van bestellingen
    /// </summary>
    public class OrderItemViewModel
    {
        /// <summary>
        /// Unieke identificatie van het item (product of onderdeel)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Naam van het item voor weergave in de gebruikersinterface
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Type van het item: "Product" of "Part"
        /// Gebruikt om onderscheid te maken tussen producten en onderdelen in de view
        /// </summary>
        public string Type { get; set; }  // "Product" or "Part"
        
        /// <summary>
        /// Prijs per stuk van het item
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Huidige voorraad van het item
        /// Gebruikt voor validatie en weergave van beschikbaarheid
        /// </summary>
        public int Stock { get; set; }
        
        /// <summary>
        /// Gewenste hoeveelheid van het item
        /// Standaard ingesteld op 1 voor gebruiksvriendelijkheid
        /// </summary>
        public int Quantity { get; set; } = 1;
    }
}
