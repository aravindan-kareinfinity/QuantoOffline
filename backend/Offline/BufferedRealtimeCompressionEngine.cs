namespace InfyPOS.Processors
{
    public static class BufferedRealtimeCompressionEngine
    {
        public static byte[] Compress(byte[] bytes)
        {
            return QuickLZ.compress(bytes);
        }

        public static byte[] Decompress(byte[] bytes)
        {
            return QuickLZ.decompress(bytes);
        }
    }
}