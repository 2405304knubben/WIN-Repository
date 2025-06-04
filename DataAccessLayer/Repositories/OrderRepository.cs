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
    public class OrderRepository : IOrderRepository
    {
        private readonly MatrixIncDbContext _context;

        public OrderRepository(MatrixIncDbContext context)
        {
            _context = context;
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public void DeleteOrder(Order order)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }

        public IEnumerable<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product);
        }

        public Order? GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefault(o => o.Id == id);
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public int GetTotalOrdersCount(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate.Date <= endDate.Value.Date);

            // Tel het aantal unieke orders
            return query
                .Select(o => o.Id) // Selecteer alleen de Id's voor betere performance
                .Distinct() // Zorg ervoor dat we geen dubbele orders tellen
                .Count();
        }

        public decimal GetTotalRevenue(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate.Date >= startDate.Value.Date);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate.Date <= endDate.Value.Date);

            return query.AsEnumerable()
                .Sum(o => o.OrderProducts.Sum(op => op.Product.Price * op.Aantal));
        }        public IEnumerable<(DateTime Date, int Count)> GetDailyOrderCounts(DateTime startDate, DateTime endDate)
        {
            // Zorg ervoor dat we alleen de datum vergelijken, niet de tijd
            var dateStart = startDate.Date;
            var dateEnd = endDate.Date;

            var orders = _context.Orders
                .Where(o => o.OrderDate.Date >= dateStart && o.OrderDate.Date <= dateEnd)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList();

            // Vul missende datums in met 0
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
