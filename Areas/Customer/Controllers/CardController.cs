using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Iyzipay.Model;
using Microsoft.IdentityModel.Tokens;
using Iyzipay.Request;
using System.Collections.Generic;
using Iyzipay;

namespace ECommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        [BindProperty]
        public ShoppingCardVM ShoppingCardVM { get; set; }


        public CardController(ApplicationDbContext db,IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _emailSender = emailSender;
            _userManager = userManager;
            
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCardVM = new ShoppingCardVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCard = _db.ShoppingCards.Where(i => i.ApplicationUserId == claim.Value).Include(i => i.Product)
            };
            foreach(var item in ShoppingCardVM.ListCard)
            {
                item.Price = item.Product.Price;
                ShoppingCardVM.OrderHeader.OrderTotal += (item.Count * item.Price);
            }
            return View(ShoppingCardVM);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(ShoppingCardVM model)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCardVM.ListCard=_db.ShoppingCards.Where(i=>i.ApplicationUserId==claim.Value).Include(i => i.Product);
            ShoppingCardVM.OrderHeader.OrderStatus = Others.Status_OnHold;
            ShoppingCardVM.OrderHeader.ApplicationUserId = claim.Value;
            ShoppingCardVM.OrderHeader.OrderDate = DateTime.Now;
            _db.OrderHeaders.Add(ShoppingCardVM.OrderHeader);
            _db.SaveChanges();
            foreach(var item in ShoppingCardVM.ListCard)
            {
                item.Price = item.Product.Price;
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId=item.ProductId,
                    OrderId=ShoppingCardVM.OrderHeader.Id,
                    Price=item.Price,
                    Count=item.Count

                };
                ShoppingCardVM.OrderHeader.OrderTotal += item.Count * item.Product.Price;
                model.OrderHeader.OrderTotal += item.Count * item.Product.Price;
                _db.OrderDetails.Add(orderDetails);
            }
            var payment = PaymentProcess(model);
            _db.ShoppingCards.RemoveRange(ShoppingCardVM.ListCard);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(Others.ssShoppingCard, 0);
            return RedirectToAction("Order Complete");
        }
        private Payment PaymentProcess(ShoppingCardVM model)
        {
            Options options = new Options();
            options.ApiKey = "sandbox-DUaiKxvHr5z9CraxfODLbNSrbDs9ENJt";
            options.SecretKey = "sandbox-mz0R4qecon34YT6yIJw7K4V4ETxNYwmh";
            options.BaseUrl = "https://sandbox-api.iyzipay.com";

            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = new Random().Next(1111,9999).ToString();
            request.Price = model.OrderHeader.OrderTotal.ToString();
            request.PaidPrice = model.OrderHeader.OrderTotal.ToString();
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B67832";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

            //PaymentCard paymentCard = new PaymentCard();
            //paymentCard.CardHolderName = "John Doe";
            //paymentCard.CardNumber = "5528790000000008";
            //paymentCard.ExpireMonth = "12";
            //paymentCard.ExpireYear = "2030";
            //paymentCard.Cvc = "123";
            //paymentCard.RegisterCard = 0;
            //request.PaymentCard = paymentCard;
            PaymentCard paymentCard = new PaymentCard();
            paymentCard.CardHolderName = model.OrderHeader.Name;
            paymentCard.CardNumber = model.OrderHeader.CardNumber;
            paymentCard.ExpireMonth = model.OrderHeader.ExpirationMonth;
            paymentCard.ExpireYear = model.OrderHeader.ExpirationYear;
            paymentCard.Cvc = model.OrderHeader.Cvc;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = model.OrderHeader.Id.ToString();
            buyer.Name = model.OrderHeader.Name;
            buyer.Surname = model.OrderHeader.Surname;
            buyer.GsmNumber = model.OrderHeader.PhoneNumber;
            buyer.Email = "g@gmail.com";
            buyer.IdentityNumber = "74300864791";
            buyer.LastLoginDate = "2015-10-05 12:43:35";
            buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = model.OrderHeader.Address;
            buyer.Ip = "85.34.78.112";
            buyer.City = model.OrderHeader.City;
            buyer.Country = "Turkey";
            buyer.ZipCode = model.OrderHeader.PostalCode ;
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Jane Doe";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Jane Doe";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            foreach(var item in _db.ShoppingCards.Where(i=>i.ApplicationUserId==claim.Value).Include(i=>i.Product))
            {
                basketItems.Add(new BasketItem()
                {
                    Id=item.Id.ToString(),
                    Name=item.Product.Title,
                    Category1=item.Product.CategoryId.ToString(),
                    ItemType=BasketItemType.PHYSICAL.ToString(),
                    Price=(item.Price*item.Count).ToString()

                });
            }
            return Payment.Create(request, options);
        }
        public IActionResult OrderComplete()
        {
            return View();
        }
        public IActionResult Index()
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCardVM = new ShoppingCardVM()
            {
                OrderHeader = new Models.OrderHeader(),
                ListCard = _db.ShoppingCards.Where(i => i.ApplicationUserId == claim.Value).Include(i => i.Product)
        };
            ShoppingCardVM.OrderHeader.OrderTotal = 0;
            ShoppingCardVM.OrderHeader.ApplicationUser = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
            foreach(var item in ShoppingCardVM.ListCard)
            {
                ShoppingCardVM.OrderHeader.OrderTotal += (item.Count * item.Product.Price);
            }
            return View(ShoppingCardVM);
        }
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _db.ApplicationUsers.FirstOrDefault(i => i.Id == claim.Value);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification eMail is Empty..");
            }
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            ModelState.AddModelError(string.Empty, "Send eMail Verification Code..");
            return RedirectToAction("Success");
        }
        public IActionResult Success()
        {
            return View();
        }
        public IActionResult Add(int cardId)
        {
            var card = _db.ShoppingCards.FirstOrDefault(i => i.Id == cardId);
            card.Count += 1;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Decrease(int cardId)
        {
            var card = _db.ShoppingCards.FirstOrDefault(i => i.Id == cardId);
            if(card.Count==1)
            {
                var count = _db.ShoppingCards.Where(u => u.ApplicationUserId == card.ApplicationUserId).ToList().Count();
                _db.ShoppingCards.Remove(card);
                _db.SaveChanges();
                HttpContext.Session.SetInt32(Others.ssShoppingCard, count - 1);

            }
            else
            {
                card.Count -= 1;
                _db.SaveChanges();
            }
           
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cardId)
        {
            var card = _db.ShoppingCards.FirstOrDefault(i => i.Id == cardId);
            var count = _db.ShoppingCards.Where(u => u.ApplicationUserId == card.ApplicationUserId).ToList().Count();
            _db.ShoppingCards.Remove(card);
            _db.SaveChanges();
            HttpContext.Session.SetInt32(Others.ssShoppingCard, count - 1);
            return RedirectToAction(nameof(Index));
        }
    }
}
