using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class OrderViewModel
    {
        public IEnumerable<OrderDetailViewModel> OrderDataForCustomer { get; set; } = new List<OrderDetailViewModel>();
    }

    public class OrderDetailViewModel
    {
        public Order Order { get; set; }
        public decimal TotalOrderPrice => ProductDetails.Sum(p => p.TotalPrice);
        public List<ProductDetailViewModel> ProductDetails { get; set; } = new List<ProductDetailViewModel>();
    }

    public class ProductDetailViewModel
    {
        public string ProductName { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Amount * Price;
    }
}
