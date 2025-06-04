using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public int CustomerId { get; set; }
        
        public Customer Customer { get; set; } = null!;        public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

        public decimal TotalPrice => OrderProducts.Sum(op => op.Product.Price * op.Aantal);
    }
}
