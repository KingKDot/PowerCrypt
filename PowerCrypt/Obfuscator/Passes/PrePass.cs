using PowerCrypt.Obfuscator.Helpers.globals;
using Spectre.Console;
using System.Management.Automation.Language;
using System.Text;

namespace PowerCrypt.Obfuscator.Passes
{
    public class PrePass
    {
        private static readonly string _decryptCode = """
                                        function DecodeAndDecompressString {param ([string] $inputString);$compressedBytes = [Convert]::FromBase64String($inputString);$memoryStream = [System.IO.MemoryStream]::new($compressedBytes);$decompressedStream = [System.IO.Compression.GzipStream]::new($memoryStream, [System.IO.Compression.CompressionMode]::Decompress);$decompressedStream = [System.IO.Compression.GzipStream]::new([System.IO.MemoryStream]::new($compressedBytes), [System.IO.Compression.CompressionMode]::Decompress);$decompressedStreamReader = [System.IO.StreamReader]::new($decompressedStream);$decompressedString = $decompressedStreamReader.ReadToEnd();return $decompressedString}

                                        """;

        public static string DoPrePrePass(string code)
        {
            Globals.CompressFunctionName = "DecodeAndDecompressString";

            ScriptBlockAst ast = (ScriptBlockAst)Parser.ParseInput(code, out _, out _);
            var firstNamedBlock = ast.FindAll(item => item is NamedBlockAst, false).FirstOrDefault() as NamedBlockAst;

            if (firstNamedBlock != null)
            {
                var firstChild = firstNamedBlock.FindAll(item => !(item is NamedBlockAst), true)
                                                 .FirstOrDefault();

                if (firstChild != null)
                {
                    int insertPosition = firstChild.Extent.StartOffset;
                    AnsiConsole.MarkupInterpolated($"[yellow]First child found; inserting decrypt code at position {insertPosition}[/]\n");
                    code = code.Insert(insertPosition, _decryptCode);
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]No first child found; prepending decrypt code.[/]");
                    code = _decryptCode + code;
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No first named block found; prepending decrypt code.[/]");
                code = _decryptCode + code;
            }

            //code = ObfuscateControlFlow(code);
            code = SplitExpandableStrings(code);

            return code;
        }


        public static string SplitExpandableStrings(string code)
        {
            ScriptBlockAst ast = (ScriptBlockAst)Parser.ParseInput(code, out _, out _);
            var expandableStrings = ast.FindAll(item => item is ExpandableStringExpressionAst, true).ToList();

            var obfuscatedCode = new StringBuilder(code);

            foreach (ExpandableStringExpressionAst expandableString in expandableStrings)
            {
                var parts = new List<string>();
                int lastIndex = expandableString.Extent.StartOffset;

                foreach (var nestedAst in expandableString.NestedExpressions)
                {
                    if (nestedAst.Extent.StartOffset > lastIndex)
                    {
                        string plainText = code.Substring(lastIndex, nestedAst.Extent.StartOffset - lastIndex);
                        if (!string.IsNullOrEmpty(plainText))
                        {
                            parts.Add($"\"{plainText}\"");
                        }
                    }

                    parts.Add(nestedAst.Extent.Text);
                    lastIndex = nestedAst.Extent.EndOffset;
                }

                if (lastIndex < expandableString.Extent.EndOffset)
                {
                    string remainingText = code.Substring(lastIndex, expandableString.Extent.EndOffset - lastIndex);
                    if (!string.IsNullOrEmpty(remainingText))
                    {
                        parts.Add($"\"{remainingText}\"");
                    }
                }

                string newExpression = $"{string.Join(" + ", parts)}";
                newExpression = newExpression.Substring(1, newExpression.Length - 2);

                newExpression = $"({newExpression})";

                obfuscatedCode.Replace(expandableString.Extent.Text, newExpression);
            }
            string output = obfuscatedCode.ToString();

            return output;
        }
    }
}
