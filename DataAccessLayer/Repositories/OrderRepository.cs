// Importeert interfaces, models, Entity Framework en standaard namespaces
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    /// <summary>
    /// Implementatie van IOrderRepository voor order gerelateerde database operaties.
    /// Biedt volledige CRUD functionaliteit plus geavanceerde statistiek en rapportage functies.
    /// Gebruikt Entity Framework Core voor data toegang met eager loading voor performance.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        // Database context voor data toegang
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor OrderRepository. Injecteert de database context.
        /// </summary>
        /// <param name="context">Database context voor Entity Framework operaties</param>
        public OrderRepository(MatrixIncDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Voegt een nieuwe bestelling toe aan de database en slaat wijzigingen direct op.
        /// </summary>
        /// <param name="order">Het Order object om toe te voegen</param>
        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Verwijdert een bestelling uit de database en slaat wijzigingen direct op.
        /// Let op: Dit verwijdert ook alle gekoppelde OrderProduct records via cascade delete.
        /// </summary>
        /// <param name="order">Het Order object om te verwijderen</param>
        public void DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges(); // Direct opslaan in database
        }

        /// <summary>
        /// Haalt alle bestellingen op inclusief gerelateerde klant en product informatie.
        /// Gebruikt eager loading om N+1 query problemen te voorkomen.
        /// </summary>
        /// <returns>Collectie van alle Order objecten met Customer en OrderProducts</returns>
        public IEnumerable<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Customer)              // Laad klant informatie
                .Include(o => o.OrderProducts)         // Laad order-product koppelingen
                    .ThenInclude(op => op.Product);    // Laad product details
        }

        /// <summary>
        /// Haalt een specifieke bestelling op via ID inclusief alle gerelateerde data.
        /// </summary>
        /// <param name="id">Unieke identifier van de bestelling</param>
        /// <returns>Order object met alle relaties of null als niet gevonden</returns>
        public Order? GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.Customer)              // Laad klant informatie
                .Include(o => o.OrderProducts)         // Laad order-product koppelingen
                    .ThenInclude(op => op.Product)     // Laad product details
                .FirstOrDefault(o => o.Id == id);
        }

        /// <summary>
        /// Werkt een bestaande bestelling bij in de database en slaat wijzigingen direct op.
        /// Entity Framework tracking zorgt voor automatische change detection.
        /// </summary>
        /// <param name="order">Het Order object met bijgewerkte gegevens</param>
        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges(); // Direct opslaan in database
        }

        // *** STATISTIEK EN RAPPORTAGE METHODEN ***

        /// <summary>
        /// Berekent het totaal aantal unieke bestellingen voor een bepaalde periode.
        /// Voorkomt dubbele telling door gebruik van Distinct() op Order ID's.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode (optioneel)</param>
        /// <param name="endDate">Einddatum van de periode (optioneel)</param>
        /// <returns>Totaal aantal unieke bestellingen</returns>
        public int GetTotalOrdersCount(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            // Filter op startdatum als opgegeven
            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate.Date >= startDate.Value.Date);
            
            // Filter op einddatum als opgegeven
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate.Date <= endDate.Value.Date);

            // Tel het aantal unieke orders voor betere performance
            return query
                .Select(o => o.Id)      // Selecteer alleen de ID's
                .Distinct()             // Voorkom dubbele telling
                .Count();
        }

        /// <summary>
        /// Berekent de totale omzet voor een bepaalde periode.
        /// Sommeert alle product prijzen vermenigvuldigd met aantallen in bestellingen.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode (optioneel)</param>
        /// <param name="endDate">Einddatum van de periode (optioneel)</param>
        /// <returns>Totale omzet in decimalen</returns>
        public decimal GetTotalRevenue(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderProducts)         // Laad order-product koppelingen
                .ThenInclude(op => op.Product)         // Laad product prijzen
                .AsQueryable();

            // Filter op startdatum als opgegeven
            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate.Date >= startDate.Value.Date);
            
            // Filter op einddatum als opgegeven
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate.Date <= endDate.Value.Date);

            // Bereken totale omzet (prijs × aantal voor elk product)
            return query.AsEnumerable()
                .Sum(o => o.OrderProducts.Sum(op => op.Product.Price * op.Aantal));
        }

        /// <summary>
        /// Haalt dagelijkse order aantallen op voor een bepaalde periode.
        /// Vult automatisch missende datums in met 0 voor complete dataset.
        /// Gebruikt voor trending en grafiek generatie in het dashboard.
        /// </summary>
        /// <param name="startDate">Startdatum van de periode</param>
        /// <param name="endDate">Einddatum van de periode</param>
        /// <returns>Collectie van datum-aantal tupels voor elke dag in de periode</returns>
        public IEnumerable<(DateTime Date, int Count)> GetDailyOrderCounts(DateTime startDate, DateTime endDate)
        {
            // Normaliseer datums naar alleen datum (geen tijd) voor accurate vergelijking
            var dateStart = startDate.Date;
            var dateEnd = endDate.Date;

            // Haal order data op gegroepeerd per dag
            var orders = _context.Orders
                .Where(o => o.OrderDate.Date >= dateStart && o.OrderDate.Date <= dateEnd)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            // Genereer alle datums in de periode om missende dagen in te vullen
            var allDates = Enumerable.Range(0, (dateEnd - dateStart).Days + 1)
                .Select(offset => dateStart.AddDays(offset))
                .ToList();

            var result = allDates.Select(date => (
                Date: date,
                Count: orders.FirstOrDefault(o => o.Date == date)?.Count ?? 0
            ));

            return result;
        }        public IEnumerable<(DateTime Date, decimal Revenue)> GetDailyRevenue(DateTime startDate, DateTime endDate)
        {
            var dateStart = startDate.Date;
            var dateEnd = endDate.Date;

            var revenues = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.OrderDate.Date >= dateStart && o.OrderDate.Date <= dateEnd)
                .AsEnumerable() // Switch to client-side evaluation
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .ToList();

            // Vul missende datums aan
            var allDates = Enumerable.Range(0, (dateEnd - dateStart).Days + 1)
                .Select(offset => dateStart.AddDays(offset))
                .ToList();

            return allDates
                .GroupJoin(
                    revenues,
                    date => date,
                    rev => rev.Date,
                    (date, revs) => (
                        Date: date,
                        Revenue: revs.FirstOrDefault()?.Revenue ?? 0m
                    ))
                .OrderBy(x => x.Date);
        }

        public int GetNewCustomersCount(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Customers
                .Where(c => c.Orders.Any());

            if (startDate.HasValue)
                query = query.Where(c => c.Orders.Min(o => o.OrderDate) >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(c => c.Orders.Min(o => o.OrderDate) <= endDate.Value);

            return query.Count();
        }
    }
}
