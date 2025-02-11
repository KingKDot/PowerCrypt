using Spectre.Console;

namespace PowerCrypt.Obfuscator.Methods
{
    public class Replacer
    {
        public static string Replace(string sourceText, int start, int length, string replacementText)
        {
            try
            {
                return sourceText.Remove(start, length).Insert(start, replacementText);
            }
            catch
            {
                AnsiConsole.MarkupInterpolated($"[red]Failed to replace text at position:[/] [yellow]{start}[/] [red]with length[/] [yellow]{length}[/] [red]to[/] [yellow]'{replacementText}'[/]\n");
                return sourceText;
            }
        }
    }
}
