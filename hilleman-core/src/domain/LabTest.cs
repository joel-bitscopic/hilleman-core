using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class LabTest : BaseClass
    {
        public LabTest() { }

        public String name;
        public String description;
        public String type;
        public String comment;
        public String cost;
        public LabTest parent;
        public DateTime completed;
        /// <summary>
        /// VistA specific - the workload code of a lab test (from file 64)
        /// </summary>
        public String wkldCode;

        public LabTest clone()
        {
            return new LabTest()
            {
                name = this.name,
                description = this.description,
                type = this.type,
                comment = this.comment,
                cost = this.cost,
                completed = this.completed,
                wkldCode = this.wkldCode,
                parent = (parent == null ? null : parent.clone())
            };
        }
    }
}