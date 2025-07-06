namespace Server.Network.Packets
{
    public enum DeleteResultType
    {
        PasswordInvalid,
        CharNotExist,
        CharBeingPlayed,
        CharTooYoung,
        CharQueued,
        BadRequest
    }

    public sealed class CharacterDeleteResultPacket : Packet
    {
        public CharacterDeleteResultPacket(DeleteResultType res)
            : base(0x85, 2)
        {
            m_Stream.Write((byte)res);
        }
    }
}
