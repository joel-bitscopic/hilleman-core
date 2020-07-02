using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using Newtonsoft.Json;

namespace com.bitscopic.hilleman.core.dao
{
    public class ReadRangeResponse : BaseCrrudResponse
    {
        public ReadRangeResponse() { }

        public static ReadRangeResponse parseResponse(ReadRangeRequest request, String response)
        {
            if (request.getSource().type == domain.SourceSystemType.VISTA_CRUD_REST_SVC)
            {
                return ReadRangeResponse.parseJsonDdrResponse(response);
            }
            else if (request.getSource().type == domain.SourceSystemType.VISTA_RPC_BROKER)
            {
                return ReadRangeResponse.parseVistaRpcResponse(request, response);
            }
            else
            {
                throw new NotImplementedException(String.Format("The source type {0} is not currently supported for the read range request",
                    Enum.GetName(typeof(domain.SourceSystemType), request.getSource())));
            }
        }

        public static ReadRangeResponse parseJsonDdrResponse(String response)
        {
            return JsonConvert.DeserializeObject<ReadRangeResponse>(response);
        }

        public static ReadRangeResponse parseVistaRpcResponse(ReadRangeRequest request, String response)
        {
            if (String.IsNullOrEmpty(request.apiName) || "DDR LISTER".Equals(request.apiName, StringComparison.CurrentCultureIgnoreCase))
            {
                if (request.getFlags().Contains("P"))
                {
                    return parseVistaRpcPackedResponseDdrLister(response);
                }
                else
                {
                    throw new NotImplementedException("Unpacked DDR LISTER result formatting is not yet supported. Check back later...");
                }
            }
            else if ("SC LISTER".Equals(request.apiName, StringComparison.CurrentCultureIgnoreCase))
            {
                if (request.getFlags().Contains("P"))
                {
                    return parseVistaRpcPackedResponseScLister(response);
                }
                else
                {
                    throw new NotImplementedException("Unpacked DDR LISTER result formatting is not yet supported. Check back later...");
                }
            }

            throw new ArgumentException("Unable to parse request: " + SerializerUtils.serialize(request) + " -- Response: " + response);
        }

        public static ReadRangeResponse parseVistaRpcPackedResponseDdrLister(String response)
        {
            return ReadRangeResponse.parseVistaRpcPackedResponse(response, VistaRpcConstants.BEGIN_ERRS, VistaRpcConstants.END_ERRS, VistaRpcConstants.BEGIN_DATA, VistaRpcConstants.END_DATA);
        }

        public static ReadRangeResponse parseVistaRpcPackedResponseScLister(String response)
        {
            throw new NotImplementedException("Check back soon!");
            return ReadRangeResponse.parseVistaRpcPackedResponse(response, VistaRpcConstants.BEGIN_ERRS, VistaRpcConstants.END_ERRS, VistaRpcConstants.BEGIN_DATA, VistaRpcConstants.END_DATA);
        }

        private static ReadRangeResponse parseVistaRpcPackedResponse(String response, String beginErrorsConst, String endErrorsConst, String beginDataConst, String endDataConst)
        {
            List<String> lines = StringUtils.splitToList(response, StringUtils.CRLF_ARY, StringSplitOptions.None);

            Int32 errIdx = StringUtils.firstIndexOf(lines, beginErrorsConst);
            if (errIdx > -1) // found error!
            {
                Int32 endErrorsIdx = StringUtils.firstIndexOf(lines, endErrorsConst);
                if (endErrorsIdx < 0)
                {
                    endErrorsIdx = lines.Count;
                }
                throw new CrrudException(StringUtils.join(lines, null, 3, endErrorsIdx));
            }

            Int32 startIndex = StringUtils.firstIndexOf(lines, beginDataConst);

            if (startIndex < 0)
            {
                throw new CrrudException("Unexpected data format - no '" + beginDataConst + "'");
            }

            Int32 endIndex = StringUtils.lastIndexOf(lines, endDataConst);
            if (endIndex < 0)
            {
                endIndex = lines.Count;
            }

            lines = lines.GetRange(startIndex + 1, endIndex - startIndex - 1); // -1 so we don't include don't include end_data line

            return new ReadRangeResponse() { value = lines };
        }
    }
}