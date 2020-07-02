using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.utils
{
    public static class ActiveDirectoryHelper
    {

        public static IList<String> getCachedDomains()
        {
            return new List<String>()
            {
                "aac.dva.va.gov",
                "cem.va.gov",
                "dva.va.gov",
                "med.va.gov",
                "mpi.v21.med.va.gov",
                "r01.med.va.gov",
                "r02.med.va.gov",
                "r03.med.va.gov",
                "r04.med.va.gov",
                "v01.med.va.gov",
                "v02.med.va.gov",
                "v03.med.va.gov",
                "v04.med.va.gov",
                "v05.med.va.gov",
                "v06.med.va.gov",
                "v07.med.va.gov",
                "v08.med.va.gov",
                "v09.med.va.gov",
                "v10.med.va.gov",
                "v11.med.va.gov",
                "v12.med.va.gov",
                "v15.med.va.gov",
                "v16.med.va.gov",
                "v17.med.va.gov",
                "v18.med.va.gov",
                "v19.med.va.gov",
                "v20.med.va.gov",
                "v21.med.va.gov",
                "v22.med.va.gov",
                "v23.med.va.gov",
                "va.gov",
                "vba.va.gov",
                "vha.med.va.gov"            
            };
        }

        /// <summary>
        /// Return a 'LDAP://DC=va,DC=gov' type string from a 'v11.med.va.gov' formatted string
        /// </summary>
        /// <param name="domain">The domain with period separated DC values</param>
        /// <returns>LDAP string</returns>
        public static string getLDAPPath(string domain)
        {
            string ldapString = @"LDAP://";
            string[] tokens = domain.Split('.');
            foreach (string s in tokens)
            {
                ldapString = ldapString + "DC=" + s + ",";
            }
            return ldapString.Substring(0, ldapString.Length - 1);
        }


    }
}
