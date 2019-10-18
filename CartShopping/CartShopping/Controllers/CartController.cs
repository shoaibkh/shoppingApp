using CartShopping.Models.Data;
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

        public ActionResult AddToCartPartial(int id)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);

                var ProductIncart = cart.FirstOrDefault(x => x.ProductId == id);

                if (ProductIncart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    ProductIncart.Quantity++;
                }
            }

                int qty = 0;
                decimal price = 0m;

                foreach(var item in cart)
                {
                    qty = item.Quantity;
                    price = item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

                Session["cart"] = cart;
            
            return PartialView(model);
        }
    }
}