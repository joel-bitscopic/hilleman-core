using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.dao
{
    public class UpdateResponse : BaseCrrudResponse
    {
        internal static UpdateResponse parseUpdateResponse(UpdateRequest request, String response)
        {
            if (request.getSource().type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<UpdateResponse>(response);
            }
            else if (request.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return parseRpcUpdateResponse(response);
            }
            else
            {
                throw new NotImplementedException("The request type has not been implemented for create");
            }
        }

        private static UpdateResponse parseRpcUpdateResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
            {
                throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException("An empty response was received but is invalid for this operation");
            }

            IList<String> pieces = StringUtils.splitToList(response, StringUtils.CRLF_ARY, StringSplitOptions.RemoveEmptyEntries);

            if (pieces.Count > 1 && pieces[1].Contains(VistaRpcConstants.BEGIN_ERRS))
            {
                throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException(response);
            }

            UpdateResponse result = new UpdateResponse() { value = pieces };
            return result;
        }
    }
}