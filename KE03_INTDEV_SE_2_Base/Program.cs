// Importeert alle benodigde namespaces voor de applicatie
using DataAccessLayer;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace KE03_INTDEV_SE_2_Base
{
    /// <summary>
    /// Hoofdklasse van de MatrixInc webapplicatie.
    /// Deze klasse configureert en start de ASP.NET Core applicatie met alle benodigde services.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Het startpunt van de applicatie. Configureert services, middleware en start de webserver.
        /// </summary>
        /// <param name="args">Commandline argumenten voor de applicatie</param>
        public static void Main(string[] args)
        {
            // Creëert een WebApplication builder voor het configureren van services en middleware
            var builder = WebApplication.CreateBuilder(args);

            // *** SERVICE CONFIGURATIE ***
            // Configureer Entity Framework met SQLite database
            // We gebruiken SQLite voor eenvoudige lokale ontwikkeling zonder complexe database setup
            builder.Services.AddDbContext<MatrixIncDbContext>(
                options => options.UseSqlite("Data Source=MatrixInc.db"));
            
            // Voegt MVC controllers en views toe aan de service container
            builder.Services.AddControllersWithViews();

            // Configureer sessie beheer voor gebruikersinteractie
            // Sessies worden gebruikt voor tijdelijke opslag van gebruikersdata
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Sessie verloopt na 30 minuten inactiviteit
                options.Cookie.HttpOnly = true;                 // Cookie alleen toegankelijk via HTTP (niet JavaScript)
                options.Cookie.IsEssential = true;              // Cookie is essentieel voor de applicatie
            });

            // Registreer alle repository interfaces en implementaties in de Dependency Injection container
            // Dit maakt het mogelijk om repositories te injecteren in controllers
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IPartRepository, PartRepository>();

            // Configureer cookie-gebaseerde authenticatie
            // Gebruikt cookies om gebruikers ingelogd te houden tussen requests
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";           // Redirect naar login pagina als niet ingelogd
                    options.LogoutPath = "/Account/Logout";         // Pad voor uitloggen
                    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect bij geen toegang
                });

            // Bouw de WebApplication met alle geconfigureerde services
            var app = builder.Build();

            // *** MIDDLEWARE PIPELINE CONFIGURATIE ***
            // Configureer verschillende middleware gebaseerd op de omgeving
            if (!app.Environment.IsDevelopment())
            {
                // In productie: gebruik globale exception handler
                app.UseExceptionHandler("/Home/Error");
                // Voeg HTTP Strict Transport Security toe voor beveiliging (30 dagen standaard)
                app.UseHsts();
            }

            // *** DATABASE INITIALISATIE ***
            // Zorg ervoor dat de database bestaat en is gevuld met initiele testdata
            // Dit gebeurt bij elke applicatie start om consistente data te garanderen
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Verkrijg de database context en initialiseer de database
                var context = services.GetRequiredService<MatrixIncDbContext>();
                context.Database.EnsureCreated();      // Creëer database als deze niet bestaat
                MatrixIncDbInitializer.Initialize(context); // Vul database met testdata
            }

            // *** HTTP PIPELINE CONFIGURATIE ***
            // De volgorde van middleware is belangrijk!
            app.UseHttpsRedirection();  // Redirect HTTP naar HTTPS voor beveiliging
            app.UseStaticFiles();       // Serve statische bestanden (CSS, JS, afbeeldingen)

            app.UseRouting();           // Configureer routing voor URL's naar controllers/actions
            app.UseSession();           // Activeer sessie ondersteuning (moet voor UseAuthentication)
            app.UseAuthentication();    // Activeer authenticatie middleware (controleer wie gebruiker is)
            app.UseAuthorization();     // Activeer autorisatie middleware (controleer wat gebruiker mag)

            // Configureer de standaard MVC route pattern
            // Standaard route gaat naar Account controller in plaats van Home voor login-first benadering
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Index}/{id?}");

            // Start de webserver en luister naar incoming requests
            app.Run();
        }
    }
}
