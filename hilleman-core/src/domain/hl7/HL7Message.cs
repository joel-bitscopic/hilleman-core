using com.bitscopic.hilleman.core.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.bitscopic.hilleman.core.domain.hl7
{
    /// <summary>
    /// HL7 version 2.X based classes
    /// </summary>
    [Serializable]
    public class HL7Message
    {
        public String sentFrom;
        public char fieldSeparator;
        public String encodingCharacters;
        public char componentSeparator;
        public char repetitionSeparator;
        public char escapeCharacter;
        public char subcomponentSeparator;

        char SOT_CHAR = HL7Helper.getHL7SOTCharFromConfig();
        char SEGMENT_SEPARATOR_CHAR = HL7Helper.getHL7SegmentDelimiterCharFromConfig();
        char EOT_CHAR = HL7Helper.getHL7EOTCharFromConfig();
        char EOT_NM_CHAR = HL7Helper.getHL7EOT_NMCharFromConfig();

        public String raw;
        public List<HL7Segment> segments;
        public HL7Message ack;

        public HL7Message() { }

        public HL7Message(String rawMsg)
        {
            this.raw = rawMsg;
            parse(rawMsg);
        }

        void parse(String rawMessage)
        {
            rawMessage = rawMessage.TrimStart(new char[] { SOT_CHAR });
            rawMessage = rawMessage.TrimEnd(new char[] { EOT_CHAR, EOT_NM_CHAR });
            rawMessage = rawMessage.TrimEnd(new char[] { EOT_CHAR });

            String[] rawSegments = StringUtils.split(rawMessage, '\x0D', StringSplitOptions.RemoveEmptyEntries);
            if (rawSegments == null || rawSegments.Length == 0)
            {
                throw new HL7Exception("Unable to parse HL7 message - no segments found");
            }

            this.segments = new List<HL7Segment>();
            // build MSH and copy over control characters from parsed MSH segment as they are used throughout the message. not entirely happy with this approach but seems right from OOP perspective
            MSH mshSegment = new MSH();
            mshSegment.parse(rawSegments[0], this);
            this.segments.Add(mshSegment);

            for (int i = 1; i < rawSegments.Length; i++)
            {
                if (rawSegments[i][rawSegments[i].Length - 1] == '\x0A') // handle when HL7 segments are seperated by CR & LF - strictly against the spec but message might come from text file, etc where CR/LF might creep in
                {
                    rawSegments[i] = rawSegments[i].Substring(0, rawSegments[i].Length - 1); 
                }
                HL7Segment currentSegment = new HL7Segment();
                currentSegment.parse(rawSegments[i], this);
                this.segments.Add(currentSegment);
            }
        }

        MSH _msh = null;
        internal MSH getMSH()
        {
            if (this.segments == null || !(this.segments.Any(seg => seg.segmentId == "MSH")))
            {
                throw new HL7Exception("The HL7 message object does not appear to have a MSH segment");
            }

            if (_msh == null)
            {
                _msh = (MSH)this.segments.First(seg => seg.segmentId == "MSH");
            }

            return _msh;
        }

        internal List<HL7Segment> getSegment(String segmentId)
        {
            if (this.segments == null || !(this.segments.Any(seg => seg.segmentId == segmentId)))
            {
                throw new HL7Exception("The HL7 message object does not appear to have a " + segmentId + " segment");
            }

            return (this.segments.Where(seg => seg.segmentId == segmentId)).ToList();
        }

        public HL7Message getUnsolicitedNack(String ackMessage = null)
        {
            HL7Message nack = new HL7Message();

            MSH ackMSH = MSH.buildDefaultMSH();
            ackMSH.messageControlId = CryptographyUtils.getNCharRandom(8);
            ackMSH.messageType = "ACK";

            MSA ackMSA = new MSA();
            ackMSA.acknowledgementCode = "AE";
            ackMSA.messageControlId = ackMSH.messageControlId;
            ackMSA.textMessage = (String.IsNullOrEmpty(ackMessage) ? "Message error" : ackMessage);

            nack.segments = new List<HL7Segment>();
            nack.segments.Add(ackMSH);
            nack.segments.Add(ackMSA);

            return nack;
        }

        /// <summary>
        /// Build a HL7 ACK message from this parsed HL7 message. Result is cached to preserve unique message control ID (available directly from the HL7Message.ack public property)
        /// </summary>
        /// <param name="ackMessage">Message to use in ack (default message is: "Message Accepted"</param>
        /// <returns></returns>
        public HL7Message getAck(String ackMessage = null)
        {
            if (this.ack != null)
            {
                return this.ack;
            }

            MSH receivedMSH = this.getMSH();

            HL7Message ack = new HL7Message();

            MSH ackMSH = MSH.buildDefaultMSH();
            ackMSH.messageControlId = HL7Utils.getUniqueMessageControlId(); 
            ackMSH.receivingApplication = receivedMSH.sendingApplication;
            ackMSH.receivingFacility = receivedMSH.sendingFacility;
            ackMSH.versionId = receivedMSH.versionId;
            ackMSH.messageType = "ACK";

            MSA ackMSA = new MSA();
            ackMSA.acknowledgementCode = "AA";
            ackMSA.messageControlId = receivedMSH.messageControlId;
            ackMSA.textMessage = (String.IsNullOrEmpty(ackMessage) ? "Message accepted" : ackMessage);

            ack.segments = new List<HL7Segment>();
            ack.segments.Add(ackMSH);
            ack.segments.Add(ackMSA);

            this.ack = ack;
            return ack;
        }

        public HL7Message getNack(String ackMessage = null)
        {
            MSH receivedMSH = this.getMSH();

            HL7Message nack = new HL7Message();

            MSH ackMSH = MSH.buildDefaultMSH();
            ackMSH.messageControlId = CryptographyUtils.getNCharRandom(8);
            ackMSH.receivingApplication = receivedMSH.sendingApplication;
            ackMSH.receivingFacility = receivedMSH.sendingFacility;
            ackMSH.versionId = receivedMSH.versionId;
            ackMSH.messageType = "ACK";

            MSA ackMSA = new MSA();
            ackMSA.acknowledgementCode = "AR";
            ackMSA.messageControlId = receivedMSH.messageControlId;
            ackMSA.textMessage = (String.IsNullOrEmpty(ackMessage) ? "Message rejected" : ackMessage);

            nack.segments = new List<HL7Segment>();
            nack.segments.Add(ackMSH);
            nack.segments.Add(ackMSA);

            return nack;
        }

        public String toEncodedMessage()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(SOT_CHAR);
            foreach (HL7Segment seg in this.segments)
            {
                sb.Append(seg.toEncodedString(this));
                sb.Append(SEGMENT_SEPARATOR_CHAR);
            }
            sb.Remove(sb.Length - 1, 1); // remove last segment separator
            sb.Append(EOT_CHAR);
            sb.Append(EOT_NM_CHAR);
            return sb.ToString();
        }
    }

    public abstract class AbstractHL7Segment
    {
        internal abstract void parse(String rawSegment, HL7Message parentMessage);

        public abstract string toEncodedString(HL7Message parentMessage);
    }

    [Serializable]
    public class HL7Segment : AbstractHL7Segment
    {
        public String raw;
        public String segmentId;
        public Dictionary<Int32, String> segmentPiecesByIndex;

        public HL7Segment() { } 

        public HL7Segment(String rawSegment)
        {
            this.raw = rawSegment;
            parseSegmentId(rawSegment);
        }

        /// <summary>
        /// Minimal parsing of message - breaks segment up in to fields and stores the in dictionary by 0-based index
        /// </summary>
        /// <param name="rawSegment"></param>
        /// <param name="parentMessage"></param>
        internal override void parse(string rawSegment, HL7Message parentMessage)
        {
            this.raw = rawSegment;
            parseSegmentId(rawSegment);
            segmentPiecesByIndex = new Dictionary<Int32, String>();

            String[] pieces = StringUtils.split(rawSegment, parentMessage.fieldSeparator);
            for (int i = 0; i< pieces.Length; i++)
            {
                if (String.IsNullOrEmpty(pieces[i]))
                {
                    continue;
                }

                segmentPiecesByIndex.Add(i, pieces[i]);
            }
        }

        public override string toEncodedString(HL7Message parentMessage)
        {
            return this.raw;
        }

        void parseSegmentId(String rawSegment)
        {
            if (String.IsNullOrEmpty(rawSegment) || rawSegment.Length < 3)
            {
                throw new HL7Exception("Invalid segment - unable to parse Segment ID");
            }

            this.segmentId = rawSegment.Substring(0, 3); // segment is always the first 3 chars
        }

    }

    /// <summary>
    /// HL7 Message Header Segment
    /// </summary>
    [Serializable]
    public class MSH : HL7Segment
    {
        const String defaultEncodingChars = "^~\\&";
        const char defaultFieldSeperator = '|';

        public char fieldSeparator;
        public String encodingCharacters;
        public char componentSeparator;
        public char repetitionSeparator;
        public char escapeCharacter;
        public char subcomponentSeparator;

        public String sendingApplication;
        public String sendingFacility;
        public String receivingApplication;
        public String receivingFacility;
        public String dateTimeOfMessage;
        /// <summary>
        /// MSH-9 - e.g. ORU^R01
        /// </summary>
        public String messageType;
        // MSH-10 - unique ID for the message
        public String messageControlId;
        /// <summary>
        /// D - debugging
        /// P - production
        /// T - training
        /// </summary>
        public String processingId;
        /// <summary>
        /// e.g. 2.3
        /// </summary>
        public String versionId;

        public MSH()
        {
            this.segmentId = "MSH";
        }

        public MSH(String rawMSH)
        {
            this.segmentId = "MSH";
            this.raw = rawMSH;
        }

        /// <summary>
        /// Parse the MSH segment. NOTE: this parse API is special in that it sets the encoding characters on the parent message 
        /// </summary>
        /// <param name="rawMSH"></param>
        /// <param name="parentMessage"></param>
        internal override void parse(string rawMSH, HL7Message parentMessage)
        {
            this.raw = rawMSH;

            this.fieldSeparator = rawMSH[3]; // field separator is always 4th char - almost always a pipe ('|') char but should support different characters
            parentMessage.fieldSeparator = rawMSH[3];

            String[] pieces = StringUtils.split(rawMSH, this.fieldSeparator);
            if (pieces == null || pieces.Length < 12)
            {
                throw new HL7Exception("Invalid MSH segment - unable to parse");
            }

            if (String.IsNullOrEmpty(pieces[1]) || pieces[1].Length != 4)
            {
                throw new HL7Exception("Invalid MSH-2. Unable to parse control characters");
            }
            this.encodingCharacters = pieces[1];
            parentMessage.encodingCharacters = pieces[1];
            this.componentSeparator = pieces[1][0];
            parentMessage.componentSeparator = pieces[1][0];
            this.repetitionSeparator = pieces[1][1];
            parentMessage.repetitionSeparator = pieces[1][1];
            this.escapeCharacter = pieces[1][2];
            parentMessage.escapeCharacter = pieces[1][2];
            this.subcomponentSeparator = pieces[1][3];
            parentMessage.subcomponentSeparator = pieces[1][3];

            this.sendingApplication = pieces[2];
            this.sendingFacility = pieces[3];
            this.receivingApplication = pieces[4];
            this.receivingFacility = pieces[5];
            this.dateTimeOfMessage = pieces[6];
            this.messageType = pieces[8];
            this.messageControlId = pieces[9];
            this.processingId = pieces[10];
            this.versionId = pieces[11];
        }

        public override string toEncodedString(HL7Message parentMessage)
        {
            return "MSH" + parentMessage.fieldSeparator
                + this.encodingCharacters + parentMessage.fieldSeparator
                + this.sendingApplication + parentMessage.fieldSeparator
                + this.sendingFacility + parentMessage.fieldSeparator
                + this.receivingApplication + parentMessage.fieldSeparator
                + this.receivingFacility + parentMessage.fieldSeparator
                + this.dateTimeOfMessage + parentMessage.fieldSeparator + parentMessage.fieldSeparator
                + this.messageType + parentMessage.fieldSeparator
                + this.messageControlId + parentMessage.fieldSeparator
                + this.processingId + parentMessage.fieldSeparator
                + this.versionId;
        }

        internal static MSH buildDefaultMSH()
        {
            MSH response = new MSH();

            response.dateTimeOfMessage = DateUtils.toIsoString(DateTime.UtcNow, true, true);
            response.encodingCharacters = MSH.defaultEncodingChars;
            response.sendingApplication = MyConfigurationManager.getValue("PG_HL7_v2_SendingApplicationName");
            response.sendingFacility = MyConfigurationManager.getValue("PG_HL7_v2_SendingFacilityName");
            response.versionId = MyConfigurationManager.getValue("PG_HL7_v2_VersionId");

            return response;
        }
    }

    /// <summary>
    /// HL7 Message Acknoledgement Segment
    /// </summary>
    [Serializable]
    public class MSA : HL7Segment
    {
        public String acknowledgementCode;
        public String messageControlId;
        public String textMessage;

        public MSA()
        {
            this.segmentId = "MSA";
        }

        public MSA(String rawMSA)
        {
            this.segmentId = "MSA";
            this.raw = rawMSA;
        }

        internal override void parse(String rawMSA, HL7Message parentMessage)
        {
            this.raw = rawMSA;

            String[] pieces = StringUtils.split(rawMSA, parentMessage.fieldSeparator);
            if (pieces == null || pieces.Length < 2)
            {
                throw new HL7Exception("Invalid MSA segment - unable to parse");
            }

            this.acknowledgementCode = pieces[1];
            this.messageControlId = pieces[2];
            if (pieces.Length > 3)
            {
                this.textMessage = pieces[3];
            }
        }

        public override string toEncodedString(HL7Message parentMessage)
        {
            return "MSA" + parentMessage.fieldSeparator + this.acknowledgementCode + parentMessage.fieldSeparator + this.messageControlId + parentMessage.fieldSeparator + this.textMessage;
        }
    }
}