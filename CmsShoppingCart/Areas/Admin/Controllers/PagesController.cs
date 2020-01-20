using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {

            //Declare list of page models
            List<PageVM> PagesList;

            using (Db db = new Db())
            {

                // Init the list
                PagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();

            }
            // Return view with list
            return View(PagesList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {

            // Check Model State 
            if (! ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {


                // Declare slug

                string slug;
                // Init PageDTO

                PageDTO dto = new PageDTO();

                // DTO Title

                dto.Title = model.Title;
                // Check for and set slug if need be 

                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ","-").ToLower();

                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Make sure Title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError(" ", "That Title or Slug Already Exists.");
                    return View(model);
                }


                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;



                //Save the DTO

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //Set TempDat Message 

            TempData["SM"] = "You have added a new Page";
            //Redirect 

            return RedirectToAction("AddPage");
        }

        // GET: Admin/Pages/AddPage/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // Declare the PageVM

            PageVM model;

            using (Db db = new Db())
            {
                //Get the Page 

                PageDTO dto = db.Pages.Find(id);
                // Confirm that the page exists
                if (dto == null)
                {
                    return Content("The Page does not exist");
                }
                //Init the PageVM
                model = new PageVM(dto);
            }

            //Return View with the Model
            return View(model);
        }


        // POST: Admin/Pages/AddPage/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            // Check model state 
            if (! ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Get Page ID

                int id = model.Id;
                //Init Slug 
                string slug = "home";

                // Get the page 
                PageDTO dto = db.Pages.Find(id);

                //DTO the Title 
                dto.Title = model.Title;

                // Check for slug and set it if need be 
                if (model.Slug != "home")
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

                //Make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }


                //DTO the rest 

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Save the DTO
                db.SaveChanges();

            }

            //Set Tempdata Message 
            TempData["SM"] = "You have Edited the Page";


            //Redirect 
            return RedirectToAction("EditPage");
        }


        // GET: Admin/Pages/AddPage/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            // Declare the PageVM 

            PageVM model;

            using (Db db = new Db())
            {

                //Get the Page 

                PageDTO dto = db.Pages.Find(id);

                //Confirm the Page Exists 
                if (dto==null)
                {
                    return Content("The page does not exist");
                }
                //Init The PageVM
                model = new PageVM(dto);
            }
            //Return Page with model

            return View(model);
        }

        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {

            using (Db db = new Db())
            {
                //Get the Page 

                PageDTO dto = db.Pages.Find(id);

                //Remove the Page 
                db.Pages.Remove(dto);
                //Save 
                db.SaveChanges();
            }
            //Redirect

            return RedirectToAction("Index");
        }


        // POST: Admin/Pages/AddPage/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                // Set Initial count 

                int count = 1;

                // Declare DTO

                PageDTO dto;

                //Set Sorting for each page
                foreach (var pageID in id)
                {
                    dto = db.Pages.Find(pageID);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;

                }

            }
        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            // Declare the model 
            SidebarVM model;

            using (Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);  // we are had coding the 1 
                model = new SidebarVM(dto);
                //Init Model 
            }
            //Return View with model
            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {

            using (Db db = new Db())
            {

                //Get the DTO 
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the Body 

                dto.Body = model.Body;
                //Save 
                db.SaveChanges();

            }
            //Set TempData Message 
            TempData["SM"] = "You have Edited the Sidebar!.";

            //Redirect 
            return RedirectToAction("EditSidebar");
        }

    }
 }