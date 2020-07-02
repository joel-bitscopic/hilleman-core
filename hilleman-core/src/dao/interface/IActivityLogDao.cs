using System;
using System.Data;
using com.bitscopic.hilleman.core.domain.session;

namespace com.bitscopic.hilleman.core.dao.iface.praedigene
{
    public interface IActivityLogDao
    {
        void saveSearchMetrics(String searchTarget, DateTime requestStart, DateTime requestEnd, String requestParams, Int32 totalFilteredRecordCount, Int32 pageSize, Int32 pageStart);

        void logChange(String userID, DateTime timestamp, String callID, Object before, Object after, String reason);

        String startUserSession(HillemanSession session);

        //void addRequestToSession(String sessionDbKey, HillemanRequest request);

        void endUserSessionAndSaveRequests(String sessionDbKey, HillemanSession session);

        void saveData(com.bitscopic.hilleman.core.domain.SerializedVersionedNamespacedObject svno);

        void saveDataWithTx(com.bitscopic.hilleman.core.domain.SerializedVersionedNamespacedObject svno, IDbTransaction tx);
    }
}
