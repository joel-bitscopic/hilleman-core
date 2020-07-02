using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class IdentifierSet
    {
        public IList<Identifier> ids;

        public IdentifierSet() 
        {
            ids = new List<Identifier>();
        }

        /// <summary>
        /// Helper for adding new Identifier objects
        /// </summary>
        /// <param name="id"></param>
        public void add(Identifier id)
        {
            if (this.ids == null)
            {
                this.ids = new List<Identifier>();
            }
            this.ids.Add(id);
        }

        /// <summary>
        /// Helper for adding new Identifier objects
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idName"></param>
        public void add(String id, String idName)
        {
            this.add(new Identifier() { id = id, name = idName });
        }

        /// <summary>
        /// Static helper method with null checking for fetching Identifier by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idSet"></param>
        /// <returns></returns>
        public static Identifier getById(String id, IdentifierSet idSet)
        {
            if (idSet != null && idSet.ids != null && idSet.ids.Count > 0)
            {
                return idSet.getById(id);
            }

            return null;
        }

        public Identifier getById(String id)
        {
            if (this.ids == null || this.ids.Count == 0)
            {
                return null;
            }

            foreach (Identifier ident in this.ids)
            {
                if (String.Equals(ident.id, id, StringComparison.CurrentCultureIgnoreCase))
                {
                    return ident;
                }
            }
            return null;
        }

        /// <summary>
        /// Static helper method with null checking for fetching Identifier by name 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="idSet"></param>
        /// <returns></returns>
        public static Identifier getByName(String name, IdentifierSet idSet)
        {
            if (idSet != null && idSet.ids != null && idSet.ids.Count > 0)
            {
                return idSet.getByName(name);
            }

            return null;
        }



        /// <summary>
        /// Return Identifier object by name. Returns null if no object with name is located
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Identifier getByName(String name)
        {
            if (this.ids == null || this.ids.Count == 0)
            {
                return null;
            }

            foreach (Identifier ident in this.ids)
            {
                if (String.Equals(ident.name, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return ident;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class Identifier : BaseClass
    {
        public String checksum;
        public String name;

        public Identifier() { }
    }
}