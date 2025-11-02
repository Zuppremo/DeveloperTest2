using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace PokemonApp.Services
{
    public class PokemonService : IPokemonService
    {
        public event Action<bool> Loading;
        private const string API_BASE_URL = "https://pokeapi.co/api/v2/";

        private HttpClient httpClient;

        public List<Pokemon> Pokemons = new List<Pokemon>();

        public PokemonService()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(API_BASE_URL);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<Pokemon>> TryGetPokemonsData(int limit, int offset)
        {
            HttpResponseMessage response;
            try
            {
                Loading?.Invoke(true);
                response = await httpClient.GetAsync($"{API_BASE_URL}pokemon?limit={limit}&offset={offset}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(JsonConvert.SerializeObject(content));
                    dynamic contentDeserialized = JsonConvert.DeserializeObject(content);
                    for (int i = 0; i < limit; i++)
                    {
                        Pokemon pokemon = new Pokemon();
                        pokemon.Id = i + 1;
                        //Debug.WriteLine((string)contentDeserialized["results"][i]["name"]);
                        pokemon.Name = (string)contentDeserialized["results"][i]["name"];
                        pokemon.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pokemon.Name.ToLower());
                        Pokemons.Add(pokemon);
                    }
                    //Debug.WriteLine(Pokemons.Count);
                    //Debug.WriteLine($"First Pokemon is {Pokemons[0].Name}");
                    //Debug.WriteLine($"Last Pokemon is {Pokemons[Pokemons.Count - 1].Name}");
                }
                Loading?.Invoke(false);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Loading?.Invoke(false);
            }

            return Pokemons;
        }
        /*

        public async Task TryGetPokemonData(Pokemon pokemon)
        {
            HttpResponseMessage response;
            try
            {
                Loading?.Invoke(true);
                response = await httpClient.GetAsync($"{API_BASE_URL}pokemon/{pokemon.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    Debug.WriteLine(JsonConvert.SerializeObject(content));
                    dynamic contentDeserialized = JsonConvert.DeserializeObject(content);
                    JArray types = contentDeserialized["types"];
                    foreach (JObject type in types)
                        pokemon.Types.Add((string)type["type"]["name"]);

                    foreach (var type in pokemon.Types)
                        Debug.WriteLine($"Pokemon Types{type}");

                }
                Loading?.Invoke(false);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Loading?.Invoke(false);
            }
        }

        */

        public async Task TryGetPokemonSpecificData(List<Pokemon> pokemons)
        {
            HttpResponseMessage response;
            try
            {
                Loading?.Invoke(true);

                var tasks = pokemons.Select(async pokemon =>
                {
                    response = await httpClient.GetAsync($"{API_BASE_URL}pokemon/{pokemon.Id}");
                    if (!response.IsSuccessStatusCode) return;
                    var content = await response.Content.ReadAsStringAsync();
                    //Debug.WriteLine(JsonConvert.SerializeObject(content));
                    dynamic contentDeserialized = JsonConvert.DeserializeObject(content);
                    pokemon.ImageUrl = (string)contentDeserialized["sprites"]["front_default"];
                    //Debug.WriteLine($"Pokemon {pokemon.Name} Image Url{pokemon.ImageUrl}");
                    JArray types = contentDeserialized["types"];
                    pokemon.Types.Clear();
                    foreach (JObject type in types)
                        pokemon.Types.Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase((string)type["type"]["name"])); //capitalize
                });

                await Task.WhenAll(tasks);
                //foreach (var type in pokemon.Types)
                // Debug.WriteLine($"Pokemon Name {pokemon.Name} has Pokemon Types {type}");
                Loading?.Invoke(false);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Loading?.Invoke(false);
            }
        }
    }
}