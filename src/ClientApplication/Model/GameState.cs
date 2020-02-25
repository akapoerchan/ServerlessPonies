using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amolenk.ServerlessPonies.ClientApplication.Model
{
    public class GameState
    {
        public string GameName { get; set; }

        public bool IsStarted { get; set; }

        public string SelectedEnclosureName { get; set; }

        public string SelectedAnimalName { get; set; }

        public List<Player> Players { get; set; }

        public List<Animal> Animals { get; set; }

        public List<Enclosure> Enclosures { get; set; }
        
        public event EventHandler<Animal> AnimalStateChanged;

        public event EventHandler<Animal> AnimalPurchaseFailed;

        public Animal SelectedAnimal()
            => Animals.Find(animal => animal.Name == SelectedAnimalName);

        public Player FindPlayer(string name)
            => Players.Find(player => player.Name == name);

        public Animal FindAnimal(string name)
            => Animals.Find(animal => animal.Name == name);

        // public void UpdateAnimalState(string name, Action<Animal> update)
        // {
        //     var animal = Animals.Find(animal => animal.Name == name);
        //     if (animal != null)
        //     {
        //         update(animal);
        //     }

        //     AnimalStateChanged?.Invoke(this, animal);
        // }
    }
}