namespace Server.Items
{
    public class LavaLobsterTrap : LobsterTrap
    {
        public override int LabelNumber => 1116474; // lava lobster trap

        [Constructable]
        public LavaLobsterTrap()
        {
            Hue = 2515;
        }

        public LavaLobsterTrap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }
    }
}
