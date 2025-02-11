using PowerCrypt.Obfuscator.Methods;
using PowerCrypt.Obfuscator.Methods.Counters;
using PowerCrypt.Obfuscator.Methods.StringObfuscation;
using PowerCrypt.Settings;
using Spectre.Console;
using System.Management.Automation.Language;

namespace PowerCrypt.Obfuscator.Passes
{
    public class ThirdPass
    {
        public static List<ReplacementMapUniversal> CollectThirdPassReplacements(ScriptBlockAst ast)
        {
            var allReplacements = new List<ReplacementMapUniversal>();
            var processedStrings = new HashSet<(int StartOffset, string Value)>();

            //find all InvokeMemberExpressionAst nodes
            var memberInvocations = ast.FindAll(a => a is InvokeMemberExpressionAst, searchNestedScriptBlocks: true)
                                       .Cast<InvokeMemberExpressionAst>();

            foreach (var invocation in memberInvocations)
            {
                //find string constants within this invocation that are NOT single or double quoted
                var stringConstants = invocation.FindAll(a => a is StringConstantExpressionAst stringAst &&
                                                              !(stringAst.StringConstantType == StringConstantType.DoubleQuoted ||
                                                                stringAst.StringConstantType == StringConstantType.SingleQuoted),
                                                          searchNestedScriptBlocks: true)
                                                .Cast<StringConstantExpressionAst>();

                foreach (var stringConstant in stringConstants)
                {
                    var key = (stringConstant.Extent.StartOffset, stringConstant.Value);
                    if (!processedStrings.Contains(key))
                    {
                        var newString = StringOBFStringConstantExpressionAstUnderMember.ObfuscateString(stringConstant.Value);

                        allReplacements.Add(new ReplacementMapUniversal
                        {
                            StartOffset = stringConstant.Extent.StartOffset,
                            Length = stringConstant.Extent.Text.Length,
                            OriginalName = stringConstant.Extent.Text,
                            Text = newString,
                            Type = "WeirdMethodString",
                            RequiresKeyword = false
                        });

                        processedStrings.Add(key);
                    }
                }
            }

            return allReplacements;
        }

        public static string ApplyReplacements(string scriptContent, List<ReplacementMapUniversal> allReplacements)
        {
            allReplacements = allReplacements.OrderByDescending(r => r.StartOffset).ToList();

            foreach (var replacement in allReplacements)
            {
                string newName = replacement.Type switch
                {
                    _ => replacement.Text
                };

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
