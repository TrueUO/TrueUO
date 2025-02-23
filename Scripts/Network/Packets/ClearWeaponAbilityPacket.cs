namespace Server.Network.Packets
{
    public sealed class ClearWeaponAbilityPacket : Packet
    {
        public static readonly Packet Instance = SetStatic(new ClearWeaponAbilityPacket());

        public ClearWeaponAbilityPacket()
            : base(0xBF)
        {
            EnsureCapacity(5);

            m_Stream.Write((short)0x21);
        }
    }
}
