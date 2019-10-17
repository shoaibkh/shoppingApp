using CartShopping.Models.Data;
using CartShopping.Models.ViewModels;
using CartShopping.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CartShopping.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        [HttpGet]
        public ActionResult Categories()
        {
            List<CategoryVM> CategoryVMList;
            using(Db db = new Db())
            {
                CategoryVMList = db.Categories.
                                 ToArray()
                                 .OrderBy(x => x.Sorting)
                                 .Select(x => new CategoryVM(x))
                                 .ToList();
            }
            return View(CategoryVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;
            using (Db db = new Db())
            {
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                CategoryDTO dto = new CategoryDTO();

                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 200;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();


            }
            return id;
        }

        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;
                CategoryDTO dto;
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
            return View();
        }

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                CategoryDTO dto = db.Categories.Find(id);
                db.Categories.Remove(dto);
                db.SaveChanges();
            }
            TempData["SM"] = "Category deleted Successfully";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using(Db db = new Db())
            {
                if(db.Categories.Any(x=>x.Name == newCatName)) 
                   return "titletaken";
                CategoryDTO dto = db.Categories.Find(id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();
            }
            return "ok";
        }

        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            return View(model);
        }

        //Post: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using(Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }
            using(Db db = new Db())
            {
                if(db.Products.Any(x=>x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            int id;
            using(Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Slug;
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            TempData["SM"] = "You have added a product";

            #region Upload Image
            // Create necessary directory
            var orignalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(orignalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(orignalDirectory.ToString(), "Products\\"+id.ToString());
            var pathString3 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString()+"\\Thumbs");
            var pathString4 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //Check if a file was uploaded
            if(file != null && file.ContentLength > 0)
            {
                //Get file extension
                string ext = file.ContentType.ToLower();

                if(ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using(Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The Image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                //Init image name
                string imageName = file.FileName;

                //save image name to dto

                using(Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges(); 
                }

                //Set orignal and thumb image path

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save orignal
                file.SaveAs(path);

                //create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }
            #endregion

            return RedirectToAction("AddProduct");
        }

        //Get Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            // Declear list of product
            List<ProductVM> listOfProductVM;

            //Set page number
            var pageNumber = page ?? 1;

            using(Db db = new Db())
            {
                //init the list
                listOfProductVM = db.Products.ToArray().Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                  .Select(x => new ProductVM(x)).ToList();

                //populate category list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            //set Pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //return view with list
            return View(listOfProductVM);
        }

        //Get Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductVM model;
            using(Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                if(dto == null)
                {
                    return Content("that product does not exist");
                }
                model = new ProductVM(dto);
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                      .Select(fn => Path.GetFileName(fn));
            }
            return View(model);
        }

        //Post Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            int id = model.Id;
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                      .Select(fn => Path.GetFileName(fn));
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }
            }

            using(Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ","-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();

            }

            TempData["SM"] = "You have edited the product";

            #region Image Upload

            if(file != null && file.ContentLength > 0)
            {

                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    { 
                        ModelState.AddModelError("", "The Image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                //Set upload directory path
                var orignalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                 
                var pathString1 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Delete file from directory

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                // save image name

                string imageName = file.FileName;
                //Save img name to dto
                using(Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //save orignal and thumb image

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                //save orignal
                file.SaveAs(path);

                //create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("EditProduct");
        }


        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            using(Db db = new Db())
            {

                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            //Delete product folder

            var orignalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);
            //redirect
            return RedirectToAction("Products");
        }


        //Post Admin/Shop/SaveGalleryImages
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            foreach (string fileName in Request.Files) {
                HttpPostedFileBase file = Request.Files[fileName];

                if(file != null && file.ContentLength > 0)
                {
                    var orignalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(orignalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);


                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
           
        }


        //Post Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2); 
        }
    }
}