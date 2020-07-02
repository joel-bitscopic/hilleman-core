using System;
using System.Collections.Generic;
using System.Linq;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.domain.to;
using com.bitscopic.hilleman.core.dao;

namespace com.bitscopic.hilleman.core.refactoring
{
    public class FileManQueryDao : IRefactoringApi
    {
        IVistaConnection _cxn;
        public FileManQueryDao(IVistaConnection cxn)
        {
            _cxn = cxn;
        }

        public void setTarget(dao.vista.IVistaConnection target)
        {
            _cxn = target;
        }

        public String[] readRange(ReadRange request)
        {
            ReadRangeResponse response = new CrrudDaoFactory().getCrrudDao(_cxn).readRange(new dao.ReadRangeRequest(_cxn.getSource(), request));
            return response.value.ToArray();
        }
    }
}