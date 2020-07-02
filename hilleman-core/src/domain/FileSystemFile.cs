using com.bitscopic.hilleman.core.utils;
using System;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class FileSystemFile
    {
        public DateTime created;
        public String fileName;
        public Int32 size;
        public byte[] data;

        public FileSystemFile() { }

        public FileSystemFile(String fileName, byte[] data)
        {
            this.fileName = fileName;
            this.size = data.Length;
            this.data = data;
            this.created = DateTime.Now;
        }
    }

    public class FileSystemTextFile : FileSystemFile
    {
        public new String data;

        public FileSystemTextFile() : base() { }

        public FileSystemTextFile(String fileName, byte[] data) : base(fileName, data)
        {
            this.data = System.Text.Encoding.ASCII.GetString(data);
        }
    }

    [Serializable]
    public class SerializedVersionedFileSystemTextFile : SerializedVersionedNamespacedObject
    {
        public DateTime created;
        public String fileName;
        public Int32 size;
        public String data;

        public SerializedVersionedFileSystemTextFile() { /* parameterless constructor */ }

        public SerializedVersionedFileSystemTextFile(FileSystemTextFile file)
        {
            this.created = file.created;
            this.data = file.data;
            this.fileName = file.fileName;
            this.size = file.size;
        }

        public override object deserialize()
        {
            if (String.IsNullOrEmpty(base.serializedValue))
            {
                throw new ArgumentNullException("No serialized data found");
            }
            return SerializerUtils.deserialize<SerializedVersionedFileSystemTextFile>(base.serializedValue);
        }
    }
}