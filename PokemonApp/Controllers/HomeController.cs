using PokemonApp.Models;
using PokemonApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PokemonApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private PokemonService pokemonService;

        [OutputCache(Duration = int.MaxValue, VaryByParam = "none")]
        public async Task<ActionResult> Index()
        {
            pokemonService = new PokemonService();
            await pokemonService.TryGetPokemonsData(151, 0);
            await pokemonService.TryGetPokemonSpecificData(pokemonService.Pokemons);
            return View(pokemonService.Pokemons);
        }

        public ActionResult Favorites()
        {
            ViewBag.Message = "Your application description page.";
            List<Pokemon> favoritesPokemon = new List<Pokemon>();
            Pokemon firstPokemon = new Pokemon();
            firstPokemon.Id = 0;
            firstPokemon.Name = "Bulbasaur";
            Pokemon secondPokemon = new Pokemon();
            firstPokemon.Id = 1;
            firstPokemon.Name = "Mew";
            Pokemon thirdPokemon = new Pokemon();
            firstPokemon.Id = 2;
            firstPokemon.Name = "Raticate";
            Pokemon fourthPokemon = new Pokemon();
            firstPokemon.Id = 3;
            firstPokemon.Name = "charmander";
            //favoritesPokemon.Add(firstPokemon);
            return View(favoritesPokemon);
        }
    }
}