using System.Text;

namespace PowerCrypt.Obfuscator.Methods.StringObfuscation
{
    public class StringOBFStringConstantExpressionAstUnderMember
    {
        private static readonly string GoodChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string ObfuscateString(string str)
        {
            //TODO: FIX
            //return str;

            var stringBuilder = new StringBuilder();

            // for each character in the string, if the character is capital letter or number, then add a ` before it.
            foreach (var c in str)
            {
                if (GoodChars.Contains(c))
                {
                    stringBuilder.Append('`');
                }
                stringBuilder.Append(c);
            }

            var outString = '"' + stringBuilder.ToString() + '"';

            return outString;
        }
    }
}
