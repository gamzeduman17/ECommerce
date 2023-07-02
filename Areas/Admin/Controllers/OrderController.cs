using ECommerce.Data;
using ECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ECommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public OrderDetailsVM OrderVM { get; set; }
        
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpPost]
        [Authorize(Roles =Others.Role_Admin)]
        public IActionResult Approve()
        {
            OrderHeader orderHeader = _db.OrderHeaders.FirstOrDefault(i => i.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = Others.Status_Approved;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [Authorize(Roles = Others.Role_Admin)]
        public IActionResult DeliverToCargo()
        {
            OrderHeader orderHeader = _db.OrderHeaders.FirstOrDefault(i => i.Id == OrderVM.OrderHeader.Id);
            orderHeader.OrderStatus = Others.Status_InCargo;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Details(int id)
        {
            OrderVM=new OrderDetailsVM{
                OrderHeader=_db.OrderHeaders.FirstOrDefault(i=>i.Id==id),
                OrderDetails=_db.OrderDetails.Where(x=>x.OrderId==id).Include(x=>x.Product)

            };
            return View(OrderVM);   
           
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if(User.IsInRole(Others.Role_Admin))
            {
                orderHeaderList = _db.OrderHeaders.ToList();
            }
            else
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.ApplicationUserId == claim.Value).Include(i => i.ApplicationUserId);
            }
            return View(orderHeaderList);
        }
        public IActionResult OnHold()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Others.Role_Admin))
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.OrderStatus == Others.Status_OnHold);
            }
            else
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.ApplicationUserId == claim.Value && i.OrderStatus==Others.Status_OnHold).Include(i => i.ApplicationUserId);
            }
            return View(orderHeaderList);
        }
        public IActionResult Approved()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Others.Role_Admin))
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.OrderStatus == Others.Status_Approved);
            }
            else
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.ApplicationUserId == claim.Value && i.OrderStatus == Others.Status_Approved).Include(i => i.ApplicationUserId);
            }
            return View(orderHeaderList);
        }
        public IActionResult InCargo()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeaderList;
            if (User.IsInRole(Others.Role_Admin))
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.OrderStatus == Others.Status_InCargo);
            }
            else
            {
                orderHeaderList = _db.OrderHeaders.Where(i => i.ApplicationUserId == claim.Value && i.OrderStatus == Others.Status_InCargo).Include(i => i.ApplicationUserId);
            }
            return View(orderHeaderList);
        }
    }
}
