using System.Text;

namespace PowerCrypt.Obfuscator.Methods.VariableObfuscation
{
    public class VariableOBF
    {
        private static readonly HashSet<string> BadVars = new HashSet<string>
        {
            "$$", "$?", "$^", "$_", "$args", "$ConsoleFileName", "$EnabledExperimentalFeatures", "$Error",
            "$Event", "$EventArgs", "$EventSubscriber", "$ExecutionContext", "$foreach", "$HOME", "$Host",
            "$input", "$IsCoreCLR", "$IsLinux", "$IsMacOS", "$IsWindows", "$LASTEXITCODE", "$Matches",
            "$MyInvocation", "$NestedPromptLevel", "$PID", "$PROFILE", "$PSBoundParameters", "$PSCmdlet",
            "$PSCommandPath", "$PSCulture", "$PSDebugContext", "$PSEdition", "$PSHOME", "$PSItem", "$PSScriptRoot",
            "$PSSenderInfo", "$PSUICulture", "$PSVersionTable", "$PWD", "$Sender", "$ShellId", "$StackTrace",
            "$switch", "$this", "$script", "$kdot_"
        };

        private static readonly string[] choices = new[]
        {
            "[bool][bool]", "[bool][char]", "[bool][int]", "[bool][string]", "[bool][double]",
            "[bool][decimal]", "[bool][byte]", "[bool][timespan]", "[bool][datetime]",
            "(9999 -eq 9999)", "([math]::Round([math]::PI) -eq (4583 - 4580))",
            "[Math]::E -ne [Math]::PI", "[bool](![bool]$null)",
            "!!!![bool][bool][bool][bool][bool][bool]", "![bool]$null", "![bool]$False",
            "[bool][System.Collections.ArrayList]", "[bool][System.Collections.CaseInsensitiveComparer]",
            "[bool][System.Collections.Hashtable]"
        };

        private static readonly HashSet<string> BadStart = new HashSet<string> { "$env:", "$script:", "$kdot_" };
        private static readonly string GoodChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random Random = new();

        public static string ObfuscateVariable(string variable, bool parameter)
        {
            foreach (var badPrefix in BadStart)
            {
                if (variable.StartsWith(badPrefix, StringComparison.Ordinal))
                {
                    return variable;
                }
            }

            if (BadVars.Contains(variable))
            {
                return variable;
            }

            switch (variable)
            {
                case "$true":
                    return ObfuscateTrue();
                case "$false":
                    return ObfuscateFalse();
                case "$null":
                    return ObfuscateNull();
            }

            var randomVarName = MakeRandomVariableName(10);
            var newVarFinal = $"${RandomChangeVar(randomVarName, parameter)}";
            return newVarFinal;
        }


        public static string MakeRandomVariableName(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var sb = new StringBuilder("KDOT");
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[Random.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        public static string RandomCapitalization(string input)
        {
            var result = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                result.Append(Random.Next(2) == 0 ? char.ToUpper(c) : char.ToLower(c));
            }
            return result.ToString();
        }

        public static string RandomChangeVar(string variable, bool parameter)
        {
            var varArray = variable.ToCharArray();
            var builder = new StringBuilder();

            for (int i = 0; i < varArray.Length; i++)
            {
                //skip if the previous character was a tick or the character is not in GoodChars
                if (i > 0 && varArray[i - 1] == '`')
                {
                    continue;
                }

                if (GoodChars.Contains(varArray[i]))
                {
                    var randomCap = RandomCapitalization(varArray[i].ToString())[0];

                    if (!(parameter))
                    {
                        if (char.IsUpper(randomCap) && Random.Next(2) == 0 && Random.Next(5) < 4)
                        {
                            builder.Append('`');
                        }
                    }

                    builder.Append(randomCap);
                }
                else
                {
                    builder.Append(varArray[i]);
                }
            }

            var result = builder.ToString();

            if (!(parameter))
            {
                result = result.Insert(0, "{") + "}";
            }

            return result;
        }


        public static string ReObfuscateVariable(string variable)
        {
            if (variable.Contains('`'))
            {
                variable = variable.Replace("`", "");
                var variableNoBraces = variable.Substring(2, variable.Length - 3);
                var varArray = variableNoBraces.ToCharArray();

                var outString = new StringBuilder();

                for (int i = 0; i < varArray.Length; i++)
                {
                    var currentChar = RandomCapitalization(varArray[i].ToString());

                    if (GoodChars.Contains(currentChar))
                    {
                        outString.Append('`' + currentChar);
                    }
                    else
                    {
                        outString.Append(currentChar);

                    }
                }

                var toReturn = "${" + outString.ToString() + "}";
                return toReturn;
            }

            return new string(variable.Select(c => GoodChars.Contains(c) ? RandomCapitalization(c.ToString())[0] : c).ToArray());
            //return variable;
        }

        public static string ObfuscateTrue()
        {
            return $"({choices[Random.Next(choices.Length)]})";
        }

        public static string ObfuscateFalse()
        {
            return $"(!({choices[Random.Next(choices.Length)]}))";
        }
        public static string ObfuscateNull() => "$null";
    }
}
