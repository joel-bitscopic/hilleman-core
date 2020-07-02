using System;

namespace com.bitscopic.hilleman.core.refactoring
{
    public interface IRefactoringApi
    {
        //com.bitscopic.hilleman.core.domain.session.HillemanSession getSession();
        void setTarget(com.bitscopic.hilleman.core.dao.vista.IVistaConnection target);
    }
}