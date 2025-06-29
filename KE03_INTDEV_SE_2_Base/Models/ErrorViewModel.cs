namespace KE03_INTDEV_SE_2_Base.Models
{
    /// <summary>
    /// ViewModel voor het weergeven van error informatie aan gebruikers.
    /// Gebruikt door error handling middleware en exception filters.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Unieke identifier voor de request die een fout heeft veroorzaakt.
        /// Wordt gebruikt voor logging en debugging doeleinden.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Bepaalt of de Request ID moet worden getoond aan de gebruiker.
        /// Alleen tonen als er een geldige Request ID beschikbaar is.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
