using System;
using com.bitscopic.hilleman.core.domain.session;

namespace com.bitscopic.hilleman.core.dao.logging
{
    public interface ISessionDao
    {
        void saveSessionAsync(HillemanSession session);
    }
}