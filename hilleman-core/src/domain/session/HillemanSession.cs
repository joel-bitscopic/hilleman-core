using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao;
using com.bitscopic.hilleman.core.dao.vista;
using System.Configuration;

namespace com.bitscopic.hilleman.core.domain.session
{
    [Serializable]
    public class HillemanSession
    {
        public HillemanApp application;
        public String sessionToken;
        public IList<HillemanRequest> requests = new List<HillemanRequest>();
        public User endUser;
        public DateTime sessionStart = DateTime.Now; // set on instantiation!
        public DateTime sessionEnd;
        public String encryptedSerializedSymbolTable;
        [NonSerialized]
        public String plaintextSerializedSymbolTable;
        [NonSerialized]
        public ISet<String> authorizedConnections;
        [NonSerialized]
        IVistaConnection _baseCxn;
        [NonSerialized]
        internal String dbKey; 

        public HillemanSession()
        {
        }

        public HillemanSession(IVistaConnection cxn)
        {
            this.addAuthorizedConnection(cxn);
        }

        public void addRequest(HillemanRequest request)
        {
            this.requests.Add(request);
        }

        public void addAuthorizedConnection(IVistaConnection cxn)
        {
            if (this.authorizedConnections == null)
            {
                this.authorizedConnections = new SortedSet<String>(); // List<IVistaConnection>();
            }
            if (this.authorizedConnections.Count == 0)
            {
                _baseCxn = cxn;
            }
            if (!this.authorizedConnections.Contains(cxn.getSource().id))
            {
                this.authorizedConnections.Add(cxn.getSource().id);
            }
        }

        public IVistaConnection getBaseConnection()
        {
            if (this.authorizedConnections == null || this.authorizedConnections.Count == 0)
            {
                return null;
            }
            return _baseCxn; // first connection added is base
        }

        internal bool validateSymbolTable()
        {
            return true; // TODO - provide validation that asserts the symbol table came from VistA and hasn't been altered
        }

        /// <summary>
        /// Sync MySession's symbol table with what is found in VistA. Request the symbol table from the connection and update local plaintext/encrypted values
        /// </summary>
        /// <param name="cxn">Runs only if IVistaConnection is an appropriate implementation and Hilleman is configured to serialize/deserialize from VistA</param>
        internal void syncSymbolTable(IVistaConnection cxn)
        {
            if (cxn is com.bitscopic.hilleman.core.dao.vista.rpc.VistaStatelessRpcConnection)
            {
                if (true) // eventually set to check for whether we should serialize/deserialize symbol table
                {
                    this.plaintextSerializedSymbolTable = new com.bitscopic.hilleman.core.dao.vista.rpc.VistaRpcToolsDao(cxn).serializeSymbolTable();
                    this.encryptedSerializedSymbolTable = com.bitscopic.hilleman.core.utils.CryptographyUtils.hmac256Hash(MyConfigurationManager.getValue("EncryptionKey"), this.plaintextSerializedSymbolTable);
                }
            }
        }

        /// <summary>
        /// Sync MySession's symbol table with what is found in VistA. Request the symbol table from the connection and update local plaintext/encrypted values
        /// </summary>
        /// <param name="cxn">Runs only if IVistaConnection is an appropriate implementation and Hilleman is configured to serialize/deserialize from VistA</param>
        internal void setVistaSymbolTable(IVistaConnection cxn)
        {
            if (cxn is com.bitscopic.hilleman.core.dao.vista.rpc.VistaStatelessRpcConnection)
            {
                if (true) // eventually set to check for whether we should serialize/deserialize symbol table
                {
                    String cleaned = com.bitscopic.hilleman.core.utils.SymbolTableUtils.removeIgnoredSymbols(this.plaintextSerializedSymbolTable);
                    new com.bitscopic.hilleman.core.dao.vista.rpc.VistaRpcToolsDao(cxn).deserializeSymbolTable(cleaned);
                }
            }
        }
    }
}