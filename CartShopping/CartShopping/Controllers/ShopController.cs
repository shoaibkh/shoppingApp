using CartShopping.Models.Data;
using CartShopping.Models.ViewModels;
using CartShopping.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CartShopping.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {

            return RedirectToAction("Index","Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            List<CategoryVM> categoryVMList;

            using(Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }
            return PartialView(categoryVMList);
        }

        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;

            using(Db db = new Db())
            {
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                ViewBag.CategoryName = productCat.CategoryName;

            }
            return View(productVMList);
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails( string name)
        {
            ProductVM model;
            ProductDTO dto;

            int id = 0;

            using(Db db = new Db())
            {
                if (!db.Products.Any(x => x.Slug == name))
                {
                    return RedirectToAction("Index", "Shop");
                }

                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                id = dto.Id;

                model = new ProductVM(dto);
            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                  .Select(fn => Path.GetFileName(fn));
            return View("ProductDetails",model);
        }
    }
}