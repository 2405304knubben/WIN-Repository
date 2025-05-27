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
    }
}
