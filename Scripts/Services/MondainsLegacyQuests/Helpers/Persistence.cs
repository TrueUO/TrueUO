using Server.Mobiles;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.Quests
{
    public static class MondainQuestData
    {
        private static readonly string _FilePath = Path.Combine("Saves/Quests", "MLQuests.bin");

        public static Dictionary<PlayerMobile, List<BaseQuest>> QuestData { get; set; }
        public static Dictionary<PlayerMobile, Dictionary<QuestChain, BaseChain>> ChainData { get; set; }

        public static List<BaseQuest> GetQuests(PlayerMobile pm)
        {
            if (!QuestData.TryGetValue(pm, out List<BaseQuest> value))
            {
                value = new List<BaseQuest>();

                QuestData[pm] = value;
            }

            return value;
        }

        public static Dictionary<QuestChain, BaseChain> GetChains(PlayerMobile pm)
        {
            if (!ChainData.TryGetValue(pm, out Dictionary<QuestChain, BaseChain> value))
            {
                value = new Dictionary<QuestChain, BaseChain>();

                ChainData[pm] = value;
            }

            return value;
        }

        public static void Configure()
        {
            EventSink.WorldSave += OnSave;
            EventSink.WorldLoad += OnLoad;

            QuestData = new Dictionary<PlayerMobile, List<BaseQuest>>();
            ChainData = new Dictionary<PlayerMobile, Dictionary<QuestChain, BaseChain>>();
        }

        public static void OnSave(WorldSaveEventArgs e)
        {
            Persistence.Serialize(
                _FilePath,
                writer =>
                {
                    writer.Write(0);

                    writer.Write(QuestData.Count);
                    foreach (KeyValuePair<PlayerMobile, List<BaseQuest>> kvp in QuestData)
                    {
                        writer.Write(kvp.Key);
                        QuestWriter.Quests(writer, kvp.Value);
                    }

                    writer.Write(ChainData.Count);
                    foreach (KeyValuePair<PlayerMobile, Dictionary<QuestChain, BaseChain>> kvp in ChainData)
                    {
                        writer.Write(kvp.Key);
                        QuestWriter.Chains(writer, kvp.Value);
                    }

                    TierQuestInfo.Save(writer);
                });
        }

        public static void OnLoad()
        {
            Persistence.Deserialize(
                _FilePath,
                reader =>
                {
                    int version = reader.ReadInt();

                    int count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        PlayerMobile pm = reader.ReadMobile() as PlayerMobile;

                        List<BaseQuest> quests = QuestReader.Quests(reader, pm);

                        if (pm != null)
                            QuestData[pm] = quests;
                    }

                    count = reader.ReadInt();
                    for (int i = 0; i < count; i++)
                    {
                        PlayerMobile pm = reader.ReadMobile() as PlayerMobile;

                        Dictionary<QuestChain, BaseChain> dic = QuestReader.Chains(reader);

                        if (pm != null)
                            ChainData[pm] = dic;
                    }

                    TierQuestInfo.Load(reader);
                });
        }
    }
}
