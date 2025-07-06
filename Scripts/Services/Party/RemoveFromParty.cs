using Server.Engines.PartySystem;

namespace Server.ContextMenus
{
    public class RemoveFromPartyEntry : ContextMenuEntry
    {
        private readonly Mobile _From;
        private readonly Mobile _Target;

        public RemoveFromPartyEntry(Mobile from, Mobile target)
            : base(0198, 12)
        {
            _From = from;
            _Target = target;
        }

        public override void OnClick()
        {
            Party p = Party.Get(_From);

            if (p == null || p.Leader != _From || !p.Contains(_Target))
            {
                return;
            }

            if (_From == _Target)
            {
                _From.SendLocalizedMessage(1005446); // You may only remove yourself from a party if you are not the leader.
            }
            else
            {
                p.Remove(_Target);
            }
        }
    }
}
