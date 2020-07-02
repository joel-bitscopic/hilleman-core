using System;
using System.Collections.Generic;

namespace com.bitscopic.hilleman.core.domain.vista
{
    [Serializable]
    public class FmField
    {
        public string name;
        public string number;
        public string nodePiece;
        public string access;
        public string dataTypeCode;
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public FmFieldType dataType;
        public string inputTransform;
        public string outputTransform;
        public string xref;
        public string description;
        public string helpPrompt;
        public List<FmFile> pointsTo;
        public Dictionary<String, String> setValues;
        public bool required;
        public bool alwaysAudited;
        public bool editDeleteAudited;

        public FmField() { }
    }

    public enum FmFieldType
    {
        FREE_TEXT,
        DATE_TIME,
        NUMERIC,
        SET_OF_CODES,
        SET_MULTIPLE,
        WORD_PROCESSING,
        COMPUTED,
        POINTER,
        POINTER_MULTIPLE,
        VARIABLE_POINTER,
        MUMPS,
        MULTIPLE,
        DATE_MULTIPLE
    }
}