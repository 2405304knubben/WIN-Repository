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
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository; // Voeg deze toe

        public HomeController(ILogger<HomeController> logger, 
            MatrixIncDbContext context, 
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository) // Voeg deze parameter toe
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository; // Voeg deze toe
        }

        public IActionResult Index()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var viewModel = new DashboardViewModel
            {
                TotalOrders = _orderRepository.GetTotalOrdersCount(startDate, endDate),
                TotalRevenue = _orderRepository.GetTotalRevenue(startDate, endDate),
                TotalCustomers = _customerRepository.GetAllCustomers().Count(),
                DailyOrderCounts = _orderRepository.GetDailyOrderCounts(startDate, endDate).ToList(),
                DailyRevenue = _orderRepository.GetDailyRevenue(startDate, endDate).ToList(),
                StartDate = startDate,
                EndDate = endDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateStatistics([FromBody] DateRangeModel dateRange)
        {
            var startDate = DateTime.Parse(dateRange.startDate);
            var endDate = DateTime.Parse(dateRange.endDate);

            var dailyOrders = _orderRepository.GetDailyOrderCounts(startDate, endDate)
                .Select(x => new { date = x.Date.ToString("yyyy-MM-dd"), count = x.Count })
                .ToList();

            var dailyRevenue = _orderRepository.GetDailyRevenue(startDate, endDate)
                .Select(x => new { date = x.Date.ToString("yyyy-MM-dd"), revenue = x.Revenue })
                .ToList();

            var statistics = new
            {
                totalOrders = _orderRepository.GetTotalOrdersCount(startDate, endDate),
                totalRevenue = _orderRepository.GetTotalRevenue(startDate, endDate),
                totalCustomers = _customerRepository.GetAllCustomers().Count(),
                dailyOrderCounts = dailyOrders,
                dailyRevenue = dailyRevenue
            };

            return Json(statistics);
        }

        public class DateRangeModel
        {
            public string startDate { get; set; }
            public string endDate { get; set; }
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
