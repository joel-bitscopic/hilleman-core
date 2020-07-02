using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.utils;
using System.Text;

namespace com.bitscopic.hilleman.core.dao.vista.rpc
{
    public class VistaRpcCrrudDao : ICrrudDao
    {
        VistaRpcConnection _cxn;

        public VistaRpcCrrudDao(IVistaConnection connection)
        {
            _cxn = (VistaRpcConnection)connection;
        }

        public String gvv(String arg)
        {
            VistaRpcQuery qry = new VistaRpcQuery("XWB GET VARIABLE VALUE");
            qry.addParameter(new VistaRpcParameter(VistaRpcParameterType.REFERENCE, arg));
            return (String)_cxn.query(qry);
        }

        public void heartbeat()
        {
            _cxn.query(new VistaRpcQuery("XWB IM HERE"));
        }

        public String getWelcomeMessage()
        {
            return (String)_cxn.query(new VistaRpcQuery("XUS INTRO MSG"));
        }

        public void setContext(VistaRpcConnectionBrokerContext context)
        {
            VistaRpcQuery qry = new VistaRpcQuery("XWB CREATE CONTEXT");
            qry.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, context.name, true, _cxn.getSource().cipherPad));
            String response = (String)_cxn.query(qry);
            if (!String.Equals("1", response))
            {
                throw new VistaRpcConnectionException(response);
            }
        }

        public User bseVisitWithWebCallback(User user, SourceSystem authenticationCallbackSource, VistaRpcVisitorCredentials credentials, VistaRpcConnectionBrokerContext context)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-35^");
            sb.Append(credentials.password);
            sb.Append("^" + user.sourceSystemId + "_" + user.id); // TODO - think this should be hashed - not because it's super important but to hide details in flight and at VistA

            VistaRpcQuery query = new VistaRpcQuery("XUS SIGNON SETUP");
            query.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL, sb.ToString()));

            String loginResponse = (String)_cxn.query(query);
            IList<String> pieces = StringUtils.splitToList(loginResponse, StringUtils.CRLF_ARY, StringSplitOptions.None);
            if (pieces.Count < 6 || pieces[5] == "0")
            {
                throw new VistaRpcConnectionException("BSE Visit Failed!");
            }

            if (context == null)
            {
                context = new VistaRpcConnectionBrokerContext("", MyConfigurationManager.getValue("DefaultBrokerContext"));
            }
            setContext(context);

            string arg = "$O(^VA(200,\"SSN\",\"" + user.idSet.getByName("FEDID").id + "\",0))";
            string duz = this.gvv(arg);
            if (String.IsNullOrEmpty(duz))
            {
                throw new ArgumentException("That Federated UID doesn't exist");
            }

            String zeroNode = new vista.rpc.VistaRpcToolsDao(_cxn).gvv(String.Format("$G(^VA(200,{0},0))", duz));
            String[] zeroNodePieces = StringUtils.split(zeroNode, StringUtils.CARAT);
            return new User() { id = duz, nameString = zeroNodePieces[0] };
        }

        public User visit(User user, VistaRpcVisitorCredentials credentials, VistaRpcConnectionBrokerContext context = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-31^DVBA_^");
            sb.Append(user.idSet.getByName("FEDID").id  + '^');
            sb.Append(credentials.username + '^');
            sb.Append(credentials.provider.name + '^');
            sb.Append(credentials.provider.id + '^');
            sb.Append(user.idSet.getByName("PROVIDER").id + '^');
            sb.Append(user.phones.Values.GetEnumerator().Current);

            VistaRpcQuery query = new VistaRpcQuery("XUS SIGNON SETUP");
            query.addParameter(new VistaRpcParameter(VistaRpcParameterType.LITERAL , sb.ToString()));

            String loginResponse = (String)_cxn.query(query);
            IList<String> pieces = StringUtils.splitToList(loginResponse, StringUtils.CRLF_ARY, StringSplitOptions.None);
            if (String.Equals(pieces[0], "0"))
            {
                throw new VistaRpcConnectionException(pieces[3]);
            }

            if (context == null)
            {
                context = new VistaRpcConnectionBrokerContext("", MyConfigurationManager.getValue("DefaultBrokerContext"));
            }
            setContext(context);

            string arg = "$O(^VA(200,\"SSN\",\"" + user.idSet.getByName("FEDID").id + "\",0))";
            string duz = this.gvv(arg);
            if (String.IsNullOrEmpty(duz))
            {
                throw new ArgumentException("That Federated UID doesn't exist");
            }

            String zeroNode = new vista.rpc.VistaRpcToolsDao(_cxn).gvv(String.Format("$G(^VA(200,{0},0))", duz));
            String[] zeroNodePieces = StringUtils.split(zeroNode, StringUtils.CARAT);
            return new User() { id = duz, nameString = zeroNodePieces[0] };
        }

        public User login(VistaRpcLoginCredentials credentials)
        {
            return login(credentials, new VistaRpcConnectionBrokerContext("",  MyConfigurationManager.getValue("DefaultBrokerContext"))); // default context for FileMan calls 
        }

        public User login(VistaRpcLoginCredentials credentials, VistaRpcConnectionBrokerContext context)
        {
            if (context == null || String.IsNullOrEmpty(context.name))
            {
                context = new VistaRpcConnectionBrokerContext("", MyConfigurationManager.getValue("DefaultBrokerContext"));
            }
            VistaRpcQuery setup = new VistaRpcQuery("XUS SIGNON SETUP");
            String setupResponse = (String)_cxn.query(setup);
            if (String.IsNullOrEmpty(setupResponse))
            {
                throw new VistaRpcConnectionException("Unable to setup connection for login");
            }
            
            VistaRpcQuery login = new VistaRpcQuery("XUS AV CODE");

            login.addParameter(
                new VistaRpcParameter(
                    VistaRpcParameterType.LITERAL,
                    String.Concat(credentials.username, ";", credentials.password),
                    true,
                    _cxn.getSource().cipherPad));

            String loginResponse = (String)_cxn.query(login);
            IList<String> pieces = StringUtils.splitToList(loginResponse, StringUtils.CRLF_ARY, StringSplitOptions.None);
            if (String.Equals(pieces[0], "0"))
            {
                throw new VistaRpcConnectionException(pieces[3]);
            }

            setContext(context);

            String zeroNode = new vista.rpc.VistaRpcToolsDao(_cxn).gvv(String.Format("$G(^VA(200,{0},0))", pieces[0]));
            String[] zeroNodePieces = StringUtils.split(zeroNode, StringUtils.CARAT);
            return new User() { id = pieces[0], nameString = zeroNodePieces[0] };
        }

        public ReadRangeResponse readRange(ReadRangeRequest request)
        {
            String rpcStr = (String)request.buildRequest();
            String response = (String)_cxn.query(rpcStr);
            return ReadRangeResponse.parseVistaRpcResponse(request, response);
        }

        public ReadResponse read(ReadRequest request)
        {
            String rpcStr = (String)request.buildRequest();
            String response = (String)_cxn.query(rpcStr);
            return ReadResponse.parseReadResponse(request, response);
        }

        public CreateResponse create(CreateRequest request)
        {
            String rpcStr = (String)request.buildRequest();
            String response = (String)_cxn.query(rpcStr);
            return CreateResponse.parseCreateResponse(request, response);
        }

        public UpdateResponse update(UpdateRequest request)
        {
            String rpcStr = (String)request.buildRequest();
            String response = (String)_cxn.query(rpcStr);
            return UpdateResponse.parseUpdateResponse(request, response);
        }

        public DeleteResponse delete(DeleteRequest request)
        {
            String rpcStr = (String)request.buildRequest();
            String response = (String)_cxn.query(rpcStr);
            return DeleteResponse.parseDeleteResponse(request, response);
        }

        public SourceSystem getSource()
        {
            return _cxn.getSource();
        }
    }
}