using PokemonApp.Models;
using PokemonApp.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PokemonApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private PokemonService pokemonService = new PokemonService();
        public static readonly MemoryCache cache = MemoryCache.Default;
        private const string CACHE_POKEMONS_KEY = "Pokemons";
        private Context db = new Context();
        private users currentUser;

        [OutputCache(Duration = int.MaxValue, VaryByParam = "none")]
        public async Task<ActionResult> Index()
        {
            currentUser = FindUserInCookies();
            var pokemons = cache.Get(CACHE_POKEMONS_KEY) as List<Pokemon>;
            if (pokemons == null)
            {
                await pokemonService.TryGetPokemonsData(151, 0);
                await pokemonService.TryGetPokemonSpecificData(pokemonService.Pokemons);
                pokemons = pokemonService.Pokemons;
                cache.Set(CACHE_POKEMONS_KEY, pokemons, DateTimeOffset.UtcNow.AddHours(24));
            }
            return View(pokemons);
        }

        public ActionResult Favorites()
        {
            currentUser = FindUserInCookies();
            ViewBag.Message = "Your application description page.";

            var favoriteIds = db.favorites.Where(f => f.user_id == currentUser.user_id).Select(f => f.pokemon_id).ToList();
            var totalPokemons = cache.Get(CACHE_POKEMONS_KEY) as List<Pokemon>;
            var favoritePokemons = totalPokemons.Where(p => favoriteIds.Contains(p.Id)).ToList();
            foreach (var pokemon in favoritePokemons)
                pokemon.IsFavorite = true;
            return View(favoritePokemons);
        }

        [HttpPost]
        public async Task<ActionResult> TryAddFavoriteAsync(int pokemonId)
        {
            currentUser = FindUserInCookies();
            var currentCount = await db.favorites.Where(f => f.user_id == currentUser.user_id && f.pokemon_id == pokemonId).CountAsync();

            if (currentCount >= 10)
                return  Json(new { success = false, message = "Cant save the data because you have more than maximum favorites pokemons or it is the same " }); ;

            var favorite = new favorites { user_id = currentUser.user_id, pokemon_id = pokemonId };
            db.favorites.Add(favorite);
            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Data saved successfully!" }); ;
        }

        [HttpPost]
        public async Task<bool> TryRemoveFavoriteAsync(int pokemonId)
        {
            currentUser = FindUserInCookies();
            var currentCount = await db.favorites.Where(f => f.user_id == currentUser.user_id && f.pokemon_id == pokemonId).CountAsync();
            if (currentCount <= 0)
                return false;

            var favorite = await db.favorites.Where(f => f.user_id == currentUser.user_id && f.pokemon_id == pokemonId).FirstOrDefaultAsync();
            db.favorites.Remove(favorite);
            await db.SaveChangesAsync();
            return true;
        }

        private users FindUserInCookies()
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
            {
                Debug.WriteLine("No authentication cookie found.");
                return null;
            }

            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
            if (ticket == null)
            {
                Debug.WriteLine("Failed to decrypt authentication ticket.");
                return null;
            }

            string userIdString = ticket.Name;

            if (!int.TryParse(userIdString, out int userId))
            {
                Debug.WriteLine("Invalid user ID in authentication ticket.");
                return null;
            }

            users currentUser = db.users.Find(userId);
            if (currentUser == null)
                Debug.WriteLine($"User with id {userId} not found in database.");
            else
                Debug.WriteLine($"User with id {userId} found: {currentUser.user_name}");

            return currentUser;
        }
    }
}