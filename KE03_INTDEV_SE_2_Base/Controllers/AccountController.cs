// Importeert alle benodigde namespaces voor diagnostiek, logging, authenticatie en data toegang
using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Repositories;
using DataAccessLayer;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    /// <summary>
    /// Controller verantwoordelijk voor gebruikersauthenticatie en accountbeheer.
    /// Handelt login, logout en sessie management af voor de MatrixInc applicatie.
    /// </summary>
    public class AccountController : Controller
    {
        // Logger voor het vastleggen van gebeurtenissen en fouten in deze controller
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Constructor voor AccountController. Injecteert de logger service.
        /// </summary>
        /// <param name="logger">Logger service voor het vastleggen van events</param>
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Toont de login pagina aan gebruikers die nog niet zijn ingelogd.
        /// Dit is de standaard entry point van de applicatie.
        /// </summary>
        /// <returns>Login view</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Verwerkt login attempts van gebruikers via POST request.
        /// Controleert credentials en creëert authenticatie cookie bij succes.
        /// </summary>
        /// <param name="username">De ingevoerde gebruikersnaam</param>
        /// <returns>Redirect naar Home bij succes, anders terug naar login met foutmelding</returns>
        [HttpPost]
        public async Task<IActionResult> Index(string username)
        {
            // Controleer of de gebruikersnaam "admin" is (case-insensitive)
            // In een productie omgeving zou dit via een database/identity provider gaan
            if (username?.ToLower() == "admin")
            {
                // *** AUTHENTICATIE COOKIE AANMAKEN ***
                // Creëer claims voor de gebruiker (identiteit en rol informatie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),        // Gebruikersnaam claim
                    new Claim(ClaimTypes.Role, "Administrator")  // Rol claim voor autorisatie
                };
                
                // Creëer een claims identity met cookie authenticatie scheme
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Login de gebruiker door een authenticatie cookie te maken
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // *** SESSIE DATA INSTELLEN ***
                // Sla extra gebruikersinformatie op in de sessie voor later gebruik
                HttpContext.Session.SetString("LastLoginTime", DateTime.Now.ToString());
                HttpContext.Session.SetString("UserPreference", "dark-mode");

                // Redirect naar de hoofdpagina na succesvolle login
                return RedirectToAction("Index", "Home");
            }

            // Login gefaald - toon foutmelding en blijf op login pagina
            ViewData["ErrorMessage"] = "Ongeldige gebruikersnaam";
            return View();
        }

        /// <summary>
        /// Logt de huidige gebruiker uit door de authenticatie cookie te verwijderen.
        /// Beveiligd tegen CSRF attacks door ValidateAntiForgeryToken.
        /// </summary>
        /// <returns>Redirect naar login pagina</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // Bescherming tegen Cross-Site Request Forgery attacks
        public async Task<IActionResult> Logout()
        {
            // Verwijder de authenticatie cookie om de gebruiker uit te loggen
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Redirect terug naar de login pagina
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Toont een error pagina met debugging informatie.
        /// Wordt gebruikt wanneer er onverwachte fouten optreden in de applicatie.
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
