using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class Address : BaseClass
    {
        public String street1;
        public String street2;
        public String street3;
        public String city;
        public String county;
        public String state;
        public String zipcode;

        public Address() { }
    }
}