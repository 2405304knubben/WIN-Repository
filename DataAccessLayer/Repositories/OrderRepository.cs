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
                query = query.Where(o => o.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return query.Count();
        }

        public decimal GetTotalRevenue(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            return query.ToList().Sum(o => o.TotalPrice);
        }        public IEnumerable<(DateTime Date, int Count)> GetDailyOrderCounts(DateTime startDate, DateTime endDate)
        {
            var orders = _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToList();

            // Genereer een lijst van alle datums in de range
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset).Date);

            // Join de orders met alle datums om ook datums zonder orders te krijgen (met count 0)
            return allDates
                .GroupJoin(
                    orders,
                    date => date,
                    order => order.OrderDate.Date,
                    (date, ordersOnDate) => (
                        Date: date,
                        Count: ordersOnDate.Count()
                    ))
                .OrderBy(x => x.Date);
        }        public IEnumerable<(DateTime Date, decimal Revenue)> GetDailyRevenue(DateTime startDate, DateTime endDate)
        {
            var orders = _context.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .ToList();

            // Genereer een lijst van alle datums in de range
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset).Date);

            // Join de orders met alle datums om ook datums zonder orders te krijgen (met 0 revenue)
            return allDates
                .GroupJoin(
                    orders,
                    date => date,
                    order => order.OrderDate.Date,
                    (date, ordersOnDate) => (
                        Date: date,
                        Revenue: ordersOnDate.Any() ? ordersOnDate.Sum(o => o.TotalPrice) : 0m
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
