namespace Server.Network.Packets
{
    public sealed class ToggleSpecialAbilityPacket : Packet
    {
        public ToggleSpecialAbilityPacket(int abilityID, bool active)
            : base(0xBF)
        {
            EnsureCapacity(7);

            m_Stream.Write((short)0x25);

            m_Stream.Write((short)abilityID);
            m_Stream.Write(active);
        }
    }
}
