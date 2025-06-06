using System;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }  // "Product" or "Part"
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
