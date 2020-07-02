using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public enum VistaRpcParameterType
    {
        LITERAL = 1,
        REFERENCE = 2,
        LIST = 3,
        WORDPROC = 4
    }

    public class VistaRpcParameter
    {
        VistaRpcParameterType _type;
        String _plainTextValue;
        String _encryptedValue;
        Dictionary<String, String> _listValues;
        bool _isEncrypted;

        public VistaRpcParameter(VistaRpcParameterType type, String value, bool encryptValue = false, String[] cipherPad = null) 
        {
            _type = type;
            _plainTextValue = value;
            if (encryptValue)
            {
                _isEncrypted = true;
                if (cipherPad != null && cipherPad.Length > 0)
                {
                    _encryptedValue = VistaRpcStringUtils.encrypt(value, cipherPad);
                }
                else
                {
                    _encryptedValue = VistaRpcStringUtils.encrypt(value);
                }
            }
        }

        public VistaRpcParameter(VistaRpcParameterType type, Dictionary<String, String> list)
        {
            _type = VistaRpcParameterType.LIST;
            _plainTextValue = ".x";
            _listValues = list;
        }

        public VistaRpcParameterType getType()
        {
            return _type;
        }

        public object getValue()
        {
            if (_type == VistaRpcParameterType.LIST)
            {
                return _listValues;
            }
            else if (_isEncrypted)
            {
                return _encryptedValue;
            }
            else
            {
                return _plainTextValue;
            }
        }
    }
}