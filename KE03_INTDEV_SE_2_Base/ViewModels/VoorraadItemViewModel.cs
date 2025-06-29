namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor individuele voorraad items in bestelsessies.
    /// Wordt gebruikt voor tijdelijke opslag van voorraad bestellingen in de HTTP sessie.
    /// Ondersteunt zowel producten als onderdelen met unified interface.
    /// </summary>
    public class VoorraadItemViewModel
    {
        /// <summary>
        /// Unieke identifier van het product of onderdeel.
        /// Verwijst naar Product.Id of Part.Id afhankelijk van Type.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Naam van het product of onderdeel (verplicht veld).
        /// Wordt weergegeven in de voorraad bestelling interface.
        /// </summary>
        public required string Name { get; set; }
        
        /// <summary>
        /// Huidige voorraad van dit item in het systeem.
        /// Wordt getoond voor referentie bij het bestellen.
        /// </summary>
        public int CurrentStock { get; set; }
        
        /// <summary>
        /// Aantal stuks dat besteld moet worden (standaard 1).
        /// Kan door gebruiker worden aangepast in de bestelling interface.
        /// </summary>
        public int OrderAmount { get; set; } = 1;
        
        /// <summary>
        /// Type classificatie: "Product" of "Part" (verplicht veld).
        /// Gebruikt voor onderscheid tussen producten en onderdelen in de interface.
        /// </summary>
        public required string Type { get; set; }
        
        /// <summary>
        /// Prijs per stuk van dit item.
        /// Gebruikt voor kostencalculatie van de bestelling.
        /// </summary>
        public decimal Price { get; set; }
    }
}
