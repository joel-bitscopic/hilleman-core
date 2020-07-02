using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.dao
{
    public class BaseCrrudResponse
    {
        public String type;
        public IList<String> value;

        public BaseCrrudResponse() { }

        public bool isSuccessfulCreateUpdateDeleteResponse()
        {
            if (value.Count == 1 && String.Equals(value[0], vista.rpc.VistaRpcConstants.DATA, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if (value.Count == 2 && value[1].StartsWith("+") && value[1].Contains("^")) // e.g.: [DATA]\r\n+1,^1234
            {
                return true;
            }
            if (value.Count == 0 || com.bitscopic.hilleman.core.utils.StringUtils.firstIndexOf(this.value, vista.rpc.VistaRpcConstants.BEGIN_ERRS) > -1)
            {
                return false;
            }
            return false;
        }

        public String extractError(String response)
        {
            if (String.IsNullOrEmpty(response) || response.IndexOf(vista.rpc.VistaRpcConstants.BEGIN_ERRS) < 0)
            {
                return response;
            }
            Int32 startIdx = response.IndexOf(vista.rpc.VistaRpcConstants.BEGIN_ERRS) + vista.rpc.VistaRpcConstants.BEGIN_ERRS.Length;
            String adjusted = response.Substring(startIdx);
            if (adjusted.StartsWith(StringUtils.CRLF))
            {
                adjusted = adjusted.Substring(StringUtils.CRLF.Length);
            }
            Int32 endIdx = adjusted.IndexOf(vista.rpc.VistaRpcConstants.END_ERRS);
            if (endIdx < 0)
            {
                return adjusted;
            }
            return adjusted.Substring(0, endIdx);
        }
    }
}