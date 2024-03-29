using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Engines.ArenaSystem
{
    [PropertyObject]
    public class ArenaTeam
    {
        public Dictionary<PlayerMobile, PlayerStatsEntry> Players { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Count => Players == null ? 0 : Players.Count;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Unoccupied => Count == 0;

        [CommandProperty(AccessLevel.GameMaster)]
        public PlayerMobile PlayerZero { get; set; }

        public ArenaTeam()
        {
            Players = new Dictionary<PlayerMobile, PlayerStatsEntry>();
        }

        public ArenaTeam(PlayerMobile pm)
        {
            Players = new Dictionary<PlayerMobile, PlayerStatsEntry>();
            AddParticipant(pm);
        }

        public void AddParticipant(PlayerMobile pm)
        {
            if (Players.Count == 0)
                PlayerZero = pm;

            Players[pm] = PVPArenaSystem.Instance.GetPlayerEntry<PlayerStatsEntry>(pm);
        }

        public bool RemoveParticipant(PlayerMobile pm)
        {
            return Players != null && Players.Remove(pm);
        }

        public bool Contains(PlayerMobile pm)
        {
            return Players.ContainsKey(pm);
        }

        public ArenaTeam(GenericReader reader)
        {
            int version = reader.ReadInt();

            Players = new Dictionary<PlayerMobile, PlayerStatsEntry>();
            List<PlayerMobile> list = new List<PlayerMobile>();

            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                if (reader.ReadMobile() is PlayerMobile pm)
                {
                    list.Add(pm);
                }
            }

            // have to wait for everything else to deserialize :(
            Timer.DelayCall(() =>
            {
                for (var index = 0; index < list.Count; index++)
                {
                    PlayerMobile pm = list[index];
                    AddParticipant(pm);
                }
            });
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(Players.Count);
            foreach (KeyValuePair<PlayerMobile, PlayerStatsEntry> kvp in Players)
            {
                writer.Write(kvp.Key);
            }
        }
    }
}
