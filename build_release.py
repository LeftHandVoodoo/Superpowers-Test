#!/usr/bin/env python3
"""
Build script for creating Barrow Weather release packages.

This script builds the application for all platforms and creates MSIX installers.
"""

import subprocess
import sys
import os
import argparse
import shutil
from pathlib import Path
from typing import List, Optional


def check_dotnet() -> bool:
    """Check if .NET SDK is installed."""
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
        print("Error: .NET SDK not found. Please install .NET 8 SDK.")
        return False


def read_version() -> str:
    """Read version from VERSION file."""
    version_file = Path("VERSION")
    if version_file.exists():
        return version_file.read_text().strip()
    return "0.1.0"


def update_manifest_version(version: str):
    """Update Package.appxmanifest with the current version."""
    manifest_path = Path("src/BarrowWeather/Package.appxmanifest")
    if not manifest_path.exists():
        print(f"Warning: Package.appxmanifest not found at {manifest_path}")
        return
    
    content = manifest_path.read_text()
    # Update version in Identity tag (format: Major.Minor.Patch.0)
    version_parts = version.split(".")
    if len(version_parts) < 3:
        version_parts.extend(["0"] * (3 - len(version_parts)))
    manifest_version = f"{version_parts[0]}.{version_parts[1]}.{version_parts[2]}.0"
    
    import re
    # Replace Version attribute in Identity tag
    content = re.sub(
        r'Version="[^"]*"',
        f'Version="{manifest_version}"',
        content
    )
    manifest_path.write_text(content)
    print(f"Updated Package.appxmanifest version to {manifest_version}")


def build_for_platform(platform: str, configuration: str = "Release") -> bool:
    """Build the project for a specific platform."""
    project_path = Path("src/BarrowWeather/BarrowWeather.csproj")
    if not project_path.exists():
        print(f"Error: Project file not found at {project_path}")
        return False

    print(f"\nBuilding for {platform} ({configuration})...")
    try:
        subprocess.run(
            [
                "dotnet",
                "build",
                str(project_path),
                "-p:Platform=" + platform,
                "-c", configuration,
                "-p:Configuration=" + configuration
            ],
            check=True
        )
        return True
    except subprocess.CalledProcessError as e:
        print(f"Build failed for {platform} with exit code {e.returncode}")
        return False


def publish_for_platform(platform: str, configuration: str = "Release", output_dir: Path = None) -> bool:
    """Publish the project for a specific platform."""
    project_path = Path("src/BarrowWeather/BarrowWeather.csproj")
    if not project_path.exists():
        print(f"Error: Project file not found at {project_path}")
        return False

    if output_dir is None:
        output_dir = Path(f"release/{platform}")

    print(f"\nPublishing for {platform} ({configuration}) to {output_dir}...")
    try:
        subprocess.run(
            [
                "dotnet",
                "publish",
                str(project_path),
                "-p:Platform=" + platform,
                "-c", configuration,
                "-p:Configuration=" + configuration,
                "-p:RuntimeIdentifier=win-" + platform.lower(),
                "-p:SelfContained=true",
                "-p:PublishSingleFile=false",
                "-p:IncludeNativeLibrariesForSelfExtract=true",
                f"-o", str(output_dir)
            ],
            check=True
        )
        return True
    except subprocess.CalledProcessError as e:
        print(f"Publish failed for {platform} with exit code {e.returncode}")
        return False


def create_msix(platform: str, version: str, output_dir: Path) -> Optional[Path]:
    """Create MSIX package for the platform."""
    project_path = Path("src/BarrowWeather/BarrowWeather.csproj")
    publish_dir = Path(f"release/{platform}")
    
    if not publish_dir.exists():
        print(f"Error: Publish directory not found at {publish_dir}")
        return None

    print(f"\nCreating MSIX package for {platform}...")
    
    # Use dotnet publish with MSIX packaging
    try:
        msix_output = output_dir / f"BarrowWeather-{version}-{platform}.msix"
        
        subprocess.run(
            [
                "dotnet",
                "publish",
                str(project_path),
                "-p:Platform=" + platform,
                "-c", "Release",
                "-p:RuntimeIdentifier=win-" + platform.lower(),
                "-p:SelfContained=true",
                "-p:AppxPackageOutputDir=" + str(output_dir),
                "-p:AppxPackageName=BarrowWeather",
                "-p:AppxPackageVersion=" + version.replace(".", ".") + ".0"
            ],
            check=True
        )
        
        # Find the created MSIX file
        msix_files = list(output_dir.glob(f"BarrowWeather*{platform}*.msix"))
        if msix_files:
            return msix_files[0]
        
        print(f"Warning: MSIX file not found in {output_dir}")
        return None
        
    except subprocess.CalledProcessError as e:
        print(f"MSIX creation failed for {platform} with exit code {e.returncode}")
        print("Note: MSIX packaging requires Windows SDK and may need to be done on Windows.")
        return None


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description="Build Barrow Weather release packages",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python build_release.py                    # Build all platforms
  python build_release.py --platform x64      # Build only x64
  python build_release.py --skip-msix         # Skip MSIX packaging
        """
    )
    parser.add_argument(
        "--platform",
        choices=["x64", "x86", "ARM64", "all"],
        default="all",
        help="Target platform(s) (default: all)"
    )
    parser.add_argument(
        "--skip-msix",
        action="store_true",
        help="Skip MSIX package creation (only build/publish)"
    )
    parser.add_argument(
        "--output-dir",
        type=str,
        default="release",
        help="Output directory for release files (default: release)"
    )

    args = parser.parse_args()

    # Change to script directory
    script_dir = Path(__file__).parent
    os.chdir(script_dir)

    # Check prerequisites
    if not check_dotnet():
        return 1

    # Read version
    version = read_version()
    print(f"\nBuilding release version: {version}")
    
    # Update manifest version
    update_manifest_version(version)

    # Determine platforms to build
    platforms = ["x64", "x86", "ARM64"] if args.platform == "all" else [args.platform]
    
    output_dir = Path(args.output_dir)
    output_dir.mkdir(exist_ok=True, parents=True)

    # Build and publish for each platform
    success_count = 0
    msix_files = []
    
    for platform in platforms:
        if build_for_platform(platform, "Release"):
            if publish_for_platform(platform, "Release", Path(f"release/{platform}")):
                success_count += 1
                
                if not args.skip_msix:
                    msix_file = create_msix(platform, version, output_dir)
                    if msix_file:
                        msix_files.append(msix_file)

    # Summary
    print(f"\n{'='*60}")
    print(f"Build Summary:")
    print(f"  Platforms built: {success_count}/{len(platforms)}")
    if msix_files:
        print(f"\n  MSIX packages created:")
        for msix_file in msix_files:
            size_mb = msix_file.stat().st_size / (1024 * 1024)
            print(f"    - {msix_file.name} ({size_mb:.1f} MB)")
    print(f"{'='*60}")

    return 0 if success_count == len(platforms) else 1


if __name__ == "__main__":
    sys.exit(main())
