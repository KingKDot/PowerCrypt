using PowerCrypt.Obfuscator.Helpers.globals;
using PowerCrypt.Obfuscator.Methods.RandomHelper;

namespace PowerCrypt.Obfuscator.Methods.FunctionObfuscation
{
    public class FunctionOBF
    {
        public static string ObfuscateFunctionSimple(string ObfuscateFunction)
        {
            string randomString = Helper.GetRandomString(10);
            string randomLetter = Helper.GetRandomString(1, true);

            randomString = randomLetter + randomString;

            if (ObfuscateFunction == Globals.CompressFunctionName)
            {
                Console.WriteLine("Function name is the same as the compress function name, changing it to a random string");
                Globals.CompressFunctionName = randomString;
            }

            foreach (var function in Globals.FunctionNamesToSkip)
            {
                if (function == ObfuscateFunction)
                {
                    Console.WriteLine("Function name is in the skip list, changing it to a random string");
                    return ObfuscateFunction;
                }
            }

            //add backticks to captial letters
            var modifiedString = new System.Text.StringBuilder();
            foreach (char c in randomString)
            {
                if (char.IsUpper(c))
                {
                    modifiedString.Append('`').Append(c);
                }
                else
                {
                    modifiedString.Append(c);
                }
            }

            return modifiedString.ToString();
        }
    }
}
