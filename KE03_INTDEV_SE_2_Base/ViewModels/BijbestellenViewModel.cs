namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het bijbestellen van producten met lage voorraad.
    /// Wordt gebruikt voor automatische voorraad aanvulling en order suggesties.
    /// Focust op producten die onder een bepaalde voorraad drempel komen.
    /// </summary>
    public class BijbestellenViewModel
    {
        /// <summary>
        /// Unieke identifier van het product dat bijbesteld moet worden.
        /// Verwijst naar Product.Id in de database.
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// Naam van het product (verplicht veld).
        /// Wordt weergegeven in de bijbestel interface voor herkenning.
        /// </summary>
        public required string ProductName { get; set; }
        
        /// <summary>
        /// Huidige voorraad van dit product in het systeem.
        /// Gebruikt voor het bepalen of bijbestellen nodig is.
        /// </summary>
        public int CurrentStock { get; set; }
        
        /// <summary>
        /// Voorgestelde of ingevoerde hoeveelheid om bij te bestellen.
        /// Kan gebaseerd zijn op historische verkoop data of handmatig ingevoerd.
        /// </summary>
        public int OrderAmount { get; set; }
    }
}
