using CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Declare a list of models from the table 
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {

                // Init the List 
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }

            //Return view with list
            return View(categoryVMList);
        }


        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare the id 
            string id;

            using (Db db = new Db())
            {

                // check that the category name is unique

                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //Init the DTO 

                CategoryDTO dto = new CategoryDTO();

                //Add to DTO

                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;
                //Save Dto

                db.Categories.Add(dto);
                db.SaveChanges();

                //Get the ID 
                id = dto.Id.ToString();
            }
            //Return the ID 
            return id;
        }

        // POST: Admin/Shop/AddCategory/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // Set Initial count 

                int count = 1;

                // Declare DTO

                CategoryDTO dto;

                //Set Sorting for each Category
                foreach (var catID in id)
                {
                    dto = db.Categories.Find(catID);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }

            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {

            using (Db db = new Db())
            {
                //Get the Page 

                CategoryDTO dto = db.Categories.Find(id);

                //Remove the Category 
                db.Categories.Remove(dto);
                //Save 
                db.SaveChanges();
            }
            //Redirect

            return RedirectToAction("Categories");
        }


        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName,int id)
        {
            using (Db db = new Db())
            {
                //Check category name is unique 

                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                // Get DTO 
                CategoryDTO dto = db.Categories.Find(id);
                //Edit DTO 
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Save 
                db.SaveChanges();
            }
            //Return 

            return "ok";
        }


        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {

            // Init the model 
            ProductVM model = new ProductVM();

            // Add select list of categories to model 
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }
            // Return view with model 
            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model,HttpPostedFileBase file)
        {
            // check model state 
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");
                    return View(model);
                }
            }
            // Make sure product name is unique

            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");
                    ModelState.AddModelError("", "That Product name has already been taken!");
                    return View(model);
                };
            }

            //Declare prodduct id
            int id;

            //init and save productDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                //product.ImageName = model.ImageName;  // i added this 

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //Get the ID 

                id = product.Id;
        
            }

            //Set TempData message 

            TempData["SM"] = "You Have Added a Product!";

            //

            //#region Upload Image 

            //Create Necessary Directories 
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));

            var pathstring1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathstring2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathstring3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathstring4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathstring5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathstring1))
            {
                Directory.CreateDirectory(pathstring1);
            }

            if (!Directory.Exists(pathstring2))
            {
                Directory.CreateDirectory(pathstring2);
            }

            if (!Directory.Exists(pathstring3))
            {
                Directory.CreateDirectory(pathstring3);
            }

            if (!Directory.Exists(pathstring4))
            {
                Directory.CreateDirectory(pathstring4);
            }

            if (!Directory.Exists(pathstring5))
            {
                Directory.CreateDirectory(pathstring5);
            }

               //Check if a file was uploaded 

            if (file != null && file.ContentLength > 0 )
            {
                //Get File Extension 
                string ext = file.ContentType.ToLower();

                //Verify Extension 
                if (ext != "image/jpg" && 
                    ext != "image/jpeg" && 
                    ext != "image/pjpeg" && 
                    ext != "image/gif" && 
                    ext != "image/x-png" && 
                    ext != "image/png" )
                {
                    using (Db db = new Db())
                    {
                     model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                     ModelState.AddModelError("", "The Image was not uploaded- Wrong Image extension!");
                     return View(model);
                    }
                }

                // Init image name 
                string imageName = file.FileName;

                // Save Image name to dto 
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Set original and thumb image paths 
                var path = string.Format("{0}\\{1}", pathstring2, imageName);
                var path2 = string.Format("{0}\\{1}", pathstring3, imageName);
                // Save original 
                file.SaveAs(path);

                //Create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            //#endregion

            //Redirect
            return RedirectToAction("AddProduct");
        }


        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult Products(int? page, int? catId)

        {
            // Declare a list of productVM
            List<ProductVM> ListOfProductVm;

            //Set a page number 
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Init the list 
                ListOfProductVm = db.Products.ToArray()
                                  .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                  .Select(x => new ProductVM(x))
                                  .ToList();

                //populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Set selected category 
                ViewBag.SelectedCat = catId.ToString();

            }
            // set pagination 
            var onePageOfProducts = ListOfProductVm.ToPagedList(pageNumber, 3); // change that number for how many pages 
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //return view with list
            return View(ListOfProductVm);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Declare the productVM 
            ProductVM model;

            using (Db db = new Db())
            {
                //get the Product 

                ProductDTO dto = db.Products.Find(id);

                //Make sure product exists
                if (dto == null)
                {
                    return Content("That product does not exist!.");
                }
                //Init Model 
                model = new ProductVM(dto);

                //Make a select list 
                model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");

                //Get all Gallery Images 
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn =>Path.GetFileName(fn));
            }
            //Return view with model
            return View(model);
        }

        // Post: Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Get product id 
            int id = model.Id;

            //populate categories and select list 

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            //check model state 
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That name has already been taken!.");
                    return View(model);
                }
            }
            //update product 
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //set tempdata message 

            TempData["SM"] = "You have edited the product!.";

            #region upload image 

            // check for file upload 
            if (file != null && file.ContentLength > 0)
            {
                //Get extension 
                string ext = file.ContentType.ToLower();
                //verify extension 
                if (ext != "image/jpg" &&
                   ext != "image/jpeg" &&
                   ext != "image/pjpeg" &&
                   ext != "image/gif" &&
                   ext != "image/x-png" &&
                   ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The Image was not uploaded- Wrong Image extension!");
                        return View(model);
                    }
                }
                //Set upload directory paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathstring1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathstring2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // delete paths from directories 
                DirectoryInfo di1 = new DirectoryInfo(pathstring1);
                DirectoryInfo di2 = new DirectoryInfo(pathstring2);

                foreach (FileInfo file2 in di1.GetFiles())
                file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                //Save Image name 
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //save original and thumb images

                var path = string.Format("{0}\\{1}", pathstring1, imageName);
                var path2 = string.Format("{0}\\{1}", pathstring2, imageName);
               
                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            // redirect

            return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            //Delete from the DB 
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            //Delete the folder 
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            Directory.Delete(pathString, true);

            //Redirect

            return RedirectToAction("Products");

        }

        // Post: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //Loop through the files

            foreach (string fileName in Request.Files)
            {
                //Init the files 
                HttpPostedFileBase file = Request.Files[fileName];

                //check its not null 
                if (file != null && file.ContentLength > 0 )
                {
                    // set directory paths 
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(),"Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(),"Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // set image paths 
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //save original and thumb
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }
            }
        }

        // Post: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);

        }



        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            // Init list of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init product dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(X => X.OrderId == order.OrderId).ToList();

                    // Get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;

                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to ordersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            // Return view with OrdersForAdminVM list
            return View(ordersForAdmin);
        }

    }
}