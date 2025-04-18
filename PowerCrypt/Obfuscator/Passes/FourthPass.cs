using PowerCrypt.Obfuscator.Helpers.globals;
using PowerCrypt.Obfuscator.Methods;
using PowerCrypt.Obfuscator.Methods.Counters;
using PowerCrypt.Obfuscator.Methods.GeneralControlFlowPostOBF;
using PowerCrypt.Settings;
using Spectre.Console;
using System.Diagnostics;
using System.Management.Automation.Language;

namespace PowerCrypt.Obfuscator.Passes
{
    public class FourthPass
    {
        private static List<ReplacementMapUniversal> CollectReplacements(ScriptBlockAst ast, bool isFunction)
        {
            var allReplacements = new List<ReplacementMapUniversal>();
            var nodes = isFunction
                ? ast.FindAll(a => a is FunctionDefinitionAst, searchNestedScriptBlocks: true)
                      .Where(func => !func.FindAll(inner => inner is FunctionDefinitionAst && inner != func, searchNestedScriptBlocks: true).Any())
                : ast.FindAll(a => a is IfStatementAst || a is ForStatementAst || a is ForEachStatementAst, searchNestedScriptBlocks: true)
                      .Where(stmt => !stmt.FindAll(inner => (inner is IfStatementAst || inner is ForStatementAst || inner is ForEachStatementAst) && inner != stmt, searchNestedScriptBlocks: true).Any() &&
                                     !stmt.FindAll(inner => inner is ReturnStatementAst, searchNestedScriptBlocks: true).Any());

            foreach (var function in Globals.FunctionNamesToSkip)
            {
                foreach (var node in nodes)
                {
                    bool shouldSkip = false;

                    if (isFunction && ((FunctionDefinitionAst)node).Name.Equals(Globals.CompressFunctionName))
                    {
                        continue;
                    }

                    foreach (var function2 in Globals.FunctionNamesToSkip)
                    {
                        if (isFunction && ((FunctionDefinitionAst)node).Name.Equals(function2))
                        {
                            Console.WriteLine(((FunctionDefinitionAst)node).Name);
                            shouldSkip = true;
                            break;
                        }
                    }

                    if (shouldSkip)
                    {
                        continue;
                    }

                    string obfuscatedText = WrapOBF.ObfuscateWithWrap(node.Extent.Text);
                    allReplacements.Add(new ReplacementMapUniversal
                    {
                        StartOffset = node.Extent.StartOffset,
                        Length = node.Extent.Text.Length,
                        OriginalName = node.Extent.Text,
                        Text = obfuscatedText,
                        Type = node.GetType().Name,
                        RequiresKeyword = false
                    });
                }
            }

            return allReplacements;
        }

        private static string ApplyReplacements(string scriptContent, List<ReplacementMapUniversal> allReplacements)
        {
            allReplacements = allReplacements.OrderByDescending(r => r.StartOffset).ToList();

            foreach (var replacement in allReplacements)
            {
                if (AppSettings.PrintToScreen)
                {
                    AnsiConsole.MarkupInterpolated($"[yellow]First Pass - Replacing[/] [red]'{replacement.OriginalName}'[/] [yellow]at position[/] [green]{replacement.StartOffset}[/] [yellow]with[/] [blue]'{replacement.Text}'[/] [yellow](Type: {replacement.Type})[/]\n");
                }

                Counter.Increment();
                scriptContent = Replacer.Replace(scriptContent, replacement.StartOffset, replacement.Length, replacement.Text);
            }

            return scriptContent;
        }

        public static string ProcessScript(string scriptContent)
        {
            ScriptBlockAst ast = (ScriptBlockAst)Parser.ParseInput(scriptContent, out _, out _);
            var allReplacements = CollectReplacements(ast, isFunction: false);

            while (allReplacements.Any())
            {
                scriptContent = ApplyReplacements(scriptContent, allReplacements);
                ast = (ScriptBlockAst)Parser.ParseInput(scriptContent, out _, out _);
                allReplacements = CollectReplacements(ast, isFunction: false);
            }

            var allFunctionReplacements = CollectReplacements(ast, isFunction: true);

            while (allFunctionReplacements.Any())
            {
                scriptContent = ApplyReplacements(scriptContent, allFunctionReplacements);
                ast = (ScriptBlockAst)Parser.ParseInput(scriptContent, out _, out _);
                allFunctionReplacements = CollectReplacements(ast, isFunction: true);
            }

            return scriptContent;
        }
    }
}