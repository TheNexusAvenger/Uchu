using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RakDotNet.IO;
using Uchu.Core;
using Uchu.Core.Client;

namespace Uchu.World
{
    public class CharacterComponent : ReplicaComponent
    {
        public CharacterComponent()
        {
            Listen(OnStart, async () =>
            {
                await LoadAsync();
            });
        }

        private async Task LoadAsync()
        {
            if (GameObject is Player player)
            {
                await using var uchuContext = new UchuContext();
                
                var character = await uchuContext.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
                if (character == default)
                    return;

                Currency = character.Currency;
                UniverseScore = character.UniverseScore;
                Level = character.Level;
                BaseHealth = character.BaseHealth;
                BaseImagination = character.BaseImagination;
                
                // Cache all character emotes
                foreach (var unlockedEmote in character.UnlockedEmotes)
                {
                    AddEmote(unlockedEmote.EmoteId);
                }
                
                // Cache all character flags
                foreach (var flag in character.Flags.Where(flag => flag.Id != default))
                {
                    await SetFlag((int)flag.Id, true);
                }

                HairColor = character.HairColor;
                HairStyle = character.HairStyle;
                ShirtColor = character.ShirtColor;
                PantsColor = character.PantsColor;
                EyebrowStyle = character.EyebrowStyle;
                EyeStyle = character.EyeStyle;
                MouthStyle = character.MouthStyle;
                TotalCurrencyCollected = character.TotalCurrencyCollected;
                TotalBricksCollected = character.TotalBricksCollected;
                TotalSmashablesSmashed = character.TotalSmashablesSmashed;
                TotalQuickBuildsCompleted = character.TotalQuickBuildsCompleted;
                TotalEnemiesSmashed = character.TotalEnemiesSmashed;
                TotalRocketsUsed = character.TotalRocketsUsed;
                TotalMissionsCompleted = character.TotalMissionsCompleted;
                TotalPetsTamed = character.TotalPetsTamed;
                TotalImaginationPowerUpsCollected = character.TotalImaginationPowerUpsCollected;
                TotalLifePowerUpsCollected = character.TotalLifePowerUpsCollected;
                TotalArmorPowerUpsCollected = character.TotalArmorPowerUpsCollected;
                TotalDistanceTraveled = character.TotalDistanceTraveled;
                TotalSuicides = character.TotalSuicides;
                TotalDamageTaken = character.TotalDamageTaken;
                TotalDamageHealed = character.TotalDamageHealed;
                TotalArmorRepaired = character.TotalArmorRepaired;
                TotalImaginationRestored = character.TotalImaginationRestored;
                TotalImaginationUsed = character.TotalImaginationUsed;
                TotalDistanceDriven = character.TotalDistanceDriven;
                TotalTimeAirborne = character.TotalTimeAirborne;
                TotalRacingImaginationPowerUpsCollected = character.TotalRacingImaginationPowerUpsCollected;
                TotalRacingImaginationCratesSmashed = character.TotalRacingImaginationCratesSmashed;
                TotalRacecarBoostsActivated = character.TotalRacecarBoostsActivated;
                TotalRacecarWrecks = character.TotalRacecarWrecks;
                TotalRacingSmashablesSmashed = character.TotalRacingSmashablesSmashed;
                TotalRacesFinished = character.TotalRacesFinished;
                TotalFirstPlaceFinishes = character.TotalFirstPlaceFinishes;
                LastActivity = character.LastActivity;
                FreeToPlay = character.FreeToPlay;
                LandingByRocket = character.LandingByRocket;
                Rocket = character.Rocket;
            }
        }
        
        public GameObject VehicleObject { get; set; }
        
        public bool IsPvP { get; set; }

        public bool IsGameMaster { get; set; }

        public byte GameMasterLevel { get; set; }

        public CharacterActivity Activity { get; set; }

        public long GuildId { get; set; } = -1;

        public string GuildName { get; set; }

        public override ComponentId Id => ComponentId.CharacterComponent;
        
