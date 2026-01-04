#!/usr/bin/env python3
"""
Helper script to create a GitHub release.

This script:
1. Updates the version
2. Creates a git tag
3. Optionally pushes to GitHub
"""

import subprocess
import sys
import argparse
from pathlib import Path


def read_version() -> str:
    """Read version from VERSION file."""
    version_file = Path("VERSION")
    if version_file.exists():
        return version_file.read_text().strip()
    return "0.1.0"


def update_changelog(version: str):
    """Update CHANGELOG.md with release date."""
    changelog_path = Path("CHANGELOG.md")
    if not changelog_path.exists():
        print("Warning: CHANGELOG.md not found")
        return
    
    content = changelog_path.read_text()
    
    # Check if Unreleased section exists
    if "## [Unreleased]" in content:
        from datetime import datetime
        today = datetime.now().strftime("%Y-%m-%d")
        
        # Replace [Unreleased] with version and date
        content = content.replace(
            "## [Unreleased]",
            f"## [Unreleased]\n\n## [{version}] - {today}"
        )
        changelog_path.write_text(content)
        print(f"Updated CHANGELOG.md with version {version}")


def main():
    """Main entry point."""
    parser = argparse.ArgumentParser(
        description="Create a GitHub release",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python create_release.py                    # Use version from VERSION file
  python create_release.py --version 0.2.2    # Specify version
  python create_release.py --no-push          # Create tag but don't push
        """
    )
    parser.add_argument(
        "--version",
        type=str,
        help="Version to release (default: read from VERSION file)"
    )
    parser.add_argument(
        "--no-push",
        action="store_true",
        help="Don't push tag to remote"
    )
    parser.add_argument(
        "--skip-changelog",
        action="store_true",
        help="Skip updating CHANGELOG.md"
    )

    args = parser.parse_args()

    # Determine version
    if args.version:
        version = args.version
        # Update VERSION file
        Path("VERSION").write_text(version + "\n")
        print(f"Updated VERSION file to {version}")
    else:
        version = read_version()
        print(f"Using version from VERSION file: {version}")

    # Update changelog
    if not args.skip_changelog:
        update_changelog(version)

    # Create git tag
    tag_name = f"v{version}"
    print(f"\nCreating git tag: {tag_name}")
    
    try:
        # Check if tag already exists
        result = subprocess.run(
            ["git", "tag", "-l", tag_name],
            capture_output=True,
            text=True,
            check=True
        )
        if result.stdout.strip():
            print(f"Error: Tag {tag_name} already exists")
            return 1
        
        # Create tag
        subprocess.run(
            ["git", "tag", "-a", tag_name, "-m", f"Release {version}"],
            check=True
        )
        print(f"Created tag {tag_name}")
        
        # Push tag if requested
        if not args.no_push:
            print(f"\nPushing tag to remote...")
            subprocess.run(
                ["git", "push", "origin", tag_name],
                check=True
            )
            print(f"Pushed tag {tag_name} to remote")
            print("\nGitHub Actions will automatically create the release.")
        else:
            print(f"\nTag created locally. Push with: git push origin {tag_name}")
        
        return 0
        
    except subprocess.CalledProcessError as e:
        print(f"Error: Git command failed with exit code {e.returncode}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
