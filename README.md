
# PowerCrypt

The greatest powershell obfuscator ever made.

# Settings.json
I suppose you could call this a profile system but more like a config/settings system.
You can use any .json and at any path that is readable.
Adding settings is quite easy, all you need is a .json file that follows the below format:

```json
{
    "PrintToScreen": true,
    "EnablePrePass": true,
    "InsertDecryptFunction": true,
    "DecryptFunctionName": "DecodeAndDecompressString",
    "ProcessExpandableStrings": true,
    "RemoveComments": true,
    "RemoveEmptyLines": true,
    "ObfuscateFunctions": true,
    "ObfuscateVariables": true,
    "ObfuscateStrings": true,
    "MinStringLengthToObfuscate": 3,
    "ObfuscateNumbers": true,
    "ObfuscateCommands": true,
    "ObfuscateBuiltInCommands": true,
    "ObfuscateBareWords": true,
    "ObfuscateCommonBareWords": true,
    "EnableControlFlowObfuscation": true,
    "WrapWithControlFlow": true,
    "EnableMixedBooleanArithmetic": true,
    "FunctionNamesToIgnore": ["CheckValidationResult"],
    "VariableNamesToIgnore": [
        "ErrorActionPreference",
        "VerbosePreference",
        "WarningPreference",
        "InformationPreference",
        "DebugPreference"
    ],
    "StringsToIgnore": [],
    "NumbersToIgnore": [0, 1]
}
```

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
