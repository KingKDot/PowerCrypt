using PowerCrypt.Obfuscator.Methods;
using PowerCrypt.Obfuscator.Methods.Counters;
using PowerCrypt.Obfuscator.Methods.StaticNumberObfuscation;
using PowerCrypt.Obfuscator.Methods.StringObfuscation;
using PowerCrypt.Obfuscator.Methods.VariableObfuscation;
using PowerCrypt.Settings;
using Spectre.Console;
using System.Management.Automation.Language;

namespace PowerCrypt.Obfuscator.Passes
{
    public class FirstPass
    {
        public static List<ReplacementMapUniversal> CollectFirstPassReplacements(ScriptBlockAst ast, Dictionary<string, string> functionReplacementMap, Dictionary<string, string> parameterReplacementMap, Dictionary<string, string> variableReplacementMap, Dictionary<string, string> stringReplacementMap, Dictionary<string, string> numberReplacementMap, HashSet<string> functionNamesIgnore)
        {
            var allReplacements = new List<ReplacementMapUniversal>();

            //function defs
            var functionDefinitions = ast.FindAll(a => a is FunctionDefinitionAst, searchNestedScriptBlocks: true)
                                         .Cast<FunctionDefinitionAst>();

            foreach (var func in functionDefinitions)
            {
                if (functionNamesIgnore.Contains(func.Name)) continue;

                const string functionKeyword = "function ";
                var fullStartOffset = func.Extent.StartOffset;
                var nameStartOffset = func.Extent.StartOffset;

                //find actual start of function name by checking for the keyword
                if (func.Extent.Text.TrimStart().ToLower().StartsWith(functionKeyword))
                {
                    nameStartOffset = fullStartOffset + func.Extent.Text.ToLower().IndexOf(functionKeyword) + functionKeyword.Length;
                }

                allReplacements.Add(new ReplacementMapUniversal
                {
                    StartOffset = nameStartOffset,
                    Length = func.Name.Length,
                    OriginalName = func.Name,
                    Text = func.Name,
                    Type = "Function",
                    RequiresKeyword = true
                });
            }

            //command calls
            var commandAsts = ast.FindAll(a => a is CommandAst, searchNestedScriptBlocks: true)
                                 .Cast<CommandAst>();

            foreach (var call in commandAsts)
            {
                var commandName = call.CommandElements[0].Extent.Text;

                if (functionNamesIgnore.Contains(commandName)) continue;

                if (functionReplacementMap.ContainsKey(commandName))
                {
                    //verify the replacement exists and is valid
                    var newName = functionReplacementMap[commandName];
                    allReplacements.Add(new ReplacementMapUniversal
                    {
                        StartOffset = call.CommandElements[0].Extent.StartOffset,
                        Length = commandName.Length,
                        OriginalName = commandName,
                        Text = commandName,
                        Type = "Function",
                        RequiresKeyword = false // function calls don't need the keyword
                    });

                    //process parameters
                    for (int i = 1; i < call.CommandElements.Count; i++)
                    {
                        var element = call.CommandElements[i];
                        if (element.Extent.Text.StartsWith('-'))
                        {
                            var paramName = element.Extent.Text.TrimStart('-');
                            if (parameterReplacementMap.ContainsKey(paramName))
                            {
                                allReplacements.Add(new ReplacementMapUniversal
                                {
                                    StartOffset = element.Extent.StartOffset,
                                    Length = element.Extent.Text.Length,
                                    OriginalName = paramName,
                                    Text = "-" + paramName,
                                    Type = "ParameterName",
                                    RequiresKeyword = false
                                });
                            }
                        }
                    }
                }
            }

            //variable expressions
            var variableAsts = ast.FindAll(a => a is VariableExpressionAst, searchNestedScriptBlocks: true)
                                  .Cast<VariableExpressionAst>();

            foreach (var variable in variableAsts)
            {
                var varName = variable.VariablePath.UserPath;
                var parent = variable.Parent;
                var isParameterName = false;
                while (parent != null)
                {
                    if (parent is CommandAst commandAst)
                    {
                        isParameterName = commandAst.CommandElements.Any(e => e.Extent.Text == "-" + variable.Extent.Text);
                        if (isParameterName) break;
                    }
                    parent = parent.Parent;
                }

                if (!isParameterName)
                {
                    allReplacements.Add(new ReplacementMapUniversal
                    {
                        StartOffset = variable.Extent.StartOffset,
                        Length = variable.Extent.Text.Length,
                        OriginalName = varName,
                        Text = variable.Extent.Text,
                        Type = "Variable",
                        RequiresKeyword = false
                    });
                }
            }

            //string constants
            var stringAsts = ast.FindAll(a => a is StringConstantExpressionAst stringAst &&
                                              (stringAst.StringConstantType == StringConstantType.DoubleQuoted ||
                                               stringAst.StringConstantType == StringConstantType.SingleQuoted),
                                          searchNestedScriptBlocks: true)
                                .Cast<StringConstantExpressionAst>();

            foreach (var stringAst in stringAsts)
            {
                var stringText = stringAst.Extent.Text;

                //handle empty strings
                if (stringText == "''" || stringText == "\"\"")
                {
                    allReplacements.Add(new ReplacementMapUniversal
                    {
                        StartOffset = stringAst.Extent.StartOffset,
                        Length = stringAst.Extent.Text.Length,
                        OriginalName = stringText,
                        Text = "([string]::Empty)",
                        Type = "EmptyString",
                        RequiresKeyword = false
                    });
                    continue;
                }

                if (!stringReplacementMap.ContainsKey(stringText))
                {
                    var quoteType = stringText.StartsWith("'") ? "SingleQuoted" : "DoubleQuoted";
                    stringReplacementMap[stringText] = "(" + StringOBF.ObfuscateString(stringText, quoteType) + ")";
                }

                allReplacements.Add(new ReplacementMapUniversal
                {
                    StartOffset = stringAst.Extent.StartOffset,
                    Length = stringAst.Extent.Text.Length,
                    OriginalName = stringText,
                    Text = stringAst.Extent.Text,
                    Type = "String",
                    RequiresKeyword = false
                });
            }

            var numberAsts = ast.FindAll(a => a is ConstantExpressionAst constantAst && constantAst.StaticType == typeof(int), searchNestedScriptBlocks: true)
                                .Cast<ConstantExpressionAst>();

            foreach (var numberAst in numberAsts)
            {
                var numberText = numberAst.Extent.Text;
                if (!numberReplacementMap.ContainsKey(numberText))
                {
                    numberReplacementMap[numberText] = NumberOBF.ObfuscateNumber(numberText);
                }

                allReplacements.Add(new ReplacementMapUniversal
                {
                    StartOffset = numberAst.Extent.StartOffset,
                    Length = numberAst.Extent.Text.Length,
                    OriginalName = numberText,
                    Text = numberText,
                    Type = "Number",
                    RequiresKeyword = false
                });
            }

            return allReplacements;
        }

