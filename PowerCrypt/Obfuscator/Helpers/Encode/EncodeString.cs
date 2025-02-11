using PowerCrypt.Obfuscator.Methods.CommandTypeAndBareWordObfuscation;
using System.Text;

namespace PowerCrypt.Obfuscator.Helpers.Encode
{
    public class EncodeString
    {
        public static string Encode(string str)
        {
            string base64Encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            string iexCommandObfuscated = CommandOBF.ObfuscateCommand("Invoke-Expression");

            return $"{iexCommandObfuscated}([System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{base64Encoded}')))";
        }
    }
}
