using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class PDF : FileSystemFile
    {
        public PDF() { }

        public PDF(String fileName, byte[] data) : base(fileName, data) {  }

        public PDF(MemoryStream ms, DateTime created, String fileName)
        {
            ms.Position = 0;
            this.created = created;
            this.data = ms.GetBuffer();
            this.size = Convert.ToInt32(ms.Length);
            this.fileName = fileName;
        }
    }
}