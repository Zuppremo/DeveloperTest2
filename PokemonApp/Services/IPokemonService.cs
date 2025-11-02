using PokemonApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonApp.Services
{
    public interface IPokemonService
    {
        Task<List<Pokemon>> TryGetPokemonsData(int limit, int offset);
        Task TryGetPokemonSpecificData(List<Pokemon> pokemon);
    }
}
