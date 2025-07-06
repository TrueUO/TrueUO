using System;
using Server.Mobiles;

namespace Server.Spells
{
    class UnsummonTimer : Timer
    {
        private readonly BaseCreature _Creature;

        public UnsummonTimer(BaseCreature creature, TimeSpan delay)
            : base(delay)
        {
            _Creature = creature;
        }

        protected override void OnTick()
        {
            if (!_Creature.Deleted)
            {
                _Creature.Delete();
            }
        }
    }
}
