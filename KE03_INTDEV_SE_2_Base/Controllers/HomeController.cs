using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(ILogger<HomeController> logger, MatrixIncDbContext context)
        {
            _logger = logger;
            _productRepository = new ProductRepository(context);
        }

        public IActionResult Index()
        {
            return View();
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
