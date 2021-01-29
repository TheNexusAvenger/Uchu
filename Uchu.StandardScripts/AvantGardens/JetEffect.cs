using System.Linq;
using System.Threading.Tasks;
using Uchu.World;
using Uchu.World.Scripting.Native;

namespace Uchu.StandardScripts.AvantGardens
{
    [ZoneSpecific(1100)]
    public class JetEffect : NativeScript
    {
        public override Task LoadAsync()
        {
            foreach (var gameObject in Zone.GameObjects)
            {
                Mount(gameObject);
            }

            Listen(Zone.OnObject,(obj) =>
            {
                if (!(obj is GameObject gameObject)) return;
                Mount(gameObject);
            });
            
            return Task.CompletedTask;
        }

        public void Mount(GameObject gameObject)
        {
            if (gameObject.Lot != 6209) return;
            if (!gameObject.TryGetComponent<QuickBuildComponent>(out var quickBuildComponent)) return;

            Listen(quickBuildComponent.OnStateChange, (state) =>
            {
                if (state != RebuildState.Completed) return;
                
                var jetObject = Zone.GameObjects
                    .FirstOrDefault(possibleJetObject => possibleJetObject.GetGroups()
                        .Any(@group => @group == "Jet_FX"));

                jetObject.Animate("jetFX");
                gameObject.PlayFX("radarDish","create",641);
            });
        }
    }
}