using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{Page}
        public ActionResult Index(string page = "")
        {
            //Get and Set Page Slug 
            if (page == "")
                page = "home";

            //Declare the model and Dto 
            PageVM model;
            PageDTO dto;

            //Check if page exists
            using (Db db = new Db())
            {
                if (! db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index", new { page = "" });
                }
            }
            //Get the Page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }
            //set page title 

            ViewBag.PageTitle = dto.Title;

            //Check for sidebar 
            if (dto.HasSidebar == true)
            {
                ViewBag.Sidebar = "yes";
            }
            else
            {
                ViewBag.Sidebar = "No";
            }
            //init the model
            model = new PageVM(dto);

            //return view with model
            return View(model);

        }

        public ActionResult PagesMenuPartial()
        {
            // Declare a list of PageVMS
            List<PageVM> pageVMList;

            //Get all pages except the HomePage 
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }

            //Return the partial view with list 
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            // Declare the Model 
            SidebarVM model;

            //Init the model 
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }

            //retun the model with the partial view 
            return PartialView(model);

        }

    } 
}