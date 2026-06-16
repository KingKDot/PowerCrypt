<<<<<<< HEAD

# PowerCrypt

The greatest powershell obfuscator ever made.


## Quick Start

#### First Method
Download from github release ![here](https://github.com/KingKDot/PowerCrypt/releases/tag/AutoBuild)

#### Second Method (Source)
Install visual studio. \
Clone project in visual studio or download it as zip and import it. \
\
This project does support AOT building so you can either publish it (which is what the releases does) or build it as you would any other c# project.

### How to use after building/downloading
Using this tool is extremely easy. Either just put in the path of the file after double clicking, or put in the path of the file as an argument via the command line.


## Features

- Extremely fast (.5 miliseconds for a 21kb powershell script)
- Protects exceptionaly well
- At time of writing it isn't detected statically by a single antivirus
- Cross platform
- Supports AOT building
- Exclusively uses and parses the powershell AST to do proper obfuscation

## Before and after output
Before: \
![before_image](https://github.com/user-attachments/assets/69557614-4eea-4b80-bf68-c0ef3c2d7263)
After: \
![after_image](https://github.com/user-attachments/assets/09868a6e-3ada-4adf-b524-23baf9d8fc71)



## Authors

- [@KingKDot](https://www.github.com/KingKDot)
=======
# PowerCrypt

PowerCrypt is a cross-platform .NET command-line tool for obfuscating PowerShell
scripts.

## Requirements

- .NET 8 SDK for building from source
- Linux, macOS, or Windows

## Run from source

```bash
dotnet run --project PowerCrypt/PowerCrypt.csproj -- input.ps1
```

To choose the output file:

```bash
dotnet run --project PowerCrypt/PowerCrypt.csproj -- input.ps1 output_obf.ps1
```

## Publish for Linux

Create a self-contained Linux x64 build:

```bash
dotnet publish PowerCrypt/PowerCrypt.csproj -c Release -r linux-x64 -o artifacts/publish/linux-x64
```

Run the published binary:

```bash
./artifacts/publish/linux-x64/PowerCrypt input.ps1
```
>>>>>>> 773c115 (linux support)
