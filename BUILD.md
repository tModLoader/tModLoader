# Building tModLoader with .NET SDK

This guide explains how to build tModLoader using the .NET SDK command-line interface.

## Prerequisites

- **.NET SDK 9.0 or higher** - [Download](https://dotnet.microsoft.com/download/dotnet)
- **Git** - For cloning the repository

## Quick Start

### Clone the Repository
```bash
git clone https://github.com/KaydenTheSequel/tModLoader.git
cd tModLoader
```

### Build the Solution
```bash
# Restore NuGet packages and build in Release mode
dotnet build -c Release

# Or build in Debug mode for development
dotnet build -c Debug
```

## Detailed Build Instructions

### 1. Restore NuGet Dependencies
```bash
dotnet restore
```

### 2. Clean Previous Builds
```bash
dotnet clean
```

### 3. Build Specific Configurations

#### Debug Build (for development)
```bash
dotnet build -c Debug --nologo -v minimal
```

#### Release Build (optimized)
```bash
dotnet build -c Release --nologo -v minimal
```

### 4. Build Specific Projects

#### Build Terraria Core Only
```bash
dotnet build src/tModLoader/Terraria/Terraria.csproj -c Release
```

#### Build ExampleMod
```bash
dotnet build ExampleMod/ExampleMod.csproj -c Release
```

#### Build Entire Solution
```bash
dotnet build tModLoader.sln -c Release
```

## Build Output

- **Debug builds:** Output goes to `bin/Debug/` in each project directory
- **Release builds:** Output goes to `bin/Release/` in each project directory

## Using Build Scripts

### Windows Batch Scripts
```bash
# Debug build
cd solutions
buildDebug.bat

# Release build
cd solutions
buildRelease.bat
```

### Cross-Platform (Windows, macOS, Linux)
```bash
# Build and run in one command
dotnet run --project src/tModLoader/Terraria/Terraria.csproj -c Release
```

## CLI-Only Build (No Visual Studio Required)

If you only have the .NET SDK installed and don't have Visual Studio:

```bash
# Complete build from scratch
dotnet clean
dotnet restore
dotnet build -c Release

# Or using the Makefile (on macOS/Linux)
make build
```

## Build Options

| Option | Description |
|--------|-------------|
| `-c Release/Debug` | Build configuration (Release or Debug) |
| `--no-restore` | Skip restoring NuGet packages |
| `-v` | Verbosity level: `q` (quiet), `m` (minimal), `n` (normal), `d` (detailed) |
| `--nologo` | Don't display startup banner |
| `/clp:ErrorsOnly` | Only show errors |

## Troubleshooting

### "dotnet: command not found"
Ensure .NET SDK is installed and in your PATH:
```bash
dotnet --version
```

### Restore Fails
Clear the NuGet cache and retry:
```bash
dotnet nuget locals all --clear
dotnet restore
```

### Build Errors
Check the verbose output for details:
```bash
dotnet build -c Release -v detailed
```

## Project Structure

```
tModLoader/
├── src/
│   └── tModLoader/
│       ├── Terraria/          # Core Terraria assembly
│       └── ReLogic/            # ReLogic dependency
├── FNA/                        # FNA game framework
├── ExampleMod/                 # Example mod project
├── tModPorter/                 # Mod porting tool
├── tModCodeAssist/             # IDE code assistance
├── test/                       # Unit tests
├── tModLoader.sln              # Solution file
└── tModLoader.targets          # MSBuild targets

```

## Additional Resources

- [.NET Build Documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)
- [tModLoader Wiki](https://github.com/tModLoader/tModLoader/wiki)
- [tModLoader Development](https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Usage-FAQ)

## Support

For build issues specific to tModLoader, visit:
- GitHub Issues: https://github.com/KaydenTheSequel/tModLoader/issues
- Discussions: https://github.com/KaydenTheSequel/tModLoader/discussions
