using System.Collections.Generic;

namespace PokemonApp.Models
{
    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Types { get; set; } = new List<string>();
        public bool IsFavorite { get; set; }
    }
}