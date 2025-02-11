using PowerCrypt.Obfuscator.Methods.StaticNumberObfuscation;
using System.Text;

namespace PowerCrypt.Obfuscator.Methods.CommandTypeAndBareWordObfuscation
{
    public class CommandOBF
    {
        public static string ObfuscateCommand(string input)
        {
            return ObfuscateBuiltInCommand(input);
        }

        public static string ObfuscateBuiltInCommand(string input)
        {
            var stringBuilder = new StringBuilder();
            //remove backsticks from input
            input = input.Replace("`", "");
            stringBuilder.Append(".(-join[char[]](");

            var characters = input.ToCharArray();
            var lastIndex = characters.Length - 1;

            for (var i = 0; i < lastIndex; i++)
            {
                int charValue = Convert.ToInt32(characters[i]);
                string numberExpression = NumberOBF.ObfuscateNumber(charValue.ToString());
                stringBuilder.Append($"{numberExpression},");
            }

            if (characters.Length > 0)
            {
                int charValue = Convert.ToInt32(characters[lastIndex]);
                stringBuilder.Append($"{NumberOBF.ObfuscateNumber(charValue.ToString())})");
            }

            stringBuilder.Append(')');
            return stringBuilder.ToString();
        }

        public static string ObfuscateCommonBareWord(string input)
        {
            var outString = new StringBuilder();
            var rand = new Random();
            foreach (var c in input)
            {
                if (rand.Next(0, 2) == 0)
                {
                    outString.Append(char.ToUpper(c));
                }
                else
                {
                    outString.Append(char.ToLower(c));
                }
            }
            return outString.ToString();
        }
    }
}
