using PowerCrypt.Obfuscator.Helpers.globals;
using PowerCrypt.Obfuscator.Methods.CommandTypeAndBareWordObfuscation;
using System.IO.Compression;
using System.Text;

namespace PowerCrypt.Obfuscator.Methods.GeneralControlFlowPostOBF
{
    public class WrapOBF
    {
        public static string ObfuscateWithWrap(string input)
        {
            //return CompressString.CompressPowershellCode(input);
            //return EncodeString.Encode(input);
            return $"({Globals.CompressFunctionName} {CompressAndEncodeString(input)}) | {CommandOBF.ObfuscateCommand("Invoke-Expression")}";
        }

        public static string CompressAndEncodeString(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                throw new ArgumentException("Input string cannot be null or empty.", nameof(inputString));
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }

                byte[] compressedBytes = memoryStream.ToArray();

                return Convert.ToBase64String(compressedBytes);
            }
        }
    }
}
