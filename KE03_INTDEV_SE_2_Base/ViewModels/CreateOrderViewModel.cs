using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class CreateOrderViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Customer> Customers { get; set; } = new List<Customer>();
    }
}
