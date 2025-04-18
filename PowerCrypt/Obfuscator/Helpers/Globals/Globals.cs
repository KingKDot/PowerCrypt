using System.Net.NetworkInformation;

namespace PowerCrypt.Obfuscator.Helpers.globals
{
    public static class Globals
    {
        public static string CompressFunctionName { get; set; } = string.Empty;

        public static HashSet<string> FunctionNamesToSkip { get; set; } = new HashSet<string>
               {
                   "CheckValidationResult"
               };

        public static List<string> UsedVariables { get; set; } = new List<string>();
        public static List<string> UsedFunctions { get; set; } = new List<string>();

        public static void AddUsedVariable(string variableName)
        {
            if (!UsedVariables.Contains(variableName))
            {
                UsedVariables.Add(variableName);
            }
        }

        public static void AddUsedFunction(string functionName)
        {
            if (!UsedFunctions.Contains(functionName))
            {
                UsedFunctions.Add(functionName);
            }
        }
    }
}