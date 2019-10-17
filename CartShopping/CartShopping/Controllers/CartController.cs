using CartShopping.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CartShopping.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if(cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty";
                return View();
            }

            decimal total = 0m;

            foreach(var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandtTotal = total;
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            CartVM model = new CartVM();

            int qty = 0;

            decimal price = 0m;

            if(Session["cart"] != null)
            {
                var list = (List<CartVM>)Session["cart"];

                foreach(var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price; 
                }
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0m;
            }
            return PartialView(model);
        }
    }
}