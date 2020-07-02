using System;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.dao
{
    public interface ICrrudDao
    {
        ReadRangeResponse readRange(ReadRangeRequest request);
        ReadResponse read(ReadRequest request);
        CreateResponse create(CreateRequest request);
        UpdateResponse update(UpdateRequest request);
        DeleteResponse delete(DeleteRequest request);

        SourceSystem getSource();
    }
}