        #region traits
        public int BaseImagination { get; set; }
        public int BaseHealth { get; set; }
        public string Rocket { get; private set; }
        public long LastActivity { get; private set; }
        public bool FreeToPlay { get; private set; }
        public bool LandingByRocket { get; private set; }
        public long HairColor { get; private set; }
        public long HairStyle { get; private set; }
        public long ShirtColor { get; private set; }
        public long PantsColor { get; private set; }
        public long EyebrowStyle { get; private set; }
        public long EyeStyle { get; private set; }
        public long MouthStyle { get; private set; }
        public long TotalCurrencyCollected { get; private set; }
        public long TotalBricksCollected { get; private set; }
        public long TotalSmashablesSmashed { get; private set; }
        public long TotalQuickBuildsCompleted { get; private set; }
        public long TotalEnemiesSmashed { get; private set; }
        public long TotalRocketsUsed { get; private set; }
        public long TotalMissionsCompleted { get; private set; }
        public long TotalPetsTamed { get; private set; }
        public long TotalImaginationPowerUpsCollected { get; private set; }
        public long TotalLifePowerUpsCollected { get; private set; }
        public long TotalArmorPowerUpsCollected { get; private set; }
        public long TotalDistanceTraveled { get; private set; }
        public long TotalSuicides { get; private set; }
        public long TotalDamageTaken { get; private set; }
        public long TotalDamageHealed { get; private set; }
        public long TotalArmorRepaired { get; private set; }
        public long TotalImaginationRestored { get; private set; }
        public long TotalImaginationUsed { get; private set; }
        public long TotalDistanceDriven { get; private set; }
        public long TotalTimeAirborne { get; private set; }
        public long TotalRacingImaginationPowerUpsCollected { get; private set; }
        public long TotalRacingImaginationCratesSmashed { get; private set; }
        public long TotalRacecarBoostsActivated { get; private set; }
        public long TotalRacecarWrecks { get; private set; }
        public long TotalRacingSmashablesSmashed { get; private set; }
        public long TotalRacesFinished { get; private set; }
        public long TotalFirstPlaceFinishes { get; private set; }
        #endregion traits
        
        #region emotes
        /// <summary>
        /// The emotes this player has unlocked
        /// </summary>
        private HashSet<int> Emotes { get; }

        /// <summary>
        /// Adds an emote to the player emote inventory
        /// </summary>
        /// <param name="emoteId"></param>
        public void AddEmote(int emoteId)
        {
            if (Emotes.Contains(emoteId))
                return;
            
            Emotes.Add(emoteId);

            if (GameObject is Player player)
            {
                player.Message(new SetEmoteLockStateMessage
                {
                    Associate = player,
                    EmoteId = emoteId,
                    Lock = false
                });
            }
        }
        #endregion emotes
        
        #region flags;
        
        /// <summary>
        /// The flags this player has
        /// </summary>
        private HashSet<int> Flags { get; } = new HashSet<int>();
        
        /// <summary>
        /// Returns the flag value for a flag id
        /// </summary>
        /// <param name="flagId">The flag to find for the player</param>
        /// <returns><c>true</c> or <c>false</c> based on whether the player has the flag or not</returns>
        public bool GetFlag(int flagId) => Flags.Contains(flagId);
        
        /// <summary>
        /// Adds or removes a flag from the player based on the <c>state</c>
        /// </summary>
        /// <param name="flagId">The id of the flag to change</param>
        /// <param name="state"><c>true</c> if the flag should be added, <c>false</c> if the flag should be removed</param>
        public async Task SetFlag(int flagId, bool state)
        {
            if (GameObject is Player player)
            {
                if (state)
                {
                    if (player.TryGetComponent<MissionInventoryComponent>(out var missionInventory))
                        await missionInventory.FlagAsync(flagId);

                    if (!GetFlag(flagId))
                        Flags.Add(flagId);
                }
                else if (GetFlag(flagId))
                {
                    Flags.Remove(flagId);
                }

                player.Message(new NotifyClientFlagChangeMessage
                {
                    Associate = player,
                    Flag = state,
                    FlagId = flagId
                });
            }
        }
        
        #endregion flags
        
        #region currency

        /// <summary>
        /// Negative offset for the SetCurrency message.
        /// </summary>
        /// <remarks>
        /// Used when the client adds currency by itself. E.g, achievements.
        /// </remarks>
        public long HiddenCurrency { get; set; }

        /// <summary>
        /// Internal representation of the currency a player has
        /// </summary>
        private long _currency;
        
        /// <summary>
        /// The currency a player has
        /// </summary>
        public long Currency
        {
            get => _currency;
            set
            {
                _currency = value;

                if (GameObject is Player player)
                {
                    player.Message(new SetCurrencyMessage
                    {
                        Associate = player,
                        Currency = Currency - HiddenCurrency
                    });
                }
            }
        }

        /// <summary>
        /// The amount of currency a player can pickup
        /// </summary>
        public long EntitledCurrency { get; set; }
        
        #endregion currency
        
        #region score

        /// <summary>
        /// Internal representation of a player's LU score
        /// </summary>
        private long _universeScore;
        
        /// <summary>
        /// A player's LU score
        /// </summary>
        public long UniverseScore
        {
            get => _universeScore;
            set
            {
                var oldScore = UniverseScore;
                _universeScore = value;

                if (GameObject is Player player)
                {
                    player.Message(new ModifyLegoScoreMessage
                    {
                        Associate = player,
                        Score = oldScore - UniverseScore
                    });
                }
                
            }
        }
        
        /// <summary>
        /// The level a player is currently at
        /// </summary>
        public long Level { get; private set; }
        