        public static string ApplyReplacements(string scriptContent, List<ReplacementMapUniversal> allReplacements, Dictionary<string, string> functionReplacementMap, Dictionary<string, string> parameterReplacementMap, Dictionary<string, string> variableReplacementMap, Dictionary<string, string> stringReplacementMap, Dictionary<string, string> numberReplacementMap)
        {
            allReplacements = allReplacements.OrderByDescending(r => r.StartOffset).ToList();

            foreach (var replacement in allReplacements)
            {
                string newName = replacement.Type switch
                {
                    "Function" => functionReplacementMap[replacement.OriginalName],
                    "ParameterName" => "-" + parameterReplacementMap[replacement.OriginalName],
                    "Variable" => variableReplacementMap[replacement.OriginalName],
                    "String" => stringReplacementMap[replacement.OriginalName],
                    "Command" => replacement.Text,
                    "Number" => numberReplacementMap[replacement.OriginalName],
                    "EmptyString" => "([string]::Empty)",
                    _ => replacement.Text
                };

                if (replacement.Type == "Variable")
                {
                    newName = VariableOBF.ReObfuscateVariable(newName);
                }

                if (AppSettings.PrintToScreen)
                {
                    AnsiConsole.MarkupInterpolated($"[yellow]First Pass - Replacing[/] [red]'{replacement.OriginalName}'[/] [yellow]at position[/] [green]{replacement.StartOffset}[/] [yellow]with[/] [blue]'{newName}'[/] [yellow](Type: {replacement.Type})[/]\n");
                }

                Counter.Increment();

                scriptContent = Replacer.Replace(scriptContent, replacement.StartOffset, replacement.Length, newName);
            }

            return scriptContent;
        }
    }
}
