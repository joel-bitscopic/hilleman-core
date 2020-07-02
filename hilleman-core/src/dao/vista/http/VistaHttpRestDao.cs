using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.dao.vista.http
{
    public class VistaHttpRestDao : ICrrudDao
    {
        IVistaConnection _cxn;

        public VistaHttpRestDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public ReadRangeResponse readRange(ReadRangeRequest request)
        {
            return ReadRangeResponse.parseJsonDdrResponse(HttpUtils.Post(new Uri(_cxn.getSource().connectionString), "range", (String)request.buildRequest()));
        }

        public ReadResponse read(ReadRequest request)
        {
            return ReadResponse.parseReadResponse(request, HttpUtils.Get(new Uri(_cxn.getSource().connectionString), String.Format("{0}/{1}", request.getFile(), request.getIens())));
        }

        public CreateResponse create(CreateRequest request)
        {
            return CreateResponse.parseCreateResponse(request, HttpUtils.Put(new Uri(_cxn.getSource().connectionString), "", (String)request.buildRequest()));
        }

        public UpdateResponse update(UpdateRequest request)
        {
            return UpdateResponse.parseUpdateResponse(request, HttpUtils.Post(new Uri(_cxn.getSource().connectionString), "", (String)request.buildRequest()));
        }

        public DeleteResponse delete(DeleteRequest request)
        {
            return DeleteResponse.parseDeleteResponse(request, HttpUtils.Delete(new Uri(_cxn.getSource().connectionString), request.buildRequest(), ""));
        }


        public SourceSystem getSource()
        {
            return _cxn.getSource();
        }
    }
}