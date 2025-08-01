using PowerCrypt.Obfuscator.Helpers.globals;
using PowerCrypt.Obfuscator.Methods.Counters;
using PowerCrypt.Obfuscator.Methods.FunctionObfuscation;
using PowerCrypt.Obfuscator.Methods.StaticNumberObfuscation;
using PowerCrypt.Obfuscator.Methods.StringObfuscation;
using PowerCrypt.Obfuscator.Methods.VariableObfuscation;
using PowerCrypt.Obfuscator.Passes;
using PowerCrypt.Settings;
using Spectre.Console;
using System.Management.Automation.Language;
using System.Text;

namespace PowerCrypt.Obfuscator
{
    public class PowershellObfuscator
    {

        public static string ObfuscateScript(string scriptContent)
        {
            if (AppSettings.EnablePrePass)
            {
                scriptContent = PrePass.DoPrePrePass(scriptContent);
            }

            //return scriptContent;

            if (AppSettings.RemoveComments || AppSettings.RemoveEmptyLines)
            {
                scriptContent = RemoveCommentsAndJunk(scriptContent);
            }
            //return scriptContent;

            ScriptBlockAst ast = ParseScript(scriptContent);

            var functionReplacementMap = new Dictionary<string, string>();
            var variableReplacementMap = new Dictionary<string, string>();
            var parameterReplacementMap = new Dictionary<string, string>();
            var stringReplacementMap = new Dictionary<string, string>();
            var numberReplacementMap = new Dictionary<string, string>();
            var functionNamesIgnore = AppSettings.FunctionNamesToIgnore;

            if (AppSettings.ObfuscateFunctions)
            {
                ProcessFunctionDefinitions(ast, functionReplacementMap, parameterReplacementMap, variableReplacementMap);
            }
            
            if (AppSettings.ObfuscateVariables)
            {
                ProcessVariables(ast, variableReplacementMap, parameterReplacementMap);
            }
            
            if (AppSettings.ObfuscateStrings)
            {
                ProcessStrings(ast, stringReplacementMap);
            }
            
            if (AppSettings.ObfuscateNumbers)
            {
                ProcessNumbers(ast, numberReplacementMap);
            }

            var allReplacements = FirstPass.CollectFirstPassReplacements(ast, functionReplacementMap, parameterReplacementMap, variableReplacementMap, stringReplacementMap, numberReplacementMap, functionNamesIgnore);

            scriptContent = FirstPass.ApplyReplacements(scriptContent, allReplacements, functionReplacementMap, parameterReplacementMap, variableReplacementMap, stringReplacementMap, numberReplacementMap);

            //return scriptContent;

            ast = ParseScript(scriptContent);
            var allReplacements2 = SecondPass.CollectSecondPassReplacements(ast, functionReplacementMap);
            scriptContent = SecondPass.ApplyReplacements(scriptContent, allReplacements2);

            //return scriptContent;

            ast = ParseScript(scriptContent);
            var allReplacements3 = ThirdPass.CollectThirdPassReplacements(ast);
            scriptContent = ThirdPass.ApplyReplacements(scriptContent, allReplacements3);

            //return scriptContent;

            scriptContent = FourthPass.ProcessScript(scriptContent);

            AnsiConsole.MarkupInterpolated($"[green]Obfuscated {Counter.GetTransformations()} objects in the script[/]\n");

            return scriptContent;
        }

        private static ScriptBlockAst ParseScript(string scriptContent)
        {
            ScriptBlockAst ast = Parser.ParseInput(scriptContent, out _, out ParseError[] errorsOut);

            if (errorsOut.Length != 0)
            {
                AnsiConsole.MarkupLine("[red]Failed to parse script, but continuing anyway.[/]");
            }

            return ast;
        }

        private static void ProcessFunctionDefinitions(ScriptBlockAst ast, Dictionary<string, string> functionReplacementMap, Dictionary<string, string> parameterReplacementMap, Dictionary<string, string> variableReplacementMap)
        {
            var functionDefinitions = ast.FindAll(a => a is FunctionDefinitionAst, searchNestedScriptBlocks: true)
                                         .Cast<FunctionDefinitionAst>();

            foreach (var func in functionDefinitions)
            {
                if (!functionReplacementMap.ContainsKey(func.Name))
                {
                    string obfuscatedFunctionName = FunctionOBF.ObfuscateFunctionSimple(func.Name);

                    Globals.AddUsedFunction(obfuscatedFunctionName);

                    functionReplacementMap[func.Name] = obfuscatedFunctionName;
                }

                //handle parameters in function definition
                if (func.Parameters != null)
                {
                    foreach (var param in func.Parameters)
                    {
                        var paramName = param.Name.VariablePath.UserPath;
                        if (!parameterReplacementMap.ContainsKey(paramName))
                        {
                            var newParamName = VariableOBF.ObfuscateVariable(paramName, true);
                            parameterReplacementMap[paramName] = newParamName.TrimStart('$');
                            variableReplacementMap[paramName] = newParamName;
                        }
                    }
                }

                if (func.Body.ParamBlock != null)
                {
                    foreach (var param in func.Body.ParamBlock.Parameters)
                    {
                        var paramName = param.Name.VariablePath.UserPath;
                        if (!parameterReplacementMap.ContainsKey(paramName))
                        {
                            var newParamName = VariableOBF.ObfuscateVariable(paramName, true);
                            parameterReplacementMap[paramName] = newParamName.TrimStart('$');
                            variableReplacementMap[paramName] = newParamName;
                        }
                    }
                }
            }
        }

