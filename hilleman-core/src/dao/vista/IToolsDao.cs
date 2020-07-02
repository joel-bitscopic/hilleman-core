using System;

namespace com.bitscopic.hilleman.core.dao
{
    public interface IToolsDao
    {
        String gvv(String arg);
        String getVistaSystemTime();
        void setGlobal(String globalKey, String globalValue);
        void killGlobal(String globalKey);
        void run(String m);
    }
}