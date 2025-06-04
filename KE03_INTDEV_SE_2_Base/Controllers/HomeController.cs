using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Interfaces;
using KE03_INTDEV_SE_2_Base.ViewModels;
using DataAccessLayer.Repositories;
using DataAccessLayer;
using DataAccessLayer.Models;
namespace KE03_INTDEV_SE_2_Base.Controllers
{    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ProductRepository _productRepository;
        private readonly MatrixIncDbContext _context;
        private Product matchingProduct;
        private readonly IOrderRepository _orderRepository;

        public HomeController(ILogger<HomeController> logger, MatrixIncDbContext context, IOrderRepository orderRepository)
        {
            _logger = logger;
            _productRepository = new ProductRepository(context);
            _orderRepository = orderRepository;
        }

        public IActionResult Index()
        {
            // Default to last 30 days
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var viewModel = new DashboardViewModel
            {
                TotalOrders = _orderRepository.GetTotalOrdersCount(startDate, endDate),
                TotalRevenue = _orderRepository.GetTotalRevenue(startDate, endDate),
                NewCustomers = _orderRepository.GetNewCustomersCount(startDate, endDate),
                DailyOrderCounts = _orderRepository.GetDailyOrderCounts(startDate, endDate).ToList(),
                DailyRevenue = _orderRepository.GetDailyRevenue(startDate, endDate).ToList(),
                StartDate = startDate,
                EndDate = endDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateStatistics(DateTime startDate, DateTime endDate)
        {
            var statistics = new
            {
                totalOrders = _orderRepository.GetTotalOrdersCount(startDate, endDate),
                totalRevenue = _orderRepository.GetTotalRevenue(startDate, endDate),
                newCustomers = _orderRepository.GetNewCustomersCount(startDate, endDate),
                dailyOrderCounts = _orderRepository.GetDailyOrderCounts(startDate, endDate),
                dailyRevenue = _orderRepository.GetDailyRevenue(startDate, endDate)
            };

            return Json(statistics);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string search)
        {
            if (string.IsNullOrEmpty(search))
                return RedirectToAction("Index", "Home");

            var allProducts = _productRepository.GetAllProducts();
            Product matchingProduct = null;
            foreach (var product in allProducts)
            {
                if (product.Name != null && product.Name.Equals(search))
                {
                    matchingProduct = product;
                    break;
                }
            }

            if (matchingProduct != null)
            {
                return RedirectToAction("Details", "Product", new { id = matchingProduct.Id });
            }

            return RedirectToAction("Index", "Product");
        }
        
    }
}
