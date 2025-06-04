using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.ViewModels
{
    public class ProductOverviewViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Part> Parts { get; set; } = new List<Part>();
    }
}
