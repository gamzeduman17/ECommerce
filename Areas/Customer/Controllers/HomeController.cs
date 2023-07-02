using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;


        public HomeController(ILogger<HomeController> logger,ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var products = _db.Products.Where(i => i.IsHome).ToList();
            if(claim != null)
            {
                var count = _db.ShoppingCards.Where(i => i.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(Others.ssShoppingCard, count);


            }
            return View(products);
        }

        public IActionResult Details(int id)

        {
            var product = _db.Products.FirstOrDefault(i => i.Id == id);
            ShoppingCard card = new ShoppingCard()
            {
                Product = product,
                ProductId = product.Id

            };

           return View(card);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCard Scard)

        {
            Scard.Id = 0;
            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                Scard.ApplicationUserId = claim.Value;
                ShoppingCard card = _db.ShoppingCards.FirstOrDefault(u => u.ApplicationUserId == Scard.ApplicationUserId && u.ProductId == Scard.ProductId);
                if(card == null)
                {
                    _db.ShoppingCards.Add(Scard);
                }
                else
                {
                    card.Count += Scard.Count;
                }
                _db.SaveChanges();
                var count=_db.ShoppingCards.Where(i=>i.ApplicationUserId==Scard.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(Others.ssShoppingCard, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var product = _db.Products.FirstOrDefault(i => i.Id == Scard.Id);
                ShoppingCard card = new ShoppingCard()
                {
                    Product = product,
                    ProductId = product.Id

                };
            }
           

            return View(Scard);
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
