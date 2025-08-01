using System.Text.Json;

namespace PowerCrypt.Settings
{
    public class AppSettings
    {
        // Debug and Output Settings
        public static bool PrintToScreen { get; set; } = false;

        // Pre-Processing Settings
        public static bool EnablePrePass { get; set; } = true;
        public static bool InsertDecryptFunction { get; set; } = true;
        public static string DecryptFunctionName { get; set; } = "DecodeAndDecompressString";
        public static bool ProcessExpandableStrings { get; set; } = true;
        public static bool RemoveComments { get; set; } = true;
        public static bool RemoveEmptyLines { get; set; } = true;

        // Function Obfuscation Settings
        public static bool ObfuscateFunctions { get; set; } = true;
        public static HashSet<string> FunctionNamesToIgnore { get; set; } = new HashSet<string>
        {
            "CheckValidationResult"
        };

        // Variable Obfuscation Settings
        public static bool ObfuscateVariables { get; set; } = true;
        public static HashSet<string> VariableNamesToIgnore { get; set; } = new HashSet<string>
        {
            "ErrorActionPreference",
            "VerbosePreference",
            "WarningPreference",
            "InformationPreference",
            "DebugPreference"
        };

        // String Obfuscation Settings
        public static bool ObfuscateStrings { get; set; } = true;
        public static int MinStringLengthToObfuscate { get; set; } = 3;
        public static HashSet<string> StringsToIgnore { get; set; } = new HashSet<string>();

        // Number Obfuscation Settings
        public static bool ObfuscateNumbers { get; set; } = true;
        public static HashSet<int> NumbersToIgnore { get; set; } = new HashSet<int> { 0, 1 };

        // Command Obfuscation Settings
        public static bool ObfuscateCommands { get; set; } = true;
        public static bool ObfuscateBuiltInCommands { get; set; } = true;
        public static bool ObfuscateBareWords { get; set; } = true;
        public static bool ObfuscateCommonBareWords { get; set; } = true;

        // Control Flow Obfuscation Settings
        public static bool EnableControlFlowObfuscation { get; set; } = true;
        public static bool WrapWithControlFlow { get; set; } = true;

        // Advanced Obfuscation Settings
        public static bool EnableMixedBooleanArithmetic { get; set; } = true;
    }
}
