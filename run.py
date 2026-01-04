#!/usr/bin/env python3
"""
Launch script for Barrow Weather application.

This script builds and runs the Barrow Weather WinUI 3 desktop application.
"""

import subprocess
import sys
import os
import argparse
from pathlib import Path
from typing import Optional


def check_dotnet() -> bool:
    """Check if .NET SDK is installed and available."""
    try:
        result = subprocess.run(
            ["dotnet", "--version"],
            capture_output=True,
            text=True,
            check=True
        )
        print(f"Found .NET SDK version: {result.stdout.strip()}")
        return True
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("Error: .NET SDK not found. Please install .NET 8 SDK from:")
        print("https://dotnet.microsoft.com/download/dotnet/8.0")
        return False


def build_project(platform: str = "x64", configuration: str = "Debug") -> bool:
    """Build the BarrowWeather project."""
    project_path = Path("src/BarrowWeather/BarrowWeather.csproj")
    if not project_path.exists():
        print(f"Error: Project file not found at {project_path}")
        return False

    print(f"\nBuilding project (Platform: {platform}, Configuration: {configuration})...")
    try:
        result = subprocess.run(
            [
                "dotnet",
                "build",
                str(project_path),
                "-p:Platform=" + platform,
                "-c", configuration
            ],
            check=True
        )
        print("Build successful!")
        return True
    except subprocess.CalledProcessError as e:
        print(f"Build failed with exit code {e.returncode}")
        return False


def find_executable(platform: str, configuration: str) -> Optional[Path]:
    """Find the built executable for the given platform and configuration."""
    # WinUI 3 apps are built to: bin/{platform}/{configuration}/net8.0-windows10.0.19041.0/
    exe_paths = [
        Path(f"src/BarrowWeather/bin/{platform}/{configuration}/net8.0-windows10.0.19041.0/BarrowWeather.exe"),
        Path(f"src/BarrowWeather/bin/{platform}/{configuration}/net8.0-windows10.0.19041.0/BarrowWeather.dll"),
    ]
    
    for path in exe_paths:
        if path.exists():
            return path
    return None


def run_app(platform: str = "x64", configuration: str = "Debug", no_build: bool = False) -> int:
    """Run the BarrowWeather application."""
    if not no_build:
        if not build_project(platform, configuration):
            return 1

    project_path = Path("src/BarrowWeather/BarrowWeather.csproj")
    if not project_path.exists():
        print(f"Error: Project file not found at {project_path}")
        return 1

    # Try to find and run the executable directly (more reliable for WinUI 3)
    exe_path = find_executable(platform, configuration)
    
    print(f"\nStarting Barrow Weather (Platform: {platform})...")
    
    try:
        if exe_path and exe_path.suffix == ".exe":
            # Run executable directly
            subprocess.run([str(exe_path)], check=True)
            return 0
        elif exe_path:
            # Run DLL with dotnet
            subprocess.run(["dotnet", str(exe_path)], check=True)
            return 0
        else:
            # Fall back to dotnet run
            print("Executable not found, using dotnet run...")
            subprocess.run(
                [
                    "dotnet",
                    "run",
                    "--project", str(project_path),
                    "-p:Platform=" + platform,
                    "-c", configuration
                ],
                check=True
            )
            return 0
    except subprocess.CalledProcessError as e:
        exit_code = e.returncode
        # Convert large exit codes (Windows error codes) to readable format
        if exit_code > 0x7FFFFFFF:
            # This is likely a Windows error code, convert to signed
            exit_code = exit_code - 0x100000000
        print(f"Application exited with code {exit_code} (0x{exit_code & 0xFFFFFFFF:X})")
        if exit_code != 0:
            print("Note: Non-zero exit codes may indicate an application error.")
        return exit_code & 0xFF  # Return only the lower 8 bits for standard exit codes
    except KeyboardInterrupt:
        print("\nApplication interrupted by user")
        return 130


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description="Launch Barrow Weather desktop application",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python run.py                    # Run with default settings (x64, Debug)
  python run.py --platform x86      # Run for x86 platform
  python run.py --release          # Run Release build
  python run.py --no-build         # Skip build step (use existing build)
        """
    )
    parser.add_argument(
        "--platform",
        choices=["x64", "x86", "ARM64"],
        default="x64",
        help="Target platform (default: x64)"
    )
    parser.add_argument(
        "--release",
        action="store_true",
        help="Use Release configuration instead of Debug"
    )
    parser.add_argument(
        "--no-build",
        action="store_true",
        help="Skip build step and run existing build"
    )

    args = parser.parse_args()

    # Change to script directory
    script_dir = Path(__file__).parent
    os.chdir(script_dir)

    # Check prerequisites
    if not check_dotnet():
        return 1

    # Determine configuration
    configuration = "Release" if args.release else "Debug"

    # Run the application
    return run_app(
        platform=args.platform,
        configuration=configuration,
        no_build=args.no_build
    )


if __name__ == "__main__":
    sys.exit(main())
