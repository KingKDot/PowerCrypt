using PowerCrypt.Obfuscator;
using Spectre.Console;

namespace PowerCrypt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var startTime = DateTime.Now;
            var file_location = string.Empty;
            string outputLocation = string.Empty;

            if (args.Length == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No arguments provided. Asking for file path...[/]");

                //file_location = "C:\\Users\\this1\\Desktop\\Software\\Somalifuscator-Powershell-Edition\\main.ps1";

                file_location = AnsiConsole.Prompt(new TextPrompt<string>("[bold blue]Enter the location of the powershell code to obfuscate\n->[/]")
                        .Validate(path => System.IO.File.Exists(path) ? ValidationResult.Success() : ValidationResult.Error("[bold red]File does not exist or path input is incorrect.[/]"))
                );

            }
            else
            {
                if (File.Exists(args[0]))
                {
                    file_location = args[0];
                    outputLocation = args.Length > 1 ? args[1] : string.Empty;
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]File does not exist or path input is incorrect.[/]");
                    return;
                }
            }

            var textpath = new TextPath(file_location)
            {
                RootStyle = new Style(foreground: Color.Red),
                SeparatorStyle = new Style(foreground: Color.Green),
                StemStyle = new Style(foreground: Color.Blue),
                LeafStyle = new Style(foreground: Color.Red)
            };

            AnsiConsole.Write(textpath);
            AnsiConsole.Write("\n");

            var obfuscation = PowershellObfuscator.ObfuscateScript(File.ReadAllText(file_location));

            AnsiConsole.MarkupLine("[bold green]Obfuscation complete![/]");

            //write the content out as the file name + _obf.ps1
            var obf_file = file_location.Replace(".ps1", "_obf.ps1");

            if (!string.IsNullOrEmpty(outputLocation))
            {
                obf_file = outputLocation;
            }
            File.WriteAllText(obf_file, obfuscation);

            var endTime = DateTime.Now;
            var timeDiff = endTime - startTime;

            AnsiConsole.MarkupLine($"Obfuscation took [bold green]{timeDiff.Hours}[/] hours, [bold green]{timeDiff.Minutes}[/] minutes, [bold green]{timeDiff.Seconds}[/] seconds, [bold green]{timeDiff.Milliseconds}[/] milliseconds");

            var obf_textpath = new TextPath(obf_file)
            {
                RootStyle = new Style(foreground: Color.Red),
                SeparatorStyle = new Style(foreground: Color.Green),
                StemStyle = new Style(foreground: Color.Blue),
                LeafStyle = new Style(foreground: Color.Red)
            };

            AnsiConsole.Write(obf_textpath);
            Console.WriteLine("\n");
        }
    }
}