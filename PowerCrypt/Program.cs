using System.Diagnostics;
using System.Text.Json;
using PowerCrypt.Obfuscator;
using PowerCrypt.Settings;
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
            string settingsFile = string.Empty;

            if (args.Length == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No arguments provided. Asking for file path...[/]");

                file_location = AnsiConsole.Prompt(new TextPrompt<string>("[bold blue]Enter the location of the powershell code to obfuscate\n->[/]")
                    .Validate(path => File.Exists(path) ? ValidationResult.Success() : ValidationResult.Error("[bold red]File does not exist or path input is incorrect.[/]"))
                );
            }
            else
            {
                if (File.Exists(args[0]) && Path.GetExtension(args[0]).ToLower() != ".json")
                {
                    file_location = args[0];
                }
                else
                {
                    AnsiConsole.MarkupLine("[bold red]File does not exist or path input is incorrect.[/]");

                    file_location = AnsiConsole.Prompt(new TextPrompt<string>("[bold blue]Enter the location of the powershell code to obfuscate\n->[/]")
                        .Validate(path => File.Exists(path) ? ValidationResult.Success() : ValidationResult.Error("[bold red]File does not exist or path input is incorrect.[/]"))
                    );
                }

                // Check for settings file first
                if (args.Length > 2 && Path.GetExtension(args[2]).ToLower() == ".json" && File.Exists(args[2]))
                {
                    settingsFile = args[2];
                    outputLocation = args.Length > 1 ? args[1] : string.Empty;
                }
                else if (args.Length > 1 && Path.GetExtension(args[1]).ToLower() == ".json" && File.Exists(args[1]))
                {
                    settingsFile = args[1];
                    outputLocation = string.Empty;
                }
                else if (args.Length > 1)
                {
                    outputLocation = args[1];
                }

                if (string.IsNullOrEmpty(settingsFile))
                {
                    AnsiConsole.MarkupLine($"[bold yellow]No settings file provided, using defaults[/]");
                }
                else
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(settingsFile);
                        using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                        {
                            JsonElement root = doc.RootElement;

                            if (root.TryGetProperty("PrintToScreen", out var value))
                                AppSettings.PrintToScreen = value.GetBoolean();
                            if (root.TryGetProperty("EnablePrePass", out value))
                                AppSettings.EnablePrePass = value.GetBoolean();
                            if (root.TryGetProperty("InsertDecryptFunction", out value))
                                AppSettings.InsertDecryptFunction = value.GetBoolean();
                            if (root.TryGetProperty("DecryptFunctionName", out value))
                                AppSettings.DecryptFunctionName = value.GetString();
                            if (root.TryGetProperty("ProcessExpandableStrings", out value))
                                AppSettings.ProcessExpandableStrings = value.GetBoolean();
                            if (root.TryGetProperty("RemoveComments", out value))
                                AppSettings.RemoveComments = value.GetBoolean();
                            if (root.TryGetProperty("RemoveEmptyLines", out value))
                                AppSettings.RemoveEmptyLines = value.GetBoolean();
                            if (root.TryGetProperty("ObfuscateFunctions", out value))
                                AppSettings.ObfuscateFunctions = value.GetBoolean();
                            if (root.TryGetProperty("FunctionNamesToIgnore", out value))
                                AppSettings.FunctionNamesToIgnore = new HashSet<string>(
                                    value.EnumerateArray().Select(v => v.GetString()));
                            if (root.TryGetProperty("ObfuscateVariables", out value))
                                AppSettings.ObfuscateVariables = value.GetBoolean();
                            if (root.TryGetProperty("VariableNamesToIgnore", out value))
                                AppSettings.VariableNamesToIgnore = new HashSet<string>(
                                    value.EnumerateArray().Select(v => v.GetString()));
                            if (root.TryGetProperty("ObfuscateStrings", out value))
                                AppSettings.ObfuscateStrings = value.GetBoolean();
                            if (root.TryGetProperty("MinStringLengthToObfuscate", out value))
                                AppSettings.MinStringLengthToObfuscate = value.GetInt32();
                            if (root.TryGetProperty("StringsToIgnore", out value))
                                AppSettings.StringsToIgnore = new HashSet<string>(
                                    value.EnumerateArray().Select(v => v.GetString()));
                            if (root.TryGetProperty("ObfuscateNumbers", out value))
                                AppSettings.ObfuscateNumbers = value.GetBoolean();
                            if (root.TryGetProperty("NumbersToIgnore", out value))
                                AppSettings.NumbersToIgnore = new HashSet<int>(
                                    value.EnumerateArray().Select(v => v.GetInt32()));
                            if (root.TryGetProperty("ObfuscateCommands", out value))
                                AppSettings.ObfuscateCommands = value.GetBoolean();
                            if (root.TryGetProperty("ObfuscateBuiltInCommands", out value))
                                AppSettings.ObfuscateBuiltInCommands = value.GetBoolean();
                            if (root.TryGetProperty("ObfuscateBareWords", out value))
                                AppSettings.ObfuscateBareWords = value.GetBoolean();
                            if (root.TryGetProperty("ObfuscateCommonBareWords", out value))
                                AppSettings.ObfuscateCommonBareWords = value.GetBoolean();
                            if (root.TryGetProperty("EnableControlFlowObfuscation", out value))
                                AppSettings.EnableControlFlowObfuscation = value.GetBoolean();
                            if (root.TryGetProperty("WrapWithControlFlow", out value))
                                AppSettings.WrapWithControlFlow = value.GetBoolean();
                            if (root.TryGetProperty("EnableMixedBooleanArithmetic", out value))
                                AppSettings.EnableMixedBooleanArithmetic = value.GetBoolean();

                            AnsiConsole.MarkupLine($"[bold green]Settings loaded successfully from JSON file[/]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading settings from JSON file:\n{ex.Message}");
                        Environment.Exit(1);
                    }
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
