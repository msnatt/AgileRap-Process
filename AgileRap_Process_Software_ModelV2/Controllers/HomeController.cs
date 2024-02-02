using AgileRap_Process_Software_ModelV2.Data;
using AgileRap_Process_Software_ModelV2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AgileRap_Process_Software_ModelV2.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            User user = new User();
            return View(user);
        }
        [HttpPost]
        public ActionResult Login(User user)
        {
            //เช็ค Email และ Password ที่ป้อนเข้ามามีใน database หรือไม่
            if (db.User.Where(b => b.Email == user.Email && b.Password == user.Password).Count() == 1)
            {
                //ดึง User ที่ Email และ Password ที่ตรงกับที่ป้อนเข้ามานำมาเก็บไว้ที่ UserSelected
                var UserSelected = db.User.Where(b => b.Email == user.Email && b.Password == user.Password).First();

                //set static UserID variable
                GlobalVariable.SetUserLogin(UserSelected.ID);
                HttpContext.Session.SetString("Default","Operator");

                //ส่งไปที่หน้า Index ของ Work
                return RedirectToAction("Index", "Works", new {AssignTo = GlobalVariable.GetUserLogin()});
            }
            //************************************************//

            //ถ้าไม่มีใน Database ให้ขึ้นข้อความตาม ViewBagMassageWrong
            ViewBag.MassageWrong = "Email or Password Wrong! Please try again.";
            return View("Index", user);
        }
        [HttpGet]
        public ActionResult Register()
        {
            //new User แล้วส่งไปที่หน้า Register
            return View(new User());
        }
        [HttpPost]
        public ActionResult Register(User user)
        {
            //เช็ค Email เคยมีใน Database แล้วหรือยัง Password ตรงกับ ConfirmPassword หรือไม่
            if (ModelState.IsValid && user.Password == user.ConfirmPassword && db.User.Where(b => b.Email == user.Email).Count() == 0)
            {
                //บันทึกลง Database
                db.User.Add(user);
                db.SaveChanges();
                //กลับไปที่หน้าหลัก (Login)
                return RedirectToAction("Index");
            }
            //กรณีที่ Password ไม่ตรงกับ ConfirmPassword
            else if (user.Password != user.ConfirmPassword)
            {
                ViewBag.MassageWrong = "The Password and Confirm Password do not match. ;(";
                return View(user);
            }
            //กรณีที่ Email ที่ป้อนเข้ามานั้นเคยมีใน Database แล้ว
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
