using System;
using System.Configuration;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    public static class HL7Helper
    {
        static char? HL7_SEG_DELIM = null;
        static char? HL7_SOT = null;
        static char? HL7_EOT_NM = null; // HL7 messages may be chained: <SOT>hl7 message 1<EOT><EOT_NM><SOT>hl7 message 2<EOT><EOT_NM><SOT>hl7 message 3<EOT> etc...
        static char? HL7_EOT = null; // end of tx char  

        public static char getHL7SegmentDelimiterCharFromConfig()
        {
            if (HL7Helper.HL7_SEG_DELIM == null && !String.IsNullOrEmpty(MyConfigurationManager.getValue("HL7_SEGMENT_DELIMITER_HEX")))
            {
                HL7_SEG_DELIM = Char.Parse(MyConfigurationManager.getValue("HL7_SEGMENT_DELIMITER_HEX"));
            }
            else if (HL7_SEG_DELIM == null)
            {
                HL7_SEG_DELIM = '\x0D';
            }

            return HL7_SEG_DELIM.Value;
        }

        public static char getHL7SOTCharFromConfig()
        {
            if (!String.IsNullOrEmpty(MyConfigurationManager.getValue("HL7_SOT_CHAR_HEX")))
            {
                HL7_SOT = Char.Parse(MyConfigurationManager.getValue("HL7_SOT_CHAR_HEX"));
            }
            else if (HL7_SOT == null)
            {
                HL7_SOT = '\x0B';
            }

            return HL7_SOT.Value;
        }

        public static char getHL7EOT_NMCharFromConfig()
        {
            if (!String.IsNullOrEmpty(MyConfigurationManager.getValue("HL7_EOT_NEXT_MESSAGE_CHAR_HEX")))
            {
                HL7_EOT_NM = Char.Parse(MyConfigurationManager.getValue("HL7_EOT_NEXT_MESSAGE_CHAR_HEX"));
            }
            else if (HL7_EOT_NM == null)
            {
                HL7_EOT_NM = '\x0D';
            }

            return HL7_EOT_NM.Value;
        }

        public static char getHL7EOTCharFromConfig()
        {
            if (!String.IsNullOrEmpty(MyConfigurationManager.getValue("HL7_EOT_CHAR_HEX")))
            {
                HL7_EOT = Char.Parse(MyConfigurationManager.getValue("HL7_EOT_CHAR_HEX"));
            }
            else if (HL7_EOT == null)
            {
                HL7_EOT = '\x1C';
            }

            return HL7_EOT.Value;
        }

    }
}