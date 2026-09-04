namespace Quanto.Offline
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class DataTableCustomFormatter
    {
        public static byte[] Serialize(DataTable dataTable, bool enableCompression = true)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
                {
                    SystemStringBinaryWriterDelegate(binaryWriter, dataTable.TableName);

                    var binaryWriterDelegates = new List<Action<BinaryWriter, object>>();

                    int numberOfColumns = dataTable.Columns.Count;

                    binaryWriter.Write(numberOfColumns);

                    string dataTypeAsString;

                    foreach (DataColumn dataColumm in dataTable.Columns)
                    {
                        dataTypeAsString = dataColumm.DataType.ToString();
                        binaryWriter.Write(dataColumm.ColumnName);
                        binaryWriter.Write(dataTypeAsString);

                        switch (dataTypeAsString)
                        {
                            case "System.Boolean": binaryWriterDelegates.Add(SystemBooleanBinaryWriterDelegate); break;
                            case "System.Byte": binaryWriterDelegates.Add(SystemByteBinaryWriterDelegate); break;
                            case "System.Byte[]": binaryWriterDelegates.Add(SystemByteArrayBinaryWriterDelegate); break;
                            case "System.Char": binaryWriterDelegates.Add(SystemCharBinaryWriterDelegate); break;
                            case "System.DateTime": binaryWriterDelegates.Add(SystemDateTimeBinaryWriterDelegate); break;
                            case "System.Decimal": binaryWriterDelegates.Add(SystemDecimalBinaryWriterDelegate); break;
                            case "System.Double": binaryWriterDelegates.Add(SystemDoubleBinaryWriterDelegate); break;
                            case "System.Guid": binaryWriterDelegates.Add(SystemGuidBinaryWriterDelegate); break;
                            case "System.Int16": binaryWriterDelegates.Add(SystemInt16BinaryWriterDelegate); break;
                            case "System.Int32": binaryWriterDelegates.Add(SystemInt32BinaryWriterDelegate); break;
                            case "System.Int64": binaryWriterDelegates.Add(SystemInt64BinaryWriterDelegate); break;
                            case "System.SByte": binaryWriterDelegates.Add(SystemSByteBinaryWriterDelegate); break;
                            case "System.Single": binaryWriterDelegates.Add(SystemSingleBinaryWriterDelegate); break;
                            case "System.String": binaryWriterDelegates.Add(SystemStringBinaryWriterDelegate); break;
                            case "System.TimeSpan": binaryWriterDelegates.Add(SystemTimeSpanBinaryWriterDelegate); break;
                            case "System.UInt16": binaryWriterDelegates.Add(SystemUInt16BinaryWriterDelegate); break;
                            case "System.UInt32": binaryWriterDelegates.Add(SystemUInt32BinaryWriterDelegate); break;
                            case "System.UInt64": binaryWriterDelegates.Add(SystemUInt64BinaryWriterDelegate); break;
                            case "NpgsqlDbType.TsVector": binaryWriterDelegates.Add(SystemVectorBinaryWriterDelegate); break;
                            default:
                                binaryWriterDelegates.Add(SystemNullWriterDelegate); break;
                                // throw new ArgumentOutOfRangeException("The \"" + dataTypeAsString + "\" type is not supported by this formatter during the serialization of a DataTable object.");
                        }
                    }

                    int numberOfRows = dataTable.Rows.Count;

                    binaryWriter.Write(numberOfRows);

                    int i;

                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        for (i = 0; i < numberOfColumns; i++)
                        {
                            binaryWriterDelegates[i](binaryWriter, dataRow[i]);
                        }
                    }
                }

                return enableCompression ? BufferedRealtimeCompressionEngine.Compress(memoryStream.ToArray()) : memoryStream.ToArray();
            }
        }

        public static DataTable Deserialize(byte[] inputData, bool enableCompression = true)
        {
            DataTable dataTable = new DataTable();

            var outputData = enableCompression ? BufferedRealtimeCompressionEngine.Decompress(inputData) : inputData; using (var memoryStream = new MemoryStream(outputData))
            {
                using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    dataTable.TableName = (string)SystemStringBinaryReaderDelegate(binaryReader);

                    dataTable.BeginLoadData();

                    var binaryReaderDelegates = new List<Func<BinaryReader, object>>();

                    int numberOfColumns = binaryReader.ReadInt32();

                    string columnName;
                    string dataTypeAsString;

                    int i;

                    for (i = 0; i < numberOfColumns; i++)
                    {
                        columnName = binaryReader.ReadString();
                        dataTypeAsString = binaryReader.ReadString();
                        if (dataTypeAsString == "NpgsqlTypes.NpgsqlTsVector")
                        {
                            dataTable.Columns.Add(columnName, typeof(NpgsqlTypes.NpgsqlTsVector));
                        }
                        else
                        {
                            dataTable.Columns.Add(columnName, Type.GetType(dataTypeAsString));
                        }
                        switch (dataTypeAsString)
                        {
                            case "System.Boolean": binaryReaderDelegates.Add(SystemBooleanBinaryReaderDelegate); break;
                            case "System.Byte": binaryReaderDelegates.Add(SystemByteBinaryReaderDelegate); break;
                            case "System.Byte[]": binaryReaderDelegates.Add(SystemByteArrayBinaryReaderDelegate); break;
                            case "System.Char": binaryReaderDelegates.Add(SystemCharBinaryReaderDelegate); break;
                            case "System.DateTime": binaryReaderDelegates.Add(SystemDateTimeBinaryReaderDelegate); break;
                            case "System.Decimal": binaryReaderDelegates.Add(SystemDecimalBinaryReaderDelegate); break;
                            case "System.Double": binaryReaderDelegates.Add(SystemDoubleBinaryReaderDelegate); break;
                            case "System.Guid": binaryReaderDelegates.Add(SystemGuidBinaryReaderDelegate); break;
                            case "System.Int16": binaryReaderDelegates.Add(SystemInt16BinaryReaderDelegate); break;
                            case "System.Int32": binaryReaderDelegates.Add(SystemInt32BinaryReaderDelegate); break;
                            case "System.Int64": binaryReaderDelegates.Add(SystemInt64BinaryReaderDelegate); break;
                            case "System.SByte": binaryReaderDelegates.Add(SystemSByteBinaryReaderDelegate); break;
                            case "System.Single": binaryReaderDelegates.Add(SystemSingleBinaryReaderDelegate); break;
                            case "System.String": binaryReaderDelegates.Add(SystemStringBinaryReaderDelegate); break;
                            case "NpgsqlDbType.TsVector": binaryReaderDelegates.Add(SystemVectorBinaryReaderDelegate); break;
                            case "System.TimeSpan": binaryReaderDelegates.Add(SystemTimeSpanBinaryReaderDelegate); break;
                            case "System.UInt16": binaryReaderDelegates.Add(SystemUInt16BinaryReaderDelegate); break;
                            case "System.UInt32": binaryReaderDelegates.Add(SystemUInt32BinaryReaderDelegate); break;
                            case "System.UInt64": binaryReaderDelegates.Add(SystemUInt64BinaryReaderDelegate); break;
                            default:
                                binaryReaderDelegates.Add(SystemNullReaderDelegate); break;
                                //throw new ArgumentOutOfRangeException("The \"" + dataTypeAsString + "\" type is not supported by this formatter during the deserialization of a DataTable object.");
                        }
                    }

                    int numberOfRows = binaryReader.ReadInt32();

                    object[] itemArray = new object[numberOfColumns];

                    int j;

                    for (i = 0; i < numberOfRows; i++)
                    {
                        for (j = 0; j < numberOfColumns; j++)
                        {
                            itemArray[j] = binaryReaderDelegates[j](binaryReader);
                        }

                        dataTable.Rows.Add(itemArray);
                    }

                    dataTable.EndLoadData();
                }
            }

            return dataTable;
        }

        private const byte STANDARD_VALUE_TYPE_MARKER = 0;
        private const byte DBNULL_VALUE_TYPE_MARKER = 1;
        private const byte NULL_VALUE_TYPE_MARKER = 2;

        private static void SystemBooleanBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((bool)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemByteBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((byte)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemByteArrayBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                var byteArray = (byte[])value; int byteArrayLength = byteArray.Length; binaryWriter.Write(byteArrayLength); if (byteArrayLength > 0) binaryWriter.Write(byteArray);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemCharBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((char)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemDecimalBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((decimal)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemDateTimeBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write(((DateTime)value).Ticks);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemDoubleBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((double)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemGuidBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write(((Guid)value).ToByteArray());

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemInt16BinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((short)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemInt32BinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((int)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemInt64BinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((long)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemSByteBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((sbyte)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemSingleBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((float)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        public static string CreatTSVector(DataTable dt)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (DataRow dr in dt.Rows)
                stringBuilder.Append((stringBuilder.Length > 0 ? " " : "") + dr[0].ToString());

            return Regex.Replace(stringBuilder.ToString(), @"[^\w\s]", "", RegexOptions.Compiled);
        }

        private static void SystemNullWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);
        }

        private static void SystemVectorBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);
                binaryWriter.Write(Newtonsoft.Json.JsonConvert.SerializeObject(value as DataTable));
                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemStringBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((string)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemTimeSpanBinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write(((TimeSpan)value).Ticks);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemUInt16BinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((ushort)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemUInt32BinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((uint)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static void SystemUInt64BinaryWriterDelegate(BinaryWriter binaryWriter, object value)
        {
            if (value != null && !(value is DBNull))
            {
                binaryWriter.Write(STANDARD_VALUE_TYPE_MARKER);

                binaryWriter.Write((ulong)value);

                return;
            }

            if (value is DBNull)
            {
                binaryWriter.Write(DBNULL_VALUE_TYPE_MARKER);

                return;
            }

            binaryWriter.Write(NULL_VALUE_TYPE_MARKER);
        }

        private static object SystemBooleanBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadBoolean();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemByteBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadByte();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemByteArrayBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                var byteArrayLength = binaryReader.ReadInt32(); return binaryReader.ReadBytes(byteArrayLength);
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemCharBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadChar();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemDecimalBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadDecimal();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemDateTimeBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return new DateTime(binaryReader.ReadInt64());
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemDoubleBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadDouble();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemGuidBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return new Guid(binaryReader.ReadBytes(16));
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemInt16BinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadInt16();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemInt32BinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadInt32();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemInt64BinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadInt64();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemSByteBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadSByte();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemSingleBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadSingle();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemVectorBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(binaryReader.ReadString());
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemStringBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadString();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemTimeSpanBinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return new TimeSpan(binaryReader.ReadInt64());
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemNullReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();


            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemUInt16BinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadUInt16();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemUInt32BinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadUInt32();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }

        private static object SystemUInt64BinaryReaderDelegate(BinaryReader binaryReader)
        {
            var valueType = binaryReader.ReadByte();

            if (valueType == STANDARD_VALUE_TYPE_MARKER)
            {
                return binaryReader.ReadUInt64();
            }

            if (valueType == DBNULL_VALUE_TYPE_MARKER)
            {
                return DBNull.Value;
            }

            return null;
        }
    }
}