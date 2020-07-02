using com.bitscopic.hilleman.core.utils;
using System;

namespace com.bitscopic.hilleman.core.domain
{
    /// <summary>
    /// A single class for serializing objects with their namespace. Should be used ONLY on class types in this Assembly. The class' namespace
    /// being passed in can incorporate namespace versioning to provide backwards compatibility, future object changes, etc
    /// 
    /// e.g. com.bitscopic.hilleman.core.domain.praedigene.v2.HCVResistanceResponse
    /// </summary>
    [Serializable]
    public abstract class SerializedVersionedNamespacedObject
    {
        public String fullVersionedName;
        public String serializedValue;

        /// <summary>
        /// Helper accessor. If serialized value was not already set, the implementation class is serialized 
        /// </summary>
        /// <returns></returns>
        public String getSerializedValue()
        {
            if (String.IsNullOrEmpty(serializedValue))
            {
                setFullVersionedNameAndSerialize(this);
            }
            return this.serializedValue;
        }

        /// <summary>
        /// Helper accessor. If full versioned name was not already set. the implementation's full type name is set
        /// </summary>
        /// <returns></returns>
        public String getFullVersionedName()
        {
            if (String.IsNullOrEmpty(fullVersionedName))
            {
                setFullVersionedNameAndSerialize(this);
            }
            return this.fullVersionedName;
        }

        void setFullVersionedNameAndSerialize(object forSerialization)
        {
            this.fullVersionedName = this.serializedValue = null;
            this.serializedValue = SerializerUtils.serialize(forSerialization, false);
            this.fullVersionedName = forSerialization.GetType().FullName;
        }

        /// <summary>
        /// Builds a SVNOTO object to ensure the only two properties being serialized are the 'serializedValue' and 'fullVersionedName'
        /// </summary>
        /// <returns></returns>
        String serialize()
        {
            setFullVersionedNameAndSerialize(this);
            SVNOTO serializedSVNO = new SVNOTO() { fullVersionedName = this.getFullVersionedName(), serializedValue = this.getSerializedValue() };
            return SerializerUtils.serialize(serializedSVNO, false);
        }

        public abstract object deserialize();
    }

    [Serializable]
    public class SVNOTO
    {
        public String serializedValue;
        public String fullVersionedName;

        public SVNOTO() { }
    }
}
