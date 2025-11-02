using PokemonApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PokemonApp.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private Context db = new Context();

        public ActionResult Index()
        {
            return View();
        }
    }
}