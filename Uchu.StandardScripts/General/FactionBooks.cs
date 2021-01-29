using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uchu.World;
using Uchu.World.Scripting.Native;
using Uchu.Core.Resources;
using Uchu.Core;

namespace Uchu.StandardScripts.General
{
    public class FactionBooks : NativeScript
    {
        private Dictionary<Lot, int> factionBookToAchievement = new Dictionary<Lot, int>()
        {
            {130,(int) MissionId.AssemblyEngineer1},
            {9968,(int) MissionId.AssemblyEngineer2},
            {9969,(int) MissionId.AssemblyEngineer},
            {131,(int) MissionId.AssemblySummoner1},
            {9970,(int) MissionId.AssemblySummoner2},
            {9971,(int) MissionId.AssemblySummoner},
            {11925,(int) MissionId.AssemblyInventor1},
            {11929,(int) MissionId.AssemblyInventor2},
            {11934,(int) MissionId.AssemblyInventor},
            {20,(int) MissionId.VentureLeagueBuccaneer1},
            {9935,(int) MissionId.VentureLeagueBuccaneer2},
            {9936,(int) MissionId.VentureLeagueBuccaneer},
            {21,(int) MissionId.VentureLeagueDaredevil1},
            {9976,(int) MissionId.VentureLeagueDaredevil2},
            {9963,(int) MissionId.VentureLeagueDaredevil},
            {11923,(int) MissionId.VentureLeagueAdventurer1},
            {11926,(int) MissionId.VentureLeagueAdventurer2},
            {11931,(int) MissionId.VentureLeagueAdventurer},
            {125,(int) MissionId.ParadoxSpaceMarauder1},
            {9966,(int) MissionId.ParadoxSpaceMarauder2},
            {9967,(int) MissionId.ParadoxSpaceMarauder},
            {118,(int) MissionId.ParadoxSorcerer1},
            {9964,(int) MissionId.ParadoxSorcerer2},
            {9965,(int) MissionId.ParadoxSorcerer},
            {11922,(int) MissionId.ParadoxShinobi1},
            {11927,(int) MissionId.ParadoxShinobi2},
            {11932,(int) MissionId.ParadoxShinobi},
            {89,(int) MissionId.SentinelSamurai1},
            {9974,(int) MissionId.SentinelSamurai2},
            {9975,(int) MissionId.SentinelSamurai},
            {80,(int) MissionId.SentinelKnight1},
            {9972,(int) MissionId.SentinelKnight2},
            {9973,(int) MissionId.SentinelKnight},
            {11924,(int) MissionId.SentinelSpaceRanger1},
            {11928,(int) MissionId.SentinelSpaceRanger2},
            {11933,(int) MissionId.SentinelSpaceRanger},
        };
        
        public override Task LoadAsync()
        {
            Listen(Zone.OnPlayerLoad, player => {
                var itemInventory = player.GetComponent<InventoryManagerComponent>();
                
                Listen(itemInventory.OnLotRemoved,async (itemId,count)  =>
                {
                    if (!factionBookToAchievement.ContainsKey(itemId)) return;

                    var missionId = factionBookToAchievement[itemId];
                    var missionInventory = player.GetComponent<MissionInventoryComponent>();
                    var existingMission = missionInventory.GetMission(missionId);
                    if (existingMission != default && existingMission.Completed) return;
                    await missionInventory.CompleteMissionAsync(missionId);
                });
            });

            return Task.CompletedTask;
        }
    }
}