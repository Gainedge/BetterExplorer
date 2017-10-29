using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace wyUpdate.Common
{
    public static partial class WriteFiles
    {
        public static void WriteInt(Stream fs, byte flag, int num)
        {
            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //write the size of the data (Integer = 4bytes)
            fs.Write(BitConverter.GetBytes(4), 0, 4);

            //write the integer data
            fs.Write(BitConverter.GetBytes(num), 0, 4);
        }

        public static void WriteBool(Stream fs, byte flag, bool val)
        {
            WriteInt(fs, flag, val ? 1 : 0);
        }

        public static void WriteLong(Stream fs, byte flag, long val)
        {
            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //write the size of the data (Long = 8bytes)
            fs.Write(BitConverter.GetBytes(8), 0, 4);

            //write the integer data
            fs.Write(BitConverter.GetBytes(val), 0, 8);
        }

        public static void WriteShort(Stream fs, byte flag, short val)
        {
            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //write the size of the data (Long = 2bytes)
            fs.Write(BitConverter.GetBytes(2), 0, 4);

            // write the integer data
            fs.Write(BitConverter.GetBytes(val), 0, 2);
        }

        public static void WriteDateTime(Stream fs, byte flag, DateTime dt)
        {
            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //write the size of the data (6 Integers at 4 bytes each = 24bytes)
            fs.Write(BitConverter.GetBytes(24), 0, 4);

            //write the DateTime data

            //Year
            fs.Write(BitConverter.GetBytes(dt.Year), 0, 4);
            //Month
            fs.Write(BitConverter.GetBytes(dt.Month), 0, 4);
            //Day
            fs.Write(BitConverter.GetBytes(dt.Day), 0, 4);
            //Hour
            fs.Write(BitConverter.GetBytes(dt.Hour), 0, 4);
            //Minute
            fs.Write(BitConverter.GetBytes(dt.Minute), 0, 4);
            //Second
            fs.Write(BitConverter.GetBytes(dt.Second), 0, 4);
        }

        public static void WriteString(Stream fs, byte flag, string text)
        {
            if (text == null)
                text = String.Empty;//fill with an empty string

            //the string data to be written
            byte[] tempBytes = Encoding.UTF8.GetBytes(text);

            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //the byte-length of the string. 
            fs.Write(BitConverter.GetBytes(tempBytes.Length), 0, 4);

            //write the string data
            fs.Write(tempBytes, 0, tempBytes.Length);
        }

        public static void WriteDeprecatedString(Stream fs, byte flag, string text)
        {
            if (text == null)
                text = String.Empty;//fill with an empty string

            //the string data to be written
            byte[] tempBytes = Encoding.UTF8.GetBytes(text);
            
            //the byte-length of the string. 
            //(Previously I used the string-length, this caused problems for non-bytelong characters)
            byte[] tempLength = BitConverter.GetBytes(tempBytes.Length);

            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //FIX: I'm storing both the size of the bytes *Twice*. It's stupid.

            //write the size of the data
            fs.Write(BitConverter.GetBytes(tempBytes.Length + 4), 0, 4);

            //write the string data
            fs.Write(tempLength, 0, 4);
            fs.Write(tempBytes, 0, tempBytes.Length);
        }

        public static void WriteByteArray(Stream fs, byte flag, byte[] arr)
        {
            byte[] tempLength = BitConverter.GetBytes(arr.Length);

            //write the flag (e.g. 0x01, 0xFF, etc.)
            fs.WriteByte(flag);

            //write the byte data
            fs.Write(tempLength, 0, 4);
            fs.Write(arr, 0, arr.Length);
        }

        public static void WriteHeader(Stream fs, string Header)
        {
            byte[] arr = Encoding.UTF8.GetBytes(Header);
            fs.Write(arr, 0, arr.Length);
        }
    }

    public static partial class ReadFiles
    {
        public static DateTime ReadDateTime(Stream fs)
        {
            //skip the "length of data" int value
            fs.Position += 4;

            byte[] tempBytes = new byte[4];

            //Year
            ReadWholeArray(fs, tempBytes);
            int year = BitConverter.ToInt32(tempBytes, 0);

            //Month
            ReadWholeArray(fs, tempBytes);
            int month = BitConverter.ToInt32(tempBytes, 0);

            //Day
            ReadWholeArray(fs, tempBytes);
            int day = BitConverter.ToInt32(tempBytes, 0);

            //Hour
            ReadWholeArray(fs, tempBytes);
            int hour = BitConverter.ToInt32(tempBytes, 0);

            //Minute
            ReadWholeArray(fs, tempBytes);
            int minute = BitConverter.ToInt32(tempBytes, 0);

            //Second
            ReadWholeArray(fs, tempBytes);
            int second = BitConverter.ToInt32(tempBytes, 0);

            return new DateTime(year, month, day, hour, minute, second);
        }

        public static string ReadString(Stream fs)
        {
            byte[] tempLength = new byte[4];

            ReadWholeArray(fs, tempLength);
            byte[] tempBytes = new byte[BitConverter.ToInt32(tempLength, 0)];
            ReadWholeArray(fs, tempBytes);

            return Encoding.UTF8.GetString(tempBytes);
        }

        public static string ReadDeprecatedString(Stream fs)
        {
            int length = ReadInt(fs);

            byte[] tempBytes = new byte[length];
            ReadWholeArray(fs, tempBytes);
            
            return Encoding.UTF8.GetString(tempBytes);
        }

        public static byte[] ReadByteArray(Stream fs)
        {
            byte[] tempLength = new byte[4];

            ReadWholeArray(fs, tempLength);
            byte[] tempBytes = new byte[BitConverter.ToInt32(tempLength, 0)];
            ReadWholeArray(fs, tempBytes);

            return tempBytes;
        }

        public static int ReadInt(Stream fs)
        {
            byte[] tempBytes = new byte[4];

            //skip the "length of data" int value
            fs.Position += 4;

            ReadWholeArray(fs, tempBytes);
            return BitConverter.ToInt32(tempBytes, 0);
        }

        public static bool ReadBool(Stream fs)
        {
            return ReadInt(fs) == 1;
        }

        //Long (i.e. int64)
        public static long ReadLong(Stream fs)
        {
            byte[] tempBytes = new byte[8];

            //skip the "length of data" int value
            fs.Position += 4;

            ReadWholeArray(fs, tempBytes);
            return BitConverter.ToInt64(tempBytes, 0);
        }

        public static short ReadShort(Stream fs)
        {
            byte[] tempBytes = new byte[2];

            //skip the "length of data" int value
            fs.Position += 4;

            ReadWholeArray(fs, tempBytes);
            return BitConverter.ToInt16(tempBytes, 0);
        }

        public static bool IsHeaderValid(Stream fs, string HeaderShouldBe)
        {
            // NOTE: this assumes that 1 byte = 1 character.
            // This is only true for alphanumeic characters.
            // I.e. don't use this function if you don't know what I'm talking about.

            byte[] fileIDBytes = new byte[HeaderShouldBe.Length];

            // Read back the file identification data, if any
            fs.Read(fileIDBytes, 0, fileIDBytes.Length);
            string fileID = Encoding.UTF8.GetString(fileIDBytes);

            return fileID == HeaderShouldBe;
        }


        public static bool ReachedEndByte(Stream fs, byte endByte, byte readValue)
        {
            if (endByte == readValue)
                return true;
            
            if (fs.Length == fs.Position)
                //prevent infinite loops because the end of the file has been reached
                //but the 'end byte' hasn't been detected.
                throw new Exception("Premature end of file.");
            
            return false;
        }

        //Unknown data, skip it
        public static void SkipField(Stream fs, byte flag)
        {
            //if it's not an 'identifier byte' skip the
            //skip the unknown data
            if (!(flag >= 0x80 && flag <= 0x9F))
            {
                byte[] tempBytes = new byte[4];

                fs.Read(tempBytes, 0, 4);

                //skip the number of bytes
                fs.Position += BitConverter.ToInt32(tempBytes, 0);
            }
        }

        public static void ReadWholeArray(Stream stream, byte[] data)
        {
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read = stream.Read(data, offset, remaining);
                if (read <= 0)
                    throw new EndOfStreamException
                        (String.Format(CultureInfo.CurrentCulture, "End of stream reached with {0} bytes left to read", remaining));
                remaining -= read;
                offset += read;
            }
        }
    }
}
