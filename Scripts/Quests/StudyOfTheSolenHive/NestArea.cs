namespace Server.Engines.Quests.Naturalist
{
    public class NestArea
    {
        private static readonly NestArea[] m_Areas =
        {
            new NestArea(false, new Rectangle2D(5861, 1787, 26, 25)),
            new NestArea(false, new Rectangle2D(5734, 1788, 14, 50),
                new Rectangle2D(5748, 1800, 3, 34),
                new Rectangle2D(5751, 1808, 2, 20)),
            new NestArea(false, new Rectangle2D(5907, 1908, 19, 43)),
            new NestArea(false, new Rectangle2D(5721, 1926, 24, 29),
                new Rectangle2D(5745, 1935, 7, 22)),
            new NestArea(true, new Rectangle2D(5651, 1853, 21, 32),
                new Rectangle2D(5672, 1857, 6, 20))
        };
        private readonly bool m_Special;
        private readonly Rectangle2D[] m_Rects;
        private NestArea(bool special, params Rectangle2D[] rects)
        {
            m_Special = special;
            m_Rects = rects;
        }

        public static int NonSpecialCount
        {
            get
            {
                int n = 0;

                for (var index = 0; index < m_Areas.Length; index++)
                {
                    NestArea area = m_Areas[index];

                    if (!area.Special)
                    {
                        n++;
                    }
                }

                return n;
            }
        }
        public bool Special => m_Special;
        public int ID
        {
            get
            {
                for (int i = 0; i < m_Areas.Length; i++)
                {
                    if (m_Areas[i] == this)
                        return i;
                }
                return 0;
            }
        }
        public static NestArea Find(IPoint2D p)
        {
            for (var index = 0; index < m_Areas.Length; index++)
            {
                NestArea area = m_Areas[index];

                if (area.Contains(p))
                {
                    return area;
                }
            }

            return null;
        }

        public static NestArea GetByID(int id)
        {
            if (id >= 0 && id < m_Areas.Length)
            {
                return m_Areas[id];
            }

            return null;
        }

        public bool Contains(IPoint2D p)
        {
            for (var index = 0; index < m_Rects.Length; index++)
            {
                Rectangle2D rect = m_Rects[index];

                if (rect.Contains(p))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
