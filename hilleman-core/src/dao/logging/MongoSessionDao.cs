using System;
using System.Collections.Generic;
//using MongoDB.Bson;
//using MongoDB.Driver;
using com.bitscopic.hilleman.core.domain.session;

namespace com.bitscopic.hilleman.core.dao.logging
{
    public class MongoSessionDao : ISessionDao
    {
       // String _cxnString = "mongodb:localhost";
 //       MongoClient _client;

        public MongoSessionDao(String connectionString)
        {
            throw new NotImplementedException();
//            _cxnString = connectionString;
//            _client = new MongoClient(connectionString);
        }

        public void saveSession(HillemanSession session)
        {
            throw new NotImplementedException();
            //IMongoDatabase db = _client.GetDatabase("hilleman");

            //BsonDocument doc = new BsonDocument();
            //doc.Add("_id", session.sessionId);
            //doc.Add("start", session.sessionStart);
            //doc.Add("end", session.sessionEnd);

            //IMongoCollection<BsonDocument> mc = db.GetCollection<BsonDocument>("hilleman_session");
            //mc.InsertOneAsync(doc);
        }

        public void saveSessionAsync(HillemanSession session)
        {
            throw new NotImplementedException();
        }
    }
}