namespace Server.Engines.PartySystem
{
    public class PartyMemberInfo
    {
        private readonly Mobile _Mobile;
        private bool _CanLoot;

        public PartyMemberInfo(Mobile m)
        {
            _Mobile = m;
            _CanLoot = false;
        }

        public Mobile Mobile => _Mobile;

        public bool CanLoot
        {
            get => _CanLoot;
            set => _CanLoot = value;
        }
    }
}
