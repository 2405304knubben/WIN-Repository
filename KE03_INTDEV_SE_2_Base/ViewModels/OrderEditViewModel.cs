using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class OrderEditViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderProductEditViewModel> OrderProducts { get; set; } = new List<OrderProductEditViewModel>();
        public List<OrderItemViewModel> AvailableItems { get; set; } = new List<OrderItemViewModel>();

        public static OrderEditViewModel FromOrder(Order order)
        {
            return new OrderEditViewModel
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                OrderProducts = order.OrderProducts.Select(op => new OrderProductEditViewModel
                {
                    ProductId = op.Product.Id,
                    ProductName = op.Product.Name,
                    CurrentStock = op.Product.Stock,
                    Quantity = op.Aantal,
                    Price = op.Product.Price
                }).ToList()
            };
        }
    }    public class OrderProductEditViewModel
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }
}
