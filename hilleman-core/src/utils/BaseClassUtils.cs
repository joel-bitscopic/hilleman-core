using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.utils
{
    public static class BaseClassUtils
    {
        public static T matchById<T>(IList<T> collectionOfBaseClassImpl, String id)
        {
            foreach (object t in collectionOfBaseClassImpl)
            {
                if (((com.bitscopic.hilleman.core.domain.BaseClass)t).id == id)
                {
                    return (T)t;
                }
            }

            return default(T);
        }
    }
}