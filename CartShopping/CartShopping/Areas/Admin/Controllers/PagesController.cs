using CartShopping.Models.Data;
using CartShopping.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace CartShopping.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            List<PagesVM> pagesList;
            using(Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PagesVM(x)).ToList();
            }
            return View(pagesList);
        }
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddPage(PagesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                string slug;
                PageDTO dto = new PageDTO();
                dto.Title = model.Title;
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug or title already exist");
                    return View(model);
                }
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.Sorting = model.Sorting;
                dto.HasSidebar = model.HasSidebar;

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "You have added a new page!";
            return RedirectToAction("AddPage");
        }
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PagesVM model;
            using(Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);
                if(dto == null)
                {
                    return Content("The page does not exist");
                }
                model = new PagesVM(dto);
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EditPage(PagesVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using(Db db = new Db())
            {
                int id = model.Id;
                string slug ="home";
                PageDTO dto = db.Pages.Find(id);
                dto.Title = model.Title;
                if(model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                if(db.Pages.Where(x=>x.Id !=id).Any(x =>x.Title==model.Title)|| 
                    db.Pages.Where(x=>x.Id !=id).Any(x=>x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title and slug already exist");
                    return View(model);
                }
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.Sorting = model.Sorting;
                dto.HasSidebar = model.HasSidebar;

               
                db.SaveChanges();
            }
            TempData["SM"] = "You have editd the page";
            return RedirectToAction("EditPage");
        }

        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            PagesVM model;
            using(Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);
                if(dto == null)
                {
                    return Content("The page does not exist");
                }
                model = new PagesVM(dto);
            }
            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                PageDTO dto = db.Pages.Find(id);
                db.Pages.Remove(dto);
                db.SaveChanges();
            }
            TempData["SM"] = "Page deleted Successfully";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ReorderPages(int[] id)
        {
            using(Db db = new Db())
            {
                int count = 1;
                PageDTO dto;
                foreach(var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
            return View();
        }

        public ActionResult EditSidebar()
        {
            SidebarVM model;
            using(Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using(Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                dto.Body = model.Body;
                db.SaveChanges();
            }
            TempData["SM"] = "You have edited the sidebar";
            return RedirectToAction("EditSidebar");
        }
    }

}