        private static void ProcessVariables(ScriptBlockAst ast, Dictionary<string, string> variableReplacementMap, Dictionary<string, string> parameterReplacementMap)
        {
            var variableAsts = ast.FindAll(a => a is VariableExpressionAst, searchNestedScriptBlocks: true)
                                  .Cast<VariableExpressionAst>();

            foreach (var variable in variableAsts)
            {
                var varName = variable.VariablePath.UserPath;
                
                if (AppSettings.VariableNamesToIgnore.Contains(varName))
                {
                    continue;
                }
                
                if (!variableReplacementMap.ContainsKey(varName) && !parameterReplacementMap.ContainsKey(varName))
                {
                    string variableObfuscated = VariableOBF.ObfuscateVariable(variable.Extent.Text, false);

                    Globals.AddUsedVariable(variableObfuscated);

                    variableReplacementMap[varName] = variableObfuscated;
                }
            }
        }

        private static void ProcessNumbers(ScriptBlockAst ast, Dictionary<string, string> numberReplacementMap)
        {
            var numberAsts = ast.FindAll(a => a is ConstantExpressionAst constantAst && constantAst.StaticType == typeof(int), searchNestedScriptBlocks: true)
                                .Cast<ConstantExpressionAst>();
            foreach (var numberAst in numberAsts)
            {
                var numberText = numberAst.Extent.Text;
                
                if (int.TryParse(numberText, out int number) && AppSettings.NumbersToIgnore.Contains(number))
                {
                    continue;
                }
                
                if (!numberReplacementMap.ContainsKey(numberText))
                {
                    numberReplacementMap[numberText] = NumberOBF.ObfuscateNumber(numberText);
                }
            }
        }

        private static void ProcessStrings(ScriptBlockAst ast, Dictionary<string, string> stringReplacementMap)
        {
            var stringAsts = ast.FindAll(a => a is StringConstantExpressionAst stringAst &&
                                              (stringAst.StringConstantType == StringConstantType.DoubleQuoted ||
                                               stringAst.StringConstantType == StringConstantType.SingleQuoted),
                                          searchNestedScriptBlocks: true)
                                .Cast<StringConstantExpressionAst>();

            foreach (var stringAst in stringAsts)
            {
                var stringText = stringAst.Extent.Text;
                var actualString = stringAst.Value;

                if (actualString.Length < AppSettings.MinStringLengthToObfuscate || 
                    AppSettings.StringsToIgnore.Contains(actualString))
                {
                    continue;
                }

                if (!stringReplacementMap.ContainsKey(stringText))
                {
                    var quoteType = stringText.StartsWith("'") ? "SingleQuoted" : "DoubleQuoted";
                    stringReplacementMap[stringText] = "(" + StringOBF.ObfuscateString(stringText, quoteType) + ")";
                }
            }
        }

        private static string RemoveCommentsAndJunk(string scriptContent)
        {
            ScriptBlockAst ast = Parser.ParseInput(scriptContent, out Token[] tokens, out ParseError[] errors);
            if (errors.Length > 0)
            {
                //throw new Exception($"Failed to parse PowerShell script: {string.Join(", ", errors.Select(e => e.Message))}");
                //sometimes the parser gets a little confused but no erros are actually present
                AnsiConsole.MarkupLine("[red]Failed to parse script, but continuing anyway.[/]");
            }

            var sb = new StringBuilder();
            int lastIndex = 0;

            //iterate through tokens and skip comments
            foreach (var token in tokens)
            {
                if (token.Kind == TokenKind.Comment)
                {
                    sb.Append(scriptContent, lastIndex, token.Extent.StartOffset - lastIndex);
                    lastIndex = token.Extent.EndOffset;
                }
            }

            //append the remaining content after the last comment
            sb.Append(scriptContent, lastIndex, scriptContent.Length - lastIndex);

            //now remove all empty lines that aren't inside of a string or anything. Just empty lines within the script.
            var lines = sb.ToString().Split('\n');
            sb.Clear();
            foreach (var line in lines)
            {
                if (line.Trim().Length > 0)
                {
                    sb.Append(line);
                }
            }

            return sb.ToString();
        }

    }
}
