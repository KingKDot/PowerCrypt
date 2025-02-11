using System.IO.Compression;
using System.Text;

namespace PowerCrypt.Obfuscator.Helpers.Compression
{
    public class CompressString
    {
        public static string CompressPowershellCode(string input)
        {
            string compressed = Compress(input);

            return $"iex([System.Text.Encoding]::UTF8.GetString([System.IO.Compression.GZipStream]::new([System.IO.MemoryStream]::new([Convert]::FromBase64String(\"{compressed}\")), [System.IO.Compression.CompressionMode]::Decompress).ToArray()))";
        }

        private static string Decompress(string input)
        {
            byte[] compressed = Convert.FromBase64String(input);
            byte[] decompressed = Decompress(compressed);
            return Encoding.UTF8.GetString(decompressed);
        }

        private static string Compress(string input)
        {
            byte[] encoded = Encoding.UTF8.GetBytes(input);
            byte[] compressed = Compress(encoded);
            return Convert.ToBase64String(compressed);
        }

        private static byte[] Decompress(byte[] input)
        {
            using (var source = new MemoryStream(input))
            {
                byte[] lengthBytes = new byte[4];
                source.Read(lengthBytes, 0, 4);

                var length = BitConverter.ToInt32(lengthBytes, 0);
                using (var decompressionStream = new GZipStream(source,
                    CompressionMode.Decompress))
                {
                    var result = new byte[length];
                    decompressionStream.ReadExactly(result, 0, length);
                    return result;
                }
            }
        }

        private static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }
    }
}
