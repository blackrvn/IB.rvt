# Script: copy_codebase_to_clipboard.ps1
# Purpose: Find all C# (.cs) and XAML (.xaml) files in the current directory and its subdirectories,
#          prepend each file’s content with its filename as a header,
#          and copy the complete output to the clipboard.
#
# Usage: Open PowerShell, navigate to the directory containing this script, and run:
#        .\copy_codebase_to_clipboard.ps1
#
# Note: This script uses the built-in Set-Clipboard cmdlet (available in PowerShell 5.0+).

# Retrieve all .cs and .xaml files recursively from the current directory.
$files = Get-ChildItem -Path . -Recurse -File | Where-Object { $_.Extension -match '^(?i)(\.cs|\.xaml)$' }

# Check if any matching files were found.
if (-not $files) {
    Write-Output "No C# (.cs) or XAML (.xaml) files found in this directory or subdirectories."
    exit 1
}

# Initialize an array to hold the output.
$outputLines = @()

# Process each file.
foreach ($file in $files) {
    # Add a header with the file's full path.
    $outputLines += "===== $($file.FullName) ====="
    
    # Read the entire file content.
    $fileContent = Get-Content -Path $file.FullName -Raw
    $outputLines += $fileContent

    # Add an empty line for separation.
    $outputLines += ""
}

# Combine the lines into a single string using Windows newline characters.
$outputText = $outputLines -join "`r`n"

# Copy the resulting text to the clipboard.
$outputText | Set-Clipboard

Write-Output "The code base (C# and XAML files with filenames as headers) has been copied to the clipboard."
