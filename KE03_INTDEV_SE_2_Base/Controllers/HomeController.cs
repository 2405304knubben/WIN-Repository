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
        private readonly IProductRepository _productRepository;
        private readonly MatrixIncDbContext _context;

        public HomeController(ILogger<HomeController> logger, 
            MatrixIncDbContext context, 
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository)
        {
            _logger = logger;
            _context = context;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
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
            if (string.IsNullOrEmpty(dateRange.startDate) || string.IsNullOrEmpty(dateRange.endDate))
            {
                return BadRequest("Invalid date range");
            }
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
            public string? startDate { get; set; }
            public string? endDate { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string search)
        {
            if (string.IsNullOrEmpty(search))
                return RedirectToAction("Index", "Home");

            var allProducts = _productRepository.GetAllProducts();
            var matchingProducts = allProducts.Where(p => p.Name != null && 
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            if (matchingProducts.Count() == 1)
            {
                return RedirectToAction("Details", "Product", new { id = matchingProducts.First().Id });
            }

            return RedirectToAction("Index", "Product", new { searchTerm = search });
        }

        [HttpGet]
        public IActionResult SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new string[] { });

            var allProducts = _productRepository.GetAllProducts();
            var productSuggestions = allProducts
                .Where(p => p.Name != null && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => new { 
                    label = p.Name,
                    value = p.Id,
                    type = "product"
                });

            // Get all parts that match the search term
            var allParts = _context.Parts.ToList();
            var partSuggestions = allParts
                .Where(p => p.Name != null && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => new {
                    label = p.Name,
                    value = p.Id,
                    type = "part"
                });

            var suggestions = productSuggestions.Concat(partSuggestions).Take(10);
            return Json(suggestions);
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
    }
}
