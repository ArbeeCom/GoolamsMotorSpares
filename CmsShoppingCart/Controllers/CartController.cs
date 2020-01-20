using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Init the Cart List 
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Check if cart is empty 
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is Empty.";
                return View();
            }
            //Calculate total and save to viewbag

            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //Return view with List

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Init CartVM
            CartVM model = new CartVM();
            //Init Quantity
            int qty = 0;

            //Init Price 

            decimal price = 0m;

            //Check for Cart Sessiion 

            if (Session["cart"] != null)
            {
                //Get price and Qty 
                var list =(List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                //Or set Qty and price to 0 
                model.Quantity = 0;
                model.Price = 0m;
            }

            //Return PartialView with model

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)

        {
            //Init cartVM List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Init CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //Get the Product 
                ProductDTO product = db.Products.Find(id);

                //Check if already in cart 
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //If not add new 
                if (productInCart == null)
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
                    //If is  we need to increment
                    productInCart.Quantity++;
                }
            }
            //Get total qty and price and add to model
             int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Save the cart back to session 

            Session["cart"] = cart;

            //Return Partial View with model

            return PartialView(model);
        }

        //Get: /Cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Init Cart List 
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get CartVM from list 
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //Increment qty
                model.Quantity++;
                //Store needed data 

                var result = new {qty = model.Quantity, price = model.Price };
                //Return Json With data 
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult DecrementProduct(int productId)
        {
            //Init cart 
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get model from list 
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Decrement Qty 
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Store needed data 
                var result = new { qty = model.Quantity, price = model.Price };

                //Return Json
                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        //Get: /Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            //Init Cart List 
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get model from List 
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Remove Model from List
                cart.Remove(model);
            }
        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        //POST: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Get cart list 
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Get Username
            string username = User.Identity.Name;

            int orderId = 0;
        

            using (Db db = new Db())
            {
                //Init OrderDTO 
                OrderDTO orderDTO = new OrderDTO();
                
                //Get UserId 
                var q = db.Users.FirstOrDefault(x => x.Username == username);
                int userId = q.Id;

                //Add to orderDTO and Save 
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                //Get Inserted Id 
                orderId = orderDTO.OrderId;

                //Init the orderdetailsDto 
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //Add to orderDetails DTO 
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }

            }
            //Email Admin

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("735143389aef8e", "7e431b516673f1"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "arbee8138@gmail.com", "New Order", "You have a new Order!. Order Number = " + orderId);

            //Reset Session
            Session["cart"] = null;

        }

    }
}