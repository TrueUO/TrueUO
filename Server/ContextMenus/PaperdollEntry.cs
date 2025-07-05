namespace Server.ContextMenus
{
	public sealed class PaperdollEntry(Mobile m) : ContextMenuEntry(6123, 18)
    {
        public override void OnClick()
		{
			if (m.CanPaperdollBeOpenedBy(Owner.From))
			{
				m.DisplayPaperdollTo(Owner.From);
			}
		}
	}
}
