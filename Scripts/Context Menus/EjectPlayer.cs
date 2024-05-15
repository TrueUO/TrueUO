using Server.Multis;

namespace Server.ContextMenus
{
    public enum HouseAccessType
    {
        None,
        Friend,
        CoOwner,
        Owner
    }

    public class EjectPlayerEntry : ContextMenuEntry
    {
        private readonly Mobile _From;
        private readonly Mobile _Target;
        private readonly BaseHouse _TargetHouse;

        public EjectPlayerEntry(Mobile from, Mobile target)
            : base(6206, 12)
        {
            _From = from;
            _Target = target;
            _TargetHouse = BaseHouse.FindHouseAt(_Target);
        }

        public static bool CheckAccessible(Mobile from, Mobile target)
        {
            BaseHouse house = BaseHouse.FindHouseAt(target);

            if (house != null)
            {
                HouseAccessType fromAccess = GetAccess(from, house);
                HouseAccessType targetAccess = GetAccess(target, house);

                if (house.IsFriend(from) && fromAccess > targetAccess)
                {
                    return true;
                }
            }

            return false;
        }

        public static HouseAccessType GetAccess(Mobile from, BaseHouse house)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
            {
                return HouseAccessType.Owner; // Staff can access anything
            }

            HouseAccessType type = HouseAccessType.None;

            if (house != null)
            {
                if (house.IsOwner(from))
                {
                    type = HouseAccessType.Owner;
                }
                else if (house.IsCoOwner(from))
                {
                    type = HouseAccessType.CoOwner;
                }
                else if (house.IsFriend(from))
                {
                    type = HouseAccessType.Friend;
                }
            }

            return type;
        }

        public override void OnClick()
        {
            if (!_From.Alive || _TargetHouse.Deleted || !_TargetHouse.IsFriend(_From))
            {
                return;
            }

            if (_Target != null)
            {
                _TargetHouse.Kick(_From, _Target);
            }
        }
    }
}
