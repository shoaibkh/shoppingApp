using CartShopping.Models.Data;
using CartShopping.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CartShopping.Controllers
{
    public class PagesController : Controller
    {
        // GET: Pages
        public ActionResult Index(string page = "")
        {
            if (page == "")
                page = "home";

            PagesVM model;
            PageDTO dto;

            using(Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }

            using(Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            ViewBag.PageTitle = dto.Title;
            if(dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "Yes";
            }

            else
            {
                ViewBag.Sidebar = "No";
            }

            model = new PagesVM(dto);
            return View(model);
        }


        public ActionResult PagesMenuPartial()
        {
            List<PagesVM> pageVMList;

            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PagesVM(x)).ToList();
            }
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            SidebarVM model;

            using(Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }
            return PartialView(model);
        }
    }

     
}