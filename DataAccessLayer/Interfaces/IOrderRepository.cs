using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IOrderRepository
    {
        public IEnumerable<Order> GetAllOrders();

        public Order? GetOrderById(int id);

        public void AddOrder(Order order);

        public void UpdateOrder(Order order);

        public void DeleteOrder(Order order);

        // Statistics methods
        public int GetTotalOrdersCount(DateTime? startDate = null, DateTime? endDate = null);

        public decimal GetTotalRevenue(DateTime? startDate = null, DateTime? endDate = null);

        public IEnumerable<(DateTime Date, int Count)> GetDailyOrderCounts(DateTime startDate, DateTime endDate);

        public IEnumerable<(DateTime Date, decimal Revenue)> GetDailyRevenue(DateTime startDate, DateTime endDate);

        public int GetNewCustomersCount(DateTime? startDate = null, DateTime? endDate = null);
    }
}
