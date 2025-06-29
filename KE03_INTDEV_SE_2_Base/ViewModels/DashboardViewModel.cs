// Importeert standaard namespaces voor datum/tijd en collections
using System;
using System.Collections.Generic;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    /// <summary>
    /// ViewModel voor het dashboard met bedrijfsstatistieken.
    /// Bevat alle data die nodig is voor het weergeven van grafieken en KPI's op de hoofdpagina.
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Totaal aantal bestellingen in de geselecteerde periode.
        /// Wordt weergegeven als KPI card op het dashboard.
        /// </summary>
        public int TotalOrders { get; set; }
        
        /// <summary>
        /// Totale omzet in euro's voor de geselecteerde periode.
        /// Wordt weergegeven als KPI card op het dashboard.
        /// </summary>
        public decimal TotalRevenue { get; set; }
        
        /// <summary>
        /// Totaal aantal klanten in het hele systeem (niet periode gebonden).
        /// Gebruikt voor algemene bedrijfsstatistieken.
        /// </summary>
        public int TotalCustomers { get; set; }
        
        /// <summary>
        /// Dagelijkse order aantallen voor de geselecteerde periode.
        /// Wordt gebruikt voor het genereren van lijngrafieken in het dashboard.
        /// </summary>
        public List<(DateTime Date, int Count)> DailyOrderCounts { get; set; }
        
        /// <summary>
        /// Dagelijkse omzet cijfers voor de geselecteerde periode.
        /// Wordt gebruikt voor het genereren van omzet grafieken in het dashboard.
        /// </summary>
        public List<(DateTime Date, decimal Revenue)> DailyRevenue { get; set; }
        
        /// <summary>
        /// Startdatum van de geselecteerde periode voor rapportage.
        /// Wordt gebruikt voor datumfiltering en weergave.
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// Einddatum van de geselecteerde periode voor rapportage.
        /// Wordt gebruikt voor datumfiltering en weergave.
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
