namespace Server.Items
{
    public abstract class BaseRefreshPotion : BasePotion
    {
        public BaseRefreshPotion(PotionEffect effect)
            : base(0xF0B, effect)
        {
        }

        public BaseRefreshPotion(Serial serial)
            : base(serial)
        {
        }

        public abstract double Refresh { get; }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
        }

        public override void Drink(Mobile from)
        {
            if (!PropertyEffect.VictimIsUnderEffects<BoneBreakerContext>(from))
            {
                from.Stam += Scale(from, (int) (Refresh * from.StamMax));

                PlayDrinkEffect(from);
                Consume();
            }
        }
    }
}
