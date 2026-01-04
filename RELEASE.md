# Release Guide

This document describes how to create and publish releases of Barrow Weather.

## Prerequisites

- .NET 8 SDK installed
- Git configured with access to the repository
- GitHub Actions enabled for the repository (for automated releases)

## Release Process

### 1. Update Version

Update the version in the `VERSION` file:

```
0.2.2
```

### 2. Update Changelog

Add your changes to `CHANGELOG.md` under the `[Unreleased]` section. When creating a release, these will be moved to the versioned section.

### 3. Create Release Tag

Use the helper script:

```bash
python create_release.py
```

Or manually:

```bash
# Read version from VERSION file
VERSION=$(cat VERSION)

# Create and push tag
git tag -a v$VERSION -m "Release $VERSION"
git push origin v$VERSION
```

### 4. Automated Build

When you push a tag starting with `v`, GitHub Actions will automatically:

1. Build the application for all platforms (x64, x86, ARM64)
2. Create MSIX installers
3. Create ZIP archives with published files
4. Create a GitHub release
5. Upload all artifacts to the release

## Manual Release Build

If you need to build releases locally:

```bash
# Build for all platforms
python build_release.py

# Build for specific platform
python build_release.py --platform x64

# Skip MSIX packaging (faster, creates ZIP only)
python build_release.py --skip-msix
```

Output will be in the `release/` directory:
- `release/x64/` - Published files for x64
- `release/x86/` - Published files for x86
- `release/ARM64/` - Published files for ARM64
- `release/*.msix` - MSIX installer packages
- `release/*.zip` - ZIP archives

## MSIX Packaging

MSIX packages are created automatically during the build process. The package manifest (`Package.appxmanifest`) defines:

- App identity and version
- Capabilities (internet access, full trust)
- Visual assets (logos, splash screen)

**Note:** For production releases, you should:
1. Create a code signing certificate
2. Update `BarrowWeather.csproj` to reference your certificate
3. Sign the MSIX package before distribution

## Testing Releases

Before publishing:

1. Build a release locally:
   ```bash
   python build_release.py --platform x64
   ```

2. Test the MSIX installer:
   ```bash
   # Install
   Add-AppxPackage release/BarrowWeather-0.2.3-x64.msix
   
   # Uninstall
   Remove-AppxPackage BarrowWeather
   ```

3. Test the ZIP archive:
   - Extract and run `BarrowWeather.exe`
   - Verify all features work correctly

## Troubleshooting

### MSIX Packaging Fails

MSIX packaging requires:
- Windows SDK installed
- Visual Studio Build Tools or Windows SDK Build Tools
- Running on Windows (MSIX packaging doesn't work on Linux/Mac)

If MSIX packaging fails, you can still create ZIP archives:

```bash
python build_release.py --skip-msix
```

### GitHub Actions Fails

Check the Actions tab in GitHub for error details. Common issues:

- Missing dependencies
- Version format issues
- Missing assets (logos, etc.)

### Version Mismatch

Ensure the version in:
- `VERSION` file
- `Package.appxmanifest` (Identity/Version)
- Git tag (should match VERSION)

Are all consistent.
