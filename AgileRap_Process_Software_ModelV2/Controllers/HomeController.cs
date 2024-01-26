using AgileRap_Process_Software_ModelV2.Data;
using AgileRap_Process_Software_ModelV2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AgileRap_Process_Software_ModelV2.Controllers
{
    public class HomeController : Controller
    {
        private AgileRap_Process_Software_Context db = new AgileRap_Process_Software_Context();
        [HttpGet]
        public ActionResult Index()
        {
            User user = new User();
            return View(user);
        }
        [HttpPost]
        public ActionResult Login(User user)
        {

            if (db.User.Where(b => b.Email == user.Email && b.Password == user.Password).Count() == 1)
            {
                var UserSelected = db.User.Where(b => b.Email == user.Email && b.Password == user.Password).First();

                HttpContext.Session.SetInt32("UserID", UserSelected.ID);

                return RedirectToAction("Index", "Works");
            }
            ViewBag.MassageWrong = "Email or Password Wrong! Please try again.";
            return View("Index", user);
        }
        [HttpGet]
        public ActionResult Register()
        {
            User user = new User();
            return View(user);
        }
        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid && user.Password == user.ConfirmPassword && db.User.Where(b => b.Email == user.Email).Count() == 0)
            {
                db.User.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else if (user.Password != user.ConfirmPassword)
            {
                ViewBag.MassageWrong = "The Password and Confirm Password do not match. ;(";
                return View(user);
            }
            else if (db.User.Where(b => b.Email == user.Email).Count() > 0)
            {
                ViewBag.MassageWrong = "This Email already. ;)";
                return View(user);
            }
            else
            {
                return View(user);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
