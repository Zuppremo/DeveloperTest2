using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Web.Security;
using PokemonApp.Models;
using System.Security.Cryptography;

namespace PokemonApp.Controllers
{
    public class AccountController : Controller
    {
        private Context db = new Context();

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string user_email, string user_password, string returnUrl)
        {
            db.Database.Connection.OpenAsync();
            if (!string.IsNullOrEmpty(user_email) && !string.IsNullOrEmpty(user_password))
            {
                try
                {
                    db.Database.Connection.OpenAsync();
                    users user = db.users.Where(x => x.user_email == user_email).FirstOrDefault();

                    if (user != null)
                    {
                        string md5_password = ComputeMD5(user_password);

                        if (user.user_password == md5_password)
                        {
                            if (user.user_status == true)
                            {
                                user.user_last_login = DateTime.Now;
                                db.Entry(user).State = EntityState.Modified;
                                db.SaveChanges();

                                FormsAuthentication.SetAuthCookie(user_email, createPersistentCookie: true);
                                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\") && returnUrl != "/Account/SignOut")
                                {
                                    return Redirect(returnUrl);
                                }
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                ModelState.AddModelError("", "The user is not active.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid User/Password.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The user does not exist.");
                    }
                }
                catch (Exception)
                {
                    db.Database.Connection.Close();
                    throw;
                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields.");
            }
            db.Database.Connection.Close();
            return View();
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.RedirectToLoginPage();            
        }


        // GET: Account
        [Authorize]
        public ActionResult Index()
        {
            return View(db.users.ToList());
        }

        

        [AllowAnonymous]
        // GET: Account/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Account/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Create([Bind(Include = "user_id,user_name,user_email,user_password,user_created_at,user_last_login,user_status")] users users)
        {
            if (ModelState.IsValid)
            {
                users.user_created_at = DateTime.Now;
                users.user_status = true;
                users.user_password = ComputeMD5(users.user_password);

                db.users.Add(users);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(users);
        }

      
        public static string ComputeMD5(string plainText)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // convertir a hex
                var sb = new StringBuilder();
                foreach (byte b in hashBytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
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
