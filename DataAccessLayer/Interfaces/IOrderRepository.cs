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
    /// Interface voor order gerelateerde database operaties en statistieken.
    /// Definieert de contract voor CRUD operaties en business intelligence functionaliteit.
    /// Implementeert het Repository pattern voor data abstractie.
    /// </summary>
    public interface IOrderRepository
    {
        // *** BASIC CRUD OPERATIES ***
        
        /// <summary>
        /// Haalt alle bestellingen op uit de database.
        /// </summary>
        /// <returns>Collectie van alle Order objecten</returns>
        public IEnumerable<Order> GetAllOrders();

        /// <summary>
        /// Haalt een specifieke bestelling op via het ID.
        /// </summary>
        /// <param name="id">Unieke identifier van de bestelling</param>
        /// <returns>Order object of null als niet gevonden</returns>
        public Order? GetOrderById(int id);

        /// <summary>
        /// Voegt een nieuwe bestelling toe aan de database.
        /// </summary>
        /// <param name="order">Het Order object om toe te voegen</param>
        public void AddOrder(Order order);

        /// <summary>
        /// Werkt een bestaande bestelling bij in de database.
        /// </summary>
        /// <param name="order">Het Order object met bijgewerkte gegevens</param>
        public void UpdateOrder(Order order);

        /// <summary>
        /// Verwijdert een bestelling uit de database.
        /// </summary>
        /// <param name="order">Het Order object om te verwijderen</param>
        public void DeleteOrder(Order order);

        // *** STATISTIEK EN RAPPORTAGE METHODEN ***
        
        /// <summary>
        /// Berekent het totaal aantal bestellingen voor een bepaalde periode.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode (optioneel)</param>
        /// <param name="endDate">Einddatum van de periode (optioneel)</param>
        /// <returns>Totaal aantal bestellingen</returns>
        public int GetTotalOrdersCount(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Berekent de totale omzet voor een bepaalde periode.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode (optioneel)</param>
        /// <param name="endDate">Einddatum van de periode (optioneel)</param>
        /// <returns>Totale omzet in decimalen</returns>
        public decimal GetTotalRevenue(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Haalt dagelijkse order aantallen op voor een bepaalde periode.
        /// Gebruikt voor trending en grafiek generatie.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode</param>
        /// <param name="endDate">Einddatum van de periode</param>
        /// <returns>Collectie van datum-aantal tupels</returns>
        public IEnumerable<(DateTime Date, int Count)> GetDailyOrderCounts(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Haalt dagelijkse omzet cijfers op voor een bepaalde periode.
        /// Gebruikt voor trending en grafiek generatie.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode</param>
        /// <param name="endDate">Einddatum van de periode</param>
        /// <returns>Collectie van datum-omzet tupels</returns>
        public IEnumerable<(DateTime Date, decimal Revenue)> GetDailyRevenue(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Berekent het aantal nieuwe klanten voor een bepaalde periode.
        /// Gebruikt voor groei statistieken en marketing insights.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode (optioneel)</param>
        /// <param name="endDate">Einddatum van de periode (optioneel)</param>
        /// <returns>Aantal nieuwe klanten</returns>
        public int GetNewCustomersCount(DateTime? startDate = null, DateTime? endDate = null);
    }
}
