using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.domain.pooling;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class SourceSystem
    {
        public AbstractPoolSource poolingConfig;
        public String name;
        public String id;
        public String organizationalUnit;
        public SourceSystemType type;
        public String[] cipherPad;
        public String timeZone;

        public Credentials credentials;
        public String connectionString;

        public List<Facility> usedBy;

        public GeographicCoordinate coordinates;

        public SourceSystem() { }

        [NonSerialized]
        public TimeZoneInfo timeZoneParsed;
    }

    public enum SourceSystemType
    {
        VISTA_CRUD_REST_SVC,
        VISTA_RPC_BROKER,
        SQLITE_CACHE,
        SQLITE,
        ORACLE
    }
}