using System.Threading.Tasks;
using Uchu.World;
using Uchu.World.Scripting.Native;

namespace Uchu.StandardScripts.VentureExplorer
{
    [ZoneSpecific(1001)]
    public class ComputerConsole : NativeScript
    {
        public override Task LoadAsync()
        {
            foreach (var gameObject in Zone.GameObjects)
            {
                // TODO: Add hiding of console parts.
                // Return the venture explorer is single player.
                // Consoles must be hidden if mission 1200 and mission 1225 aren't active.
                // Consoles must also be hidden depending on if the user has used them before
                // as re-using the same console defeats the point of using all the computers.
                if (gameObject.Lot != 12551) continue;
                if (!gameObject.Settings.TryGetValue("num", out var number)) continue;
                
                Listen(gameObject.OnInteract, async player =>
                {
                    await player.GetComponent<InventoryManagerComponent>().AddItemAsync(12547, 1);
                });
            }

            return Task.CompletedTask;
        }
    }
}