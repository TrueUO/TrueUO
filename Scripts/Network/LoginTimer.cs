using System;

namespace Server.Network
{
    public class LoginTimer : Timer
    {
        private NetState _State;

        public LoginTimer(NetState state)
            : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
        {
            _State = state;
        }

        protected override void OnTick()
        {
            if (_State == null || !_State.Running)
            {
                Stop();

                _State = null;
            }
            else if (_State.Version != null)
            {
                Stop();

                _State.BlockAllPackets = false;

                PlayerPackets.DoLogin(_State);

                _State = null;
            }
        }
    }
}
