# PowerCrypt

The greatest PowerShell obfuscator ever made.

## Quick Start

### Download a release

Download the latest automated Windows or Linux build from the
[AutoBuild release](https://github.com/KingKDot/PowerCrypt/releases/tag/AutoBuild).

### Run from source

Install the .NET 8 SDK, then run:

```bash
dotnet run --project PowerCrypt/PowerCrypt.csproj -- input.ps1
```

To choose the output file:

```bash
dotnet run --project PowerCrypt/PowerCrypt.csproj -- input.ps1 output_obf.ps1
```

## Publish Manually

Create a self-contained Windows x64 build:

```bash
dotnet publish PowerCrypt/PowerCrypt.csproj -c Release -r win-x64 -o artifacts/publish/win-x64
```

Create a self-contained Linux x64 build:

```bash
dotnet publish PowerCrypt/PowerCrypt.csproj -c Release -r linux-x64 -o artifacts/publish/linux-x64
```

Run the published Linux binary:

```bash
./artifacts/publish/linux-x64/PowerCrypt input.ps1
```

## Usage

Pass the path of the file to obfuscate as the first command-line argument, or
run the app without arguments and enter the path when prompted.

## Features

- Extremely fast
- Protects exceptionally well
- Cross-platform Windows and Linux builds
- Uses the PowerShell AST to do proper obfuscation

## Before and After Output

Before:

![before_image](https://github.com/user-attachments/assets/69557614-4eea-4b80-bf68-c0ef3c2d7263)

After:

![after_image](https://github.com/user-attachments/assets/09868a6e-3ada-4adf-b524-23baf9d8fc71)

## Authors

- [@KingKDot](https://www.github.com/KingKDot)
