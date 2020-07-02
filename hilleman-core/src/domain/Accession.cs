using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    public class Accession : BaseClass
    {
        /// <summary>
        /// The name - field .01
        /// </summary>
        public String area;
        public String abbreviation;
        public String subscript;
        /// <summary>
        /// 'S'hort or 'L'ong - field .092 (.4;2)
        /// </summary>
        public String type; 
        /// <summary>
        /// field .4 (.4;1)
        /// </summary>
        public String numericIdentifier;  
        /// <summary>
        /// field .03  (0;3)
        /// </summary>
        public String cleanUp;
        public User accessionedBy;
        /// <summary>
        /// VistA specific - the UID (unique identifier) - there is a xref on this identifier in file 68 (among others)
        /// </summary>
        public String uid;
    }
}