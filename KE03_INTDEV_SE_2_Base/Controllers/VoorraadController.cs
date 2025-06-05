using System.Diagnostics;
using KE03_INTDEV_SE_2_Base.Models;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Repositories;
using DataAccessLayer;
using DataAccessLayer.Models;
using KE03_INTDEV_SE_2_Base.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace KE03_INTDEV_SE_2_Base.Controllers
{
    public class VoorraadController : Controller
    {
        private readonly ILogger<VoorraadController> _logger;
        private readonly MatrixIncDbContext _context;

        public VoorraadController(ILogger<VoorraadController> logger, MatrixIncDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new ProductOverviewViewModel
            {
                Products = await _context.Products.Include(p => p.Parts).ToListAsync(),
                Parts = await _context.Parts.Include(p => p.Products).ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Order()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }        public async Task<IActionResult> Ordercreate(string type = "normaal")
        {
            ViewBag.OrderType = type;
            var viewModel = new OrderEditViewModel();
            
            if (type == "normaal")
            {
                viewModel.AvailableItems = await _context.Products
                    .Select(p => new OrderItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Stock = p.Stock,
                        Price = p.Price
                    })
                    .ToListAsync();
            }
            else
            {
                viewModel.AvailableItems = await _context.Parts
                    .Select(p => new OrderItemViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Stock = p.Stock,
                        Price = p.Price
                    })
                    .ToListAsync();
            }

            return View(viewModel);
        }

        public IActionResult OrderConfirm()
        {
            return View();
        }
    }
}