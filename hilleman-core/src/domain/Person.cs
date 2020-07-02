using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Person : BaseClass
    {
        public String lastName;
        public String firstName;
        public String middleName;
        public String nameString;
        public DateTime dateOfBirth;
        public String dateOfBirthVistA;
        public String gender;
        /// <summary>
        /// The 'phones' dictionary is instantiated with the Person class using a "case-insensitive" equality comparer. DO NOT reinstantiate
        /// this dictionary unless you require some other behavior!
        /// </summary>
        public Dictionary<String, String> phones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// The 'emailAddresses' dictionary is instantiated with the Person class using a "case-insensitive" equality comparer. DO NOT reinstantiate
        /// this dictionary unless you require some other behavior!
        /// </summary>
        public Dictionary<String, String> emailAddresses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public IdentifierSet idSet;
        public Address address;

        public Person() { }

        public void addId(Identifier id)
        {
            if (this.idSet == null)
            {
                this.idSet = new IdentifierSet();
            }
            if (this.idSet.ids == null)
            {
                this.idSet.ids = new List<Identifier>();
            }

            this.idSet.ids.Add(id);
        }

    }
}