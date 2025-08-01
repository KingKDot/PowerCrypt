using PowerCrypt.Obfuscator.Methods;
using PowerCrypt.Obfuscator.Methods.CommandTypeAndBareWordObfuscation;
using PowerCrypt.Obfuscator.Methods.Counters;
using PowerCrypt.Obfuscator.Methods.MixedBooleanArithmetic;
using PowerCrypt.Settings;
using Spectre.Console;
using System.Management.Automation.Language;

namespace PowerCrypt.Obfuscator.Passes
{
    public class SecondPass
    {
        public static List<ReplacementMapUniversal> CollectSecondPassReplacements(ScriptBlockAst ast, Dictionary<string, string> functionReplacementMap)
        {
            var allReplacements = new List<ReplacementMapUniversal>();

            var barewordAsts = ast.FindAll(testAst =>
                (testAst is StringConstantExpressionAst stringAst && stringAst.StringConstantType == StringConstantType.BareWord) ||
                testAst is TypeExpressionAst, true);

            foreach (var bareword in barewordAsts)
            {
                if (functionReplacementMap.ContainsKey(bareword.Extent.Text))
                {
                    continue;
                }

                if (bareword.Extent.Text.Length > 100)
                {
                    continue;
                }

                bool isFunctionCall = false;
                if (bareword.Parent is CommandAst commandAst)
                {
                    var commandElements = commandAst.CommandElements;
                    isFunctionCall = commandElements[0].Extent.Text == bareword.Extent.Text;

                    if (!isFunctionCall)
                    {
                        continue;
                    }
                }

                string newBareWordName;
                if (isFunctionCall)
                {
                    if (!AppSettings.ObfuscateCommands || !AppSettings.ObfuscateBuiltInCommands)
                    {
                        continue;
                    }
                    newBareWordName = CommandOBF.ObfuscateCommand(bareword.Extent.Text);
                }
                else
                {
                    if (!AppSettings.ObfuscateBareWords || !AppSettings.ObfuscateCommonBareWords)
                    {
                        continue;
                    }
                    newBareWordName = CommandOBF.ObfuscateCommonBareWord(bareword.Extent.Text);
                }

                allReplacements.Add(new ReplacementMapUniversal
                {
                    StartOffset = bareword.Extent.StartOffset,
                    Length = bareword.Extent.Text.Length,
                    OriginalName = bareword.Extent.Text,
                    Text = newBareWordName,
                    Type = "Bareword",
                    RequiresKeyword = false,
                });
            }

            var binaryExpressionAsts = ast.FindAll(testAst =>
                testAst is BinaryExpressionAst binaryAst &&
                new[] { TokenKind.Plus, TokenKind.Minus, TokenKind.Band, TokenKind.Bxor, TokenKind.Bor }.Contains(binaryAst.Operator), true);

            foreach (var binary in binaryExpressionAsts)
            {
                if (!AppSettings.EnableMixedBooleanArithmetic)
                {
                    break;
                }

                if (binary is BinaryExpressionAst binaryAst)
                {
                    var left = binaryAst.Left;
                    var right = binaryAst.Right;

                    if (left is ConstantExpressionAst && right is ConstantExpressionAst)
                    {
                        var text = binaryAst.Extent.Text;
                        var obfuscated = MBAOBF.ApplyMBAObfuscation(left.Extent.Text, right.Extent.Text, binaryAst.Operator.ToString(), 4);

                        if (obfuscated == null)
                        {
                            continue;
                        }

                        allReplacements.Add(new ReplacementMapUniversal
                        {
                            StartOffset = binaryAst.Extent.StartOffset,
                            Length = binaryAst.Extent.Text.Length,
                            OriginalName = text,
                            Text = obfuscated,
                            Type = "Binary",
                            RequiresKeyword = false,
                        });
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
