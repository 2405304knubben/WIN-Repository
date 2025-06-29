// Importeert alle benodigde namespaces voor diagnostiek, logging, MVC, data toegang en ViewModels
using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Interfaces;
using KE03_INTDEV_SE_2_Base.ViewModels;
using DataAccessLayer.Repositories;
using DataAccessLayer;
using DataAccessLayer.Models;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    /// <summary>
    /// Controller voor de hoofdpagina en dashboard functionaliteit van MatrixInc.
    /// Handelt statistieken, zoekfunctionaliteit en algemene navigatie af.
    /// </summary>
    public class HomeController : Controller
    {
        // Dependency injection van alle benodigde services
        private readonly ILogger<HomeController> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly MatrixIncDbContext _context;

        /// <summary>
        /// Constructor voor HomeController. Injecteert alle benodigde repositories en services.
        /// </summary>
        /// <param name="logger">Logger service voor event logging</param>
        /// <param name="context">Database context voor directe database toegang</param>
        /// <param name="orderRepository">Repository voor order gerelateerde operaties</param>
        /// <param name="customerRepository">Repository voor customer gerelateerde operaties</param>
        /// <param name="productRepository">Repository voor product gerelateerde operaties</param>
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

        /// <summary>
        /// Toont het hoofddashboard met bedrijfsstatistieken van de afgelopen 30 dagen.
        /// Berekent totaal aantal orders, omzet, klanten en dagelijkse trends.
        /// </summary>
        /// <returns>Dashboard view met DashboardViewModel</returns>
        public IActionResult Index()
        {
            // Definieer de periode voor statistieken (afgelopen 30 dagen)
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            // Creëer en vul het dashboard view model met alle benodigde statistieken
            var viewModel = new DashboardViewModel
            {
                // Totaal aantal bestellingen in de periode
                TotalOrders = _orderRepository.GetTotalOrdersCount(startDate, endDate),
                
                // Totale omzet in de periode
                TotalRevenue = _orderRepository.GetTotalRevenue(startDate, endDate),
                
                // Totaal aantal klanten in het systeem
                TotalCustomers = _customerRepository.GetAllCustomers().Count(),
                
                // Dagelijkse order aantallen voor grafiek weergave
                DailyOrderCounts = _orderRepository.GetDailyOrderCounts(startDate, endDate).ToList(),
                
                // Dagelijkse omzet voor grafiek weergave
                DailyRevenue = _orderRepository.GetDailyRevenue(startDate, endDate).ToList(),
                
                // Periode informatie voor weergave
                StartDate = startDate,
                EndDate = endDate
            };

            return View(viewModel);
        }

        /// <summary>
        /// AJAX endpoint voor het bijwerken van dashboard statistieken met een aangepaste datumperiode.
        /// Accepteert JSON data met start- en einddatum en retourneert bijgewerkte statistieken.
        /// </summary>
        /// <param name="dateRange">Object met startDate en endDate strings</param>
        /// <returns>JSON object met bijgewerkte statistieken</returns>
        [HttpPost]
        public IActionResult UpdateStatistics([FromBody] DateRangeModel dateRange)
        {
            // Valideer input parameters
            if (string.IsNullOrEmpty(dateRange.startDate) || string.IsNullOrEmpty(dateRange.endDate))
            {
                return BadRequest("Invalid date range");
            }
            
            // Parse de datum strings naar DateTime objecten
            var startDate = DateTime.Parse(dateRange.startDate);
            var endDate = DateTime.Parse(dateRange.endDate);

            // Haal dagelijkse order data op en format voor JSON response
            var dailyOrders = _orderRepository.GetDailyOrderCounts(startDate, endDate)
                .Select(x => new { date = x.Date.ToString("yyyy-MM-dd"), count = x.Count })
                .ToList();

            // Haal dagelijkse omzet data op en format voor JSON response
            var dailyRevenue = _orderRepository.GetDailyRevenue(startDate, endDate)
                .Select(x => new { date = x.Date.ToString("yyyy-MM-dd"), revenue = x.Revenue })
                .ToList();

            // Creëer response object met alle bijgewerkte statistieken
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

        /// <summary>
        /// Model klasse voor het ontvangen van datumperiode via AJAX requests.
        /// Gebruikt voor het filteren van dashboard statistieken.
        /// </summary>
        public class DateRangeModel
        {
            /// <summary>Start datum als string in YYYY-MM-DD formaat</summary>
            public string? startDate { get; set; }
            /// <summary>Eind datum als string in YYYY-MM-DD formaat</summary>
            public string? endDate { get; set; }
        }

        /// <summary>
        /// Verwerkt zoek requests van gebruikers via POST form submission.
        /// Zoekt naar producten en redirect naar details of overzicht pagina.
        /// Beveiligd tegen CSRF attacks door ValidateAntiForgeryToken.
        /// </summary>
        /// <param name="search">De zoekterm ingevoerd door de gebruiker</param>
        /// <returns>Redirect naar product details, product overzicht, of home pagina</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery
        public IActionResult Search(string search)
        {
            // Als geen zoekterm is ingevoerd, ga terug naar de homepage
            if (string.IsNullOrEmpty(search))
                return RedirectToAction("Index", "Home");

            // Haal alle producten op en filter op de zoekterm (case-insensitive)
            var allProducts = _productRepository.GetAllProducts();
            var matchingProducts = allProducts.Where(p => p.Name != null && 
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

            // Als er precies één match is, ga direct naar de product details pagina
            if (matchingProducts.Count() == 1)
            {
                return RedirectToAction("Details", "Product", new { id = matchingProducts.First().Id });
            }

            // Anders ga naar de product overzicht pagina met de zoekterm als filter
            return RedirectToAction("Index", "Product", new { searchTerm = search });
        }

        /// <summary>
        /// AJAX endpoint voor autocomplete/typeahead functionaliteit in de zoekbalk.
        /// Retourneert suggesties voor producten en onderdelen gebaseerd op de zoekterm.
        /// </summary>
        /// <param name="term">De (gedeeltelijke) zoekterm van de gebruiker</param>
        /// <returns>JSON array met zoek suggesties (max 10)</returns>
        [HttpGet]
        public IActionResult SearchSuggestions(string term)
        {
            // Als geen zoekterm, retourneer lege array
            if (string.IsNullOrEmpty(term))
                return Json(new string[] { });

            // Zoek naar matchende producten (case-insensitive)
            var allProducts = _productRepository.GetAllProducts();
            var productSuggestions = allProducts
                .Where(p => p.Name != null && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => new { 
                    label = p.Name,      // Weergave tekst
                    value = p.Id,        // Waarde voor identificatie
                    type = "product"     // Type voor client-side filtering
                });

            // Zoek naar matchende onderdelen (case-insensitive)
            var allParts = _context.Parts.ToList();
            var partSuggestions = allParts
                .Where(p => p.Name != null && p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => new {
                    label = p.Name,      // Weergave tekst
                    value = p.Id,        // Waarde voor identificatie
                    type = "part"        // Type voor client-side filtering
                });

            // Combineer product en part suggesties, limiteer tot 10 resultaten
            var suggestions = productSuggestions.Concat(partSuggestions).Take(10);
            return Json(suggestions);
        }

        /// <summary>
        /// Toont de privacy/algemene voorwaarden pagina.
        /// Statische informatie pagina zonder speciale functionaliteit.
        /// </summary>
        /// <returns>Privacy view</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Toont een error pagina met debugging informatie.
        /// Wordt gebruikt voor onverwachte fouten in de applicatie.
        /// Response caching is uitgeschakeld voor actuele error informatie.
        /// </summary>
        /// <returns>Error view met request details</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Creëer error model met trace identifier voor debugging
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
