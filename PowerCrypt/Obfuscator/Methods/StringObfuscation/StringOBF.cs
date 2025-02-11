using System.Text;

namespace PowerCrypt.Obfuscator.Methods.StringObfuscation
{
    public class StringOBF
    {
        public static string ObfuscateString(string input, string quoteType)
        {

            if (input.Length < 3)
            {
                return input;
            }

            string inputTrimQuotes = input.Substring(1, input.Length - 2);

            var methods = new Func<string, string>[] {
                    ObfuscateStringBase64,
                    ObfuscateHexString,
                    ObfuscateMixedString,
                    ObfuscateByteArrayString,
                };

            var split = SplitStrings(inputTrimQuotes);

            var resultStringBuilder = new StringBuilder();
            foreach (var res in split)
            {
                var method = methods[new Random().Next(0, methods.Length)];
                resultStringBuilder.Append(method(res) + "+");
            }

            // we don't like trailing " + "
            var result = resultStringBuilder.ToString();
            if (result.EndsWith('+'))
            {
                result = result.Substring(0, result.Length - 1);
            }

            return $"({result})";
        }

        private static string ObfuscateStringBase64(string input)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
            string command = $"[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String('{base64}'))";
            return $"({command})";
        }

        private static string ObfuscateHexString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string hexString = string.Join(",", bytes.Select(b => $"0x{b:x2}"));
            string command = $"[System.Text.Encoding]::UTF8.GetString(([byte[]]({hexString})))";
            return $"({command})";
        }

        private static string ObfuscateByteArrayString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string byteArrayString = "(" + string.Join(",", bytes) + ")";
            string command = $"[System.Text.Encoding]::UTF8.GetString([byte[]]{byteArrayString})";
            return $"({command})";
        }

        private static string ObfuscateMixedString(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            string[] hexArray = bytes.Select(b => $"0x{b:x2}").ToArray();

            string byteArrayString;
            if (bytes.Length == 1)
            {
                byteArrayString = bytes[0].ToString();
            }
            else
            {
                var random = new Random();
                var mixedArray = bytes.Select((b, i) => random.Next(0, 2) == 0 ? b.ToString() : hexArray[i]).ToArray();
                byteArrayString = "(" + string.Join(",", mixedArray) + ")";
            }

            string command = $"[System.Text.Encoding]::UTF8.GetString([byte[]]{byteArrayString})";
            return $"({command})";
        }

        public static string[] SplitStrings(string input)
        {
            int stringLength = input.Length;
            if (stringLength < 2)
            {
                return [input];
            }

            var result = new List<string>();
            int i = 0;

            Random random = new Random();

            while (i < stringLength)
            {
                int chunkLength;
                if (stringLength < 10)
                {
                    chunkLength = random.Next(2, 5);
                }
                else if (stringLength < 50)
                {
                    chunkLength = random.Next(15, 25);
                }
                else if (stringLength < 100)
                {
                    chunkLength = random.Next(24, 50);
                }
                else if (stringLength < 200)
                {
                    chunkLength = random.Next(50, 100);
                }
                else if (stringLength < 500)
                {
                    chunkLength = random.Next(75, 200);
                }
                else if (stringLength < 1000)
                {
                    chunkLength = random.Next(100, 300);
                }
                else
                {
                    chunkLength = random.Next(200, 500);
                }

                int length = Math.Min(chunkLength, stringLength - i);
                result.Add(input.Substring(i, length));
                i += length;
            }

            return result.ToArray();
        }
    }
}
