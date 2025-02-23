namespace Server.Network.Packets
{
    public enum ScreenEffectType
    {
        FadeOut = 0x00,
        FadeIn = 0x01,
        LightFlash = 0x02,
        FadeInOut = 0x03,
        DarkFlash = 0x04
    }

    public class ScreenEffect : Packet
    {
        public ScreenEffect(ScreenEffectType type)
            : base(0x70, 28)
        {
            m_Stream.Write((byte)0x04);
            m_Stream.Fill(8);
            m_Stream.Write((short)type);
            m_Stream.Fill(16);
        }
    }

    public sealed class ScreenFadeOut : ScreenEffect
    {
        public static readonly Packet Instance = SetStatic(new ScreenFadeOut());

        public ScreenFadeOut()
            : base(ScreenEffectType.FadeOut)
        { }
    }

    public sealed class ScreenFadeIn : ScreenEffect
    {
        public static readonly Packet Instance = SetStatic(new ScreenFadeIn());

        public ScreenFadeIn()
            : base(ScreenEffectType.FadeIn)
        { }
    }

    public sealed class ScreenFadeInOut : ScreenEffect
    {
        public static readonly Packet Instance = SetStatic(new ScreenFadeInOut());

        public ScreenFadeInOut()
            : base(ScreenEffectType.FadeInOut)
        { }
    }

    public sealed class ScreenLightFlash : ScreenEffect
    {
        public static readonly Packet Instance = SetStatic(new ScreenLightFlash());

        public ScreenLightFlash()
            : base(ScreenEffectType.LightFlash)
        { }
    }

    public sealed class ScreenDarkFlash : ScreenEffect
    {
        public static readonly Packet Instance = SetStatic(new ScreenDarkFlash());

        public ScreenDarkFlash()
            : base(ScreenEffectType.DarkFlash)
        { }
    }
}
