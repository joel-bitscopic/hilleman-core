using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.dao
{
    public class DeleteResponse : BaseCrrudResponse
    {
        public static DeleteResponse parseDeleteResponse(DeleteRequest request, String response)
        {
            if (request.getSource().type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<DeleteResponse>(response);
            }
            else if (request.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return parseRpcDeleteResponse(response);
            }
            else
            {
                throw new NotImplementedException("The request type has not been implemented for delete");
            }
        }

        private static DeleteResponse parseRpcDeleteResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
            {
                throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException("An empty response was received but is invalid for this operation");
            }

            IList<String> pieces = StringUtils.splitToList(response, StringUtils.CRLF_ARY, StringSplitOptions.RemoveEmptyEntries);
            DeleteResponse result = new DeleteResponse() { value = pieces };
            if (!result.isSuccessfulCreateUpdateDeleteResponse())
            {
                throw new CrrudException(result.extractError(response));
            }
            return result;
        }
    }
}