        /// <summary>
        /// Sets the level to the one provided, also checks if it's a valid level in the cd client
        /// </summary>
        /// <param name="level">The level to set</param>
        public async Task SetLevel(long level)
        {
            await using var cdContext = new CdClientContext();
            
            // Make sure the level exists
            var lookup = await cdContext.LevelProgressionLookupTable
                .FirstOrDefaultAsync(l => l.Id == Level);
            if (lookup == default)
                return;

            Level = level;

            if (GameObject is Player player)
            {
                player.Message(new ModifyLegoScoreMessage
                {
                    Associate = player,
                    Score = lookup.RequiredUScore ?? 0 - UniverseScore
                });
            }
        }
        
        #endregion score
        
        #region serialization

        public override void Construct(BitWriter writer)
        {
            WritePart1(writer);
            WritePart2(writer);
            WritePart3(writer);

            writer.WriteBit(false);
            writer.WriteBit(false);
            writer.WriteBit(false);
            writer.WriteBit(false);

            writer.Write((uint) HairColor);
            writer.Write((uint) HairStyle);
            writer.Write<uint>(0);
            writer.Write((uint) ShirtColor);
            writer.Write((uint) PantsColor);
            writer.Write<uint>(0);
            writer.Write<uint>(0);
            writer.Write((uint) EyebrowStyle);
            writer.Write((uint) EyeStyle);
            writer.Write((uint) MouthStyle);
            writer.Write((ulong) Id);
            writer.Write((ulong) LastActivity);
            writer.Write<ulong>(0);
            writer.Write((ulong) UniverseScore);
            writer.WriteBit(FreeToPlay);

            writer.Write((ulong) TotalCurrencyCollected);
            writer.Write((ulong) TotalBricksCollected);
            writer.Write((ulong) TotalSmashablesSmashed);
            writer.Write((ulong) TotalQuickBuildsCompleted);
            writer.Write((ulong) TotalEnemiesSmashed);
            writer.Write((ulong) TotalRocketsUsed);
            writer.Write((ulong) TotalMissionsCompleted);
            writer.Write((ulong) TotalPetsTamed);
            writer.Write((ulong) TotalImaginationPowerUpsCollected);
            writer.Write((ulong) TotalLifePowerUpsCollected);
            writer.Write((ulong) TotalArmorPowerUpsCollected);
            writer.Write((ulong) TotalDistanceTraveled);
            writer.Write((ulong) TotalSuicides);
            writer.Write((ulong) TotalDamageTaken);
            writer.Write((ulong) TotalDamageHealed);
            writer.Write((ulong) TotalArmorRepaired);
            writer.Write((ulong) TotalImaginationRestored);
            writer.Write((ulong) TotalImaginationUsed);
            writer.Write((ulong) TotalDistanceDriven);
            writer.Write((ulong) TotalTimeAirborne);
            writer.Write((ulong) TotalRacingImaginationPowerUpsCollected);
            writer.Write((ulong) TotalRacingImaginationCratesSmashed);
            writer.Write((ulong) TotalRacecarBoostsActivated);
            writer.Write((ulong) TotalRacecarWrecks);
            writer.Write((ulong) TotalRacingSmashablesSmashed);
            writer.Write((ulong) TotalRacesFinished);
            writer.Write((ulong) TotalFirstPlaceFinishes);
            writer.WriteBit(false);

            writer.WriteBit(LandingByRocket);

            if (LandingByRocket)
            {
                var rocketString = Rocket;
                
                writer.Write((ushort) rocketString.Length);
                writer.WriteString(rocketString, rocketString.Length, true);
            }

            WritePart4(writer);
        }

        public override void Serialize(BitWriter writer)
        {
            WritePart1(writer);
            WritePart2(writer);
            WritePart3(writer);
            WritePart4(writer);
        }

        private void WritePart1(BitWriter writer)
        {
            writer.WriteBit(true);

            var inVehicle = VehicleObject != null;

            writer.WriteBit(inVehicle);

            if (inVehicle) writer.Write(VehicleObject);

            writer.Write<byte>(0);
        }

        private void WritePart2(BitWriter writer)
        {
            var player = (Player) GameObject;
            
            var hasLevel = Level != 0;

            writer.WriteBit(hasLevel);

            if (hasLevel) writer.Write((uint) Level);
        }

        private static void WritePart3(BitWriter writer)
        {
            writer.WriteBit(true);
            writer.WriteBit(false);
            writer.WriteBit(true);
        }

        private void WritePart4(BitWriter writer)
        {
            writer.WriteBit(true);

            writer.WriteBit(IsPvP);
            writer.WriteBit(IsGameMaster);
            writer.Write(GameMasterLevel);

            writer.WriteBit(false); // ???
            writer.Write<byte>(0); // ???

            writer.WriteBit(true); // Active Activity?
            writer.Write((uint) Activity);

            var hasGuild = GuildId != -1;

            writer.WriteBit(hasGuild);

            if (!hasGuild) return;

            writer.Write(GuildId);
            writer.Write((byte) GuildName.Length);
            writer.WriteString(GuildName, GuildName.Length, true);
            writer.WriteBit(true); // Guild Owner?
            writer.Write(-1); // Guild Creation date
        }
        
        #endregion serialization
    }
}