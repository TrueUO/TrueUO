#region References
using Server.Network;
#endregion

namespace Server
{
    public class QuestArrow
    {
        private readonly Mobile m_Mobile;
        private readonly IPoint3D m_Target;
        private bool m_Running;

        public Mobile Mobile => m_Mobile;

        public IPoint3D Target => m_Target;

        public bool Running => m_Running;

        public void Update()
        {
            int xOffset = 0;
            int yOffset = 0;
            int zOffset = (int)System.Math.Round((double)m_Target.Z / 10);

            if (zOffset > 0)
                zOffset -= 1;
            else if (zOffset < 0)
                zOffset += 1;

            Point2D fakeTarget = new Point2D(m_Target.X - zOffset, m_Target.Y - zOffset);

            Direction direction = m_Mobile.GetDirectionTo(m_Target.X - zOffset, m_Target.Y - zOffset);

            if (m_Mobile.InRange(fakeTarget, 2))
            {
                direction = Direction.Up;
            }


            switch (direction)
            {
                case Direction.North:
                    xOffset = 0;
                    yOffset = 1;
                    break;
                case Direction.Right:
                    xOffset = -1;
                    yOffset = 1;
                    break;
                case Direction.East:
                    xOffset = -1;
                    yOffset = 0;
                    break;
                case Direction.Down:
                    xOffset = -1;
                    yOffset = -1;
                    break;
                case Direction.South:
                    xOffset = 0;
                    yOffset = -1;
                    break;
                case Direction.Left:
                    xOffset = 1;
                    yOffset = -1;
                    break;
                case Direction.West:
                    xOffset = 1;
                    yOffset = 0;
                    break;
                case Direction.Up:
                    xOffset = 1;
                    yOffset = 1;
                    break;
                default:
                    break;
            }

            xOffset -= zOffset + 1;
            yOffset -= zOffset + 1;
            Update(m_Target.X + xOffset, m_Target.Y + yOffset);
        }

        public void Update(int x, int y)
        {
            if (!m_Running)
            {
                return;
            }

            NetState ns = m_Mobile.NetState;

            if (ns == null)
            {
                return;
            }

            if (m_Target is IEntity entity)
            {
                ns.Send(new SetArrow(x, y, entity.Serial));
            }
            else
            {
                ns.Send(new SetArrow(x, y, Serial.MinusOne));
            }
        }

        public void Stop()
        {
            Stop(m_Target.X, m_Target.Y);
        }

        public void Stop(int x, int y)
        {
            if (!m_Running)
            {
                return;
            }

            m_Mobile.ClearQuestArrow();

            NetState ns = m_Mobile.NetState;

            if (ns != null)
            {
                if (m_Target is IEntity entity)
                {
                    ns.Send(new CancelArrow(x, y, entity.Serial));
                }
                else
                {
                    ns.Send(new CancelArrow(x, y, Serial.MinusOne));
                }
            }

            m_Running = false;
            OnStop();
        }

        public virtual void OnStop()
        { }

        public virtual void OnClick(bool rightClick)
        { }

        public QuestArrow(Mobile m, IPoint3D t)
        {
            m_Running = true;
            m_Mobile = m;
            m_Target = t;
        }

        public QuestArrow(Mobile m, IPoint3D t, int x, int y)
            : this(m, t)
        {
            Update(x, y);
        }
    }
}
