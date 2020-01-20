using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //Declare the categoryVM List
            List<CategoryVM> categoryVMList;

            // Init the List
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }


            //Return Partial view with the list
            return PartialView(categoryVMList);
        }

        // GET: Shop/Category/name

        public ActionResult Category(string name)
        {
            //Declare a list of productVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //Get Category Id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //Init the List 
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //Get Category Name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;

            }
            //Return the view with list
            return View(productVMList);
        }

        // GET: Shop/Product-Details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Declare the ProductVM and DTO 
            ProductVM model;
            ProductDTO dto;

            //Init the id
            int id = 0;

            using (Db db = new Db())
            {
                //Check if product exists
                if (! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                //init the dto
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Get ID 
                id = dto.Id;

                //init model 
                model = new ProductVM(dto);

            }
            //Get gallery images 
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/"+ id +"/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));


            //Return view with model
            return View("ProductDetails", model);
        }

    }
}