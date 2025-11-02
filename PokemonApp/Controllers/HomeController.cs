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
        private const int MAX_FAVORITE_POKEMONS = 10;

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
            var favoriteIds = db.favorites.Where(f => f.user_id == currentUser.user_id).Select(f => f.pokemon_id).ToList();
            var favoritePokemons = pokemons.Where(p => favoriteIds.Contains(p.Id)).ToList();

            foreach(var pokemon in pokemons)
                pokemon.IsFavorite = false;

            foreach (var pokemon in favoritePokemons)
                pokemon.IsFavorite = true;

            ViewBag.CurrentFavorites = $"Favorites ({favoritePokemons.Count}/{MAX_FAVORITE_POKEMONS})";
            return View(pokemons);
        }

        public ActionResult Favorites()
        {
            currentUser = FindUserInCookies();

            var favoriteIds = db.favorites.Where(f => f.user_id == currentUser.user_id).Select(f => f.pokemon_id).ToList();
            var totalPokemons = cache.Get(CACHE_POKEMONS_KEY) as List<Pokemon>;
            var favoritePokemons = totalPokemons.Where(p => favoriteIds.Contains(p.Id)).ToList();
            foreach (var pokemon in favoritePokemons)
                pokemon.IsFavorite = true;

            return View(favoritePokemons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> TryAddFavoriteAsync(int pokemonId)
        {
            currentUser = FindUserInCookies();
            int targetUserId = currentUser.user_id;
            int userCountLimit = db.favorites.Count(u => u.user_id == targetUserId);

            if (userCountLimit >= MAX_FAVORITE_POKEMONS)
                return Json(new { success = false, message = $"You can only have {MAX_FAVORITE_POKEMONS} favorite pokemon!" });

            var favorite = new favorites { user_id = currentUser.user_id, pokemon_id = pokemonId };
            db.favorites.Add(favorite);
            await db.SaveChangesAsync();
            ViewBag.CurrentFavorites = $"Favorites ({db.favorites.Count(u => u.user_id == currentUser.user_id)}/{MAX_FAVORITE_POKEMONS})";
            return Json(new { success = true, message = "Added to favorites!" }); ;
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<ActionResult> TryRemoveFavoriteAsync(int pokemonId)
        {
            currentUser = FindUserInCookies();
            var favorite = await db.favorites.Where(f => f.user_id == currentUser.user_id && f.pokemon_id == pokemonId).FirstOrDefaultAsync();

            if (favorite == null)
                return Json(new { success = false, message = "That favorite doesnt exist in our database!" });

            db.favorites.Remove(favorite);
            await db.SaveChangesAsync();
            ViewBag.CurrentFavorites = $"Favorites ({db.favorites.Count(u => u.user_id == currentUser.user_id)}/{MAX_FAVORITE_POKEMONS})";
            return Json(new { success = true, message = "removed from Favorites!" });
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