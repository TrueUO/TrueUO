using System;
using System.Collections.Generic;

namespace Server.Network
{
    internal class NetworkUtility
    {
        public struct AuthIDPersistence
        {
            public DateTime Age;
            public ClientVersion Version;

            public AuthIDPersistence(ClientVersion v)
            {
                Age = DateTime.UtcNow;
                Version = v;
            }
        }

        private const int m_AuthIDWindowSize = 128;

        public static readonly Dictionary<uint, AuthIDPersistence> m_AuthIDWindow = new Dictionary<uint, AuthIDPersistence>(m_AuthIDWindowSize);

        public static uint GenerateAuthID(NetState state)
        {
            if (m_AuthIDWindow.Count == m_AuthIDWindowSize)
            {
                uint oldestID = 0;
                DateTime oldest = DateTime.MaxValue;

                foreach (KeyValuePair<uint, AuthIDPersistence> kvp in m_AuthIDWindow)
                {
                    if (kvp.Value.Age < oldest)
                    {
                        oldestID = kvp.Key;
                        oldest = kvp.Value.Age;
                    }
                }

                m_AuthIDWindow.Remove(oldestID);
            }

            uint authID;

            do
            {
                authID = (uint)Utility.RandomMinMax(1, uint.MaxValue - 1);

                if (Utility.RandomBool())
                {
                    authID |= 1U << 31;
                }
            }
            while (m_AuthIDWindow.ContainsKey(authID));

            m_AuthIDWindow[authID] = new AuthIDPersistence(state.Version);

            return authID;
        }
        public static bool VerifyGC(NetState state)
        {
            if (state.Mobile == null || state.Mobile.IsPlayer())
            {
                if (state.Running)
                {
                    Console.WriteLine("Warning: {0}: Player using godclient, disconnecting", state);
                }

                state.Dispose();
                return false;
            }

            return true;
        }
    }
}
