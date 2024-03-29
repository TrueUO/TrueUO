using Server.Gumps;
using Server.Mobiles;
using System;

namespace Server.Services.TownCryer
{
    public class TownCryerGuildGump : BaseTownCryerGump
    {
        public TownCryerGuildEntry Entry { get; private set; }

        public TownCryerGuildGump(PlayerMobile pm, TownCrier cryer, TownCryerGuildEntry entry)
            : base(pm, cryer)
        {
            Entry = entry;
        }

        public override void AddGumpLayout()
        {
            base.AddGumpLayout();

            AddHtml(57, 155, 724, 20, Entry.Title, false, false);
            AddHtmlLocalized(57, 180, 724, 20, 1158066, $"{Entry.Author}, {Entry.Guild.Name}", 0, false, false); // Posted By: ~1_NAME~

            AddHtmlLocalized(57, 215, 50, 20, 1158067, false, false); // When:
            AddHtmlLocalized(57, 235, 50, 20, 1158068, false, false); // Where:

            int time = Entry.EventTime.Hour;

            AddLabel(102, 215, 0, $"{Entry.EventTime.Month}-{Entry.EventTime.Day}-{Entry.EventTime.Year} {(time == 0 ? 12 : time > 12 ? time - 12 : time)}{(Entry.EventTime.Hour >= 12 ? "pm" : "am")} {TimeZoneInfo.Local.StandardName}");

            AddLabel(102, 235, 0, Entry.EventLocation);

            AddHtml(57, 270, 724, 205, Entry.Body, false, false);

            AddImage(85, 425, 0x5EF);
        }

        public override void OnResponse(RelayInfo info)
        {
            if (info.ButtonID == 0)
            {
                TownCryerGump gump = new TownCryerGump(User, Cryer)
                {
                    Category = TownCryerGump.GumpCategory.Guild
                };
                SendGump(gump);
            }
        }
    }
}
