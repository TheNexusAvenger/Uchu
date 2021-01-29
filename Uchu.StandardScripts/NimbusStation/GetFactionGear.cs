using System.Collections.Generic;
using System.Threading.Tasks;
using Uchu.World;
using Uchu.World.Scripting.Native;
using Uchu.Core.Resources;
using Uchu.Core;

namespace Uchu.StandardScripts.NimbusStation
{
    [ZoneSpecific(1200)]
    public class GetFactionGear : NativeScript
    {
        private Dictionary<int,List<int>> missionsToIds = new Dictionary<int,List<int>>()
        {
            {(int) MissionId.GetyourAssemblyGear,new List<int>() {7591,8033,8031}}, // Engineer, Summoner, Inventory
            {(int) MissionId.GetyourVentureLeagueGear,new List<int>() {7586,8032,8034}}, // Buccaneer, Daredevil, Adventurer
            {(int) MissionId.GetyourParadoxGear,new List<int>() {8029,7589,8030}}, // Space Marauder, Sorcerer, Shinobi
            {(int) MissionId.GetyourSentinelFactionGear,new List<int>() {8028,7590,8027}}, // Samurai, Knight, Space Ranger
        };

        public override Task LoadAsync()
        {
            Listen(Zone.OnPlayerLoad,player => {
                Listen(player.OnRespondToMission, async (missionId,playerObject,rewardItem)  => {
                    if (!missionsToIds.ContainsKey(missionId)) return;
                    if (!missionsToIds[missionId].Contains(rewardItem.Id)) return;

                    await using var uchuContext = new UchuContext();
                    var missionInventory = player.GetComponent<MissionInventoryComponent>();
                    await missionInventory.GetMission(missionId).CompleteAsync(uchuContext,rewardItem);
                });
            });

            return Task.CompletedTask;
        }
    }
}