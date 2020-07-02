using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class GeographicCoordinate
    {
        public GeographicCoordinate() { }

        public GeographicCoordinate(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public double latitude;
        public double longitude;

        public double getDistanceTo(GeographicCoordinate coord)
        {
            throw new NotImplementedException("Distance not yet implemented");
        }
    }
}