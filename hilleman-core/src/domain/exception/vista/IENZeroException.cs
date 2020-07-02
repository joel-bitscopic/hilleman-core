using System;

namespace com.bitscopic.hilleman.core.domain.exception.vista
{
    public class IENZeroException : HillemanBaseException
    {
        String vistaFile;
        String fromArg;
        String recordWithIENZero;

        public IENZeroException(String message) : base(message) { }

        public IENZeroException(String vistaFile, String fromArg, String vistaRecord)
        {
            this.vistaFile = vistaFile;
            this.fromArg = fromArg;
            this.recordWithIENZero = vistaRecord;
        }

        public String getRecord()
        {
            return recordWithIENZero;
        }
    }
}
