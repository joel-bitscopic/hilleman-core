using System;
using com.bitscopic.hilleman.core.utils;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista.rpc;

namespace com.bitscopic.hilleman.core.dao
{
    public class CreateResponse : BaseCrrudResponse
    {
        
        internal static CreateResponse parseCreateResponse(CreateRequest request, string response)
        {
            if (request.getSource().type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<CreateResponse>(response);
            }
            else if (request.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return parseRpcCreateResponse(response);
            }
            else
            {
                throw new NotImplementedException("The request type has not been implemented for create");
            }
        }

        private static CreateResponse parseRpcCreateResponse(string response)
        {
            if (String.IsNullOrEmpty(response))
            {
                throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException("An empty response was received but is invalid for this operation");
            }

            IList<String> pieces = StringUtils.splitToList(response, StringUtils.CRLF_ARY, StringSplitOptions.RemoveEmptyEntries);
            CreateResponse result = new CreateResponse() { value = pieces };
            return result;
        }

        public static String getCreatedIEN(CreateResponse response)
        {
            if (response.value == null || response.value.Count != 2)
            {
                throw new ArgumentException("The create response does not appear to have completed successfully");
            }
            return StringUtils.split(response.value[1], StringUtils.CARAT_ARY, StringSplitOptions.RemoveEmptyEntries)[1];
        }
    }
}