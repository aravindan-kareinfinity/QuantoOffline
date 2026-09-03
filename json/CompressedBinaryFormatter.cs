namespace Quanto.Offline
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public static class CompressedBinaryFormatter
    {
        public static byte[] Serialize(object value)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, value);

                return BufferedRealtimeCompressionEngine.Compress(memoryStream.ToArray());
            }
        }

        public static object Deserialize(byte[] bytes)
        {
            var decompressedBytes = BufferedRealtimeCompressionEngine.Decompress(bytes); using (var memoryStream = new MemoryStream(decompressedBytes))
            {
                return new BinaryFormatter().Deserialize(memoryStream);
            }
        }
    }
}