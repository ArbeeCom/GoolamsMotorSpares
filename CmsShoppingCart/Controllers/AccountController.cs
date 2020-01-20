using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{
  
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: Account/login
        [HttpGet]
        public ActionResult Login()
        {
            //Confirm User is not logged in 
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                return RedirectToAction("user-profile");
            }
            //Return view 
            return View();
        }

        // POST: Account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // check model state 
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Check if the user is valid 

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }
            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid Username or Password!");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }
        }

        // GET: /Account/createAccount
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }


        // POST: /Account/createAccount
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Check model state 
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //Check if passwords match 
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match!.");
                return View("CreateAccount", model);

            }

            using (Db db = new Db())
            {
                //Make sure username is unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "UserName" + model.Username + "Is Already Taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //Create UserDTO 
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                //Add the Dto 
                db.Users.Add(userDTO);

                //Save 
                db.SaveChanges();

                //ADD to UserRolesDTO 
                int id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();

            }
            //Set tempdata Message 
            TempData["SM"] = "You Have Created an account and can now Login.";

            //Redirect

            return Redirect("~/account/login");

        }

        // GET: Account/logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return Redirect("~/Account/Login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Get the Username 
            string username = User.Identity.Name;

            //Declare  the model 
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //Get the user 
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //build the model
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };

            }
            //Return the partial view with Model

            return PartialView(model);
        }

        //GET: Account/User-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            //Get The username 
            string username = User.Identity.Name;

            //Declare the model 
            UserProfileVM model;

            using (Db db = new Db())
            {
                //Get the user 
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //Build the model 
                model = new UserProfileVM(dto);
            }
            //Return the view with model

            return View("UserProfile", model);
        }

        //POST: Account/User-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // Check model state 
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }
            //Check if passwords match 
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords Do not Match.");
                    return View("UserProfile", model);
                }
            }
            using (Db db = new Db())
            {
                //get the username 
                string username = User.Identity.Name;

                //Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username" + model.Username + "Already Exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //Edit DTO 
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //Save 
                db.SaveChanges();
            }
            //Set Tempdata Message 
            TempData["SM"] = "You have edited your profile!";
            //Redirect 

            return Redirect("~/account/user-profile");

        }

        // GET: /account/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            // Init list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                // Get user id
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init products dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Loop though list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to products dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to OrdersForUserVM list
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }

            }

            // Return view with list of OrdersForUserVM
            return View(ordersForUser);
        }

    }
}