using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using System.Configuration;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class SourceSystemTable
    {
        public IList<SourceSystem> sources; // for service exposure
        
        Dictionary<String, SourceSystem> _targetsById;

        public SourceSystem getSourceSystem(String id, bool convertIDToNumericForMatch = false)
        {
            if (convertIDToNumericForMatch)
            {
                id = com.bitscopic.hilleman.core.utils.StringUtils.extractNumeric(id);
            }

            if (_targetsById.ContainsKey(id))
            {
                return _targetsById[id];
            }
            throw new com.bitscopic.hilleman.core.domain.exception.HillemanBaseException("Source system ID (" + id + ") not in table");
        }

        /// <summary>
        /// Parameterless constructor for serializers only - do not use!
        /// </summary>
        public SourceSystemTable()
        {
            //useCacheFromTesting();
        }

        public SourceSystemTable(String fileContents)
        {
            if (String.IsNullOrEmpty(fileContents))
            {
                throw new ArgumentException("No contents. Unable to parse source system table");
            }
            //if (!System.IO.File.Exists(fullPath))
            //{
            //    throw new ArgumentException("Site table not found at: " + fullPath);
            //}

            parseSettingsFile(fileContents);
        }

        void parseSettingsFile(String fileContents)
        {
       //     this.sources = utils.SerializerUtils.deserialize<SourceSystemTable>(utils.FileIOUtils.readFile(fullPath)).sources;
            this.sources = utils.SerializerUtils.deserialize<SourceSystemTable>(fileContents).sources;
            populateSourcesForService();
            checkValid();
        }

        void useCacheFromTesting()
        {
            throw new ConfigurationErrorsException("Cached configuration settings are no longer supported");
        }

        void populateSourcesForService()
        {
            _targetsById = new Dictionary<string, SourceSystem>();
            foreach (SourceSystem ss in this.sources)
            {
                if (!_targetsById.ContainsKey(ss.id))
                {
                    _targetsById.Add(ss.id, ss);
                }
            }
        }

        void checkValid()
        {
            foreach (SourceSystem ss in _targetsById.Values)
            {
                if (String.IsNullOrEmpty(ss.timeZone))
                {
                    //throw new ConfigurationErrorsException(String.Format("Invalid source system table: {0} missing timezone", ss.id));
                    ss.timeZone = "UTC";
                }

                try
                {
                    ss.timeZoneParsed = TimeZoneInfo.FindSystemTimeZoneById(ss.timeZone);
                }
                catch (Exception exc)
                {
                    throw new ConfigurationErrorsException(String.Format("Invalid source system table timezone value ({0}) for {1}: {2}", ss.timeZone, ss.id, exc.Message));
                }
            }
        }
    }
}