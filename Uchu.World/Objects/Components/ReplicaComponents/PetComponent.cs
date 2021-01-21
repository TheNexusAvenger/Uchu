using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using RakDotNet.IO;
using Uchu.Core;
using Uchu.Core.Client;
using Uchu.World.Client;

namespace Uchu.World
{
    public class PetComponent : ReplicaComponent
    {
        private Player CurrentBuilder = default;

        private List<Brick> Bricks;
        private bool PetWild  => Owner == null;
        public GameObject Owner { get; set; }

        public uint ModerationStatus { get; set; }

        public GameObject PetInteractionObject { get; set; }

        public override ComponentId Id => ComponentId.PetComponent;

        protected PetComponent()
        {
            Bricks = new List<Brick>();

            Listen(OnStart, () =>
            {
                Listen(GameObject.OnInteract, OnInteract);
            });
        }
        
        public override void Construct(BitWriter writer)
        {
            Serialize(writer);
        }

        public override void Serialize(BitWriter writer)
        {
            writer.WriteBit(true);
            writer.Write<uint>(0x4000002);
            writer.Write<uint>(0x00);

            var hasPetInteraction = PetInteractionObject != null;
            writer.WriteBit(hasPetInteraction);
            if (hasPetInteraction) writer.Write(PetInteractionObject);

            var hasOwner = Owner != null;
            writer.WriteBit(hasOwner);
            if (hasOwner) writer.Write(Owner);

            writer.WriteBit(true);
            writer.Write(ModerationStatus);

            writer.Write((byte) GameObject.Name.Length);
            writer.WriteString(GameObject.Name, GameObject.Name.Length, true);

            if (hasOwner)
            {
                writer.Write((byte) Owner.Name.Length);
                writer.WriteString(Owner.Name, Owner.Name.Length, true);
            }
            else
            {
                writer.Write<byte>(0);
            }
        }

        public async Task OnInteract(Player player)
        {
            if (PetWild)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(Zone.UchuServer.Config.ResourcesConfiguration.GameResourceFolder,
                    ClientCache.GetTable<TamingBuildPuzzles>().FirstOrDefault(i => i.NPCLot == GameObject.Lot)
                    .ValidPiecesLXF));

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.Name == "Bricks")
                    {
                        foreach (XmlNode BrickNode in node.ChildNodes)
                        {
                            if (BrickNode.Name == "Brick")
                            {
                                Brick current = new Brick();
                                current.DesignID = UInt32.Parse(BrickNode.Attributes["designID"].Value);
                                foreach (XmlNode PartNode in BrickNode.ChildNodes)
                                {
                                    if (PartNode.Name == "Part")
                                    {
                                        Part CurrentPart = new Part();
                                        CurrentPart.Material = Int32.Parse(PartNode.Attributes["materials"].Value.Split(',')
                                            .ElementAt(0));
                                        current.DesignPart = CurrentPart;
                                    }
                                }
                                Bricks.Add(current);
                            }
                        }

                        break;
                    }
                }
                
                NotifyPetTamingMinigame msg = new NotifyPetTamingMinigame();

                msg.Associate = player;
                
                msg.bForceTeleport = true;
                msg.PlayerTamingID = player.Id;
                msg.PetID = GameObject.Id;
                msg.notifyType = NotifyType.BEGIN;
                
                Vector3 petPos = GameObject.Transform.Position;
                msg.petsDestPos = petPos;
                Vector3 pos = player.Transform.Position;
                double deg = Math.Atan2(petPos.Z - pos.Z, petPos.X - pos.X) * 180 / Math.PI;
                var interaction_distance = GameObject.Settings.ContainsKey("interaction_distance") ? GameObject.Settings["interaction_distance"] : 0.0f;
                pos = new Vector3(
                    petPos.X + (float) interaction_distance * (float)Math.Cos(-deg),
                    petPos.Y,
                    petPos.Z + (float) interaction_distance * (float)Math.Sin(-deg)
                );
                msg.telePos = pos;

                msg.teleRot = pos.QuaternionLookRotation(petPos);
                
                Zone.BroadcastMessage(msg);

                var nmsg = new NotifyPetTamingPuzzleSelectedMessage();
                nmsg.Associate = player;
                nmsg.Bricks = Bricks;
                player.Message(nmsg);

                Listen(player.OnPetTamingTryBuild, OnPetTamingTryBuild);
            }
        }

        public async Task OnPetTamingTryBuild(PetTamingTryBuildMessage msg)
        {
            int CorrectCount = 0;

            foreach (var item in Bricks)
            {
                foreach (Brick item2 in msg.Bricks)
                {
                    if (item.DesignID == item2.DesignID)
                        CorrectCount += 1;
                }
            }
                
            PetTamingTryBuildResultMessage nmsg = new PetTamingTryBuildResultMessage();
            nmsg.Associate = msg.Associate;
            nmsg.bSuccess = !(CorrectCount == Bricks.Count);
            nmsg.iNumCorrect = CorrectCount;
            (msg.Associate as Player).Message(nmsg);
        }
    }
}