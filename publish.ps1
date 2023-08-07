# Define the command to run based on the platform
if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Linux)) {
    $publishCommand = "dotnet publish -r linux-x64 -c Release"
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    $publishCommand = "dotnet publish -r win-x64 -c Release"
} else {
    Write-Host "Unsupported platform."
    Exit 1
}

# Execute the command
Invoke-Expression $publishCommand
