namespace Server.ContextMenus
{
	public sealed class OpenBackpackEntry(Mobile m) : ContextMenuEntry(6145)
    {
        public override void OnClick()
		{
			m.Use(m.Backpack);
		}
	}
}
