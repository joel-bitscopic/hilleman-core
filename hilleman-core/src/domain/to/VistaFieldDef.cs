using System;

namespace com.bitscopic.hilleman.core.domain.to
{
    [Serializable]
    public class VistaFieldDef
    {
        public bool isMultiple;
        public bool isPointer;
        public bool isWordProc;
        public string name;
        public string nodePiece;
        public string number;
        public VistaFile pointsTo;
        public string transform;
        public string type;
    }
}