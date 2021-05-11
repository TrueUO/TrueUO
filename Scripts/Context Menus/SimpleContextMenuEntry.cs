using System;

namespace Server.ContextMenus
{
    public class SimpleContextMenuEntry : ContextMenuEntry
    {
        private Mobile From { get; }
        private Action<Mobile> Callback { get; }

        public SimpleContextMenuEntry(Mobile from, int localization, Action<Mobile> callback = null, int range = -1, bool enabled = true) : base(localization, range)
        {
            From = from;
            Callback = callback;

            Enabled = enabled;
        }

        public override void OnClick()
        {
            Callback?.Invoke(From);
        }
    }

    public class SimpleContextMenuEntry<T> : ContextMenuEntry
    {
        private readonly bool _NonLocalUse;

        private Mobile From { get; }
        private T State { get; }
        private Action<Mobile, T> Callback { get; }

        public override bool NonLocalUse => _NonLocalUse;

        public SimpleContextMenuEntry(Mobile from, int localization, Action<Mobile, T> callback, T state, int range = -1, bool enabled = true, bool nonlocalUse = false) : base(localization, range)
        {
            From = from;
            State = state;
            Callback = callback;

            _NonLocalUse = nonlocalUse;

            Enabled = enabled;
        }

        public override void OnClick()
        {
            Callback?.Invoke(From, State);
        }
    }
}
