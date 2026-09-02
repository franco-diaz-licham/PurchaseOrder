<#
.SYNOPSIS
Loads key-value pairs from a dotenv file into the current process environment.

.DESCRIPTION
Reads non-empty, non-comment lines in NAME=VALUE format and exposes each value
through process-scoped environment variables for the rest of the script.

.PARAMETER Path
Path to the dotenv file to load.

.EXCEPTION
Throws when the dotenv file does not exist or contains a malformed line.
#>
function Import-DotEnv {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path $Path)) {
        throw ".env file was not found: $Path"
    }

    Get-Content $Path | ForEach-Object {
        $line = $_.Trim()
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
            return
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -le 0) {
            throw "Invalid .env line: $line"
        }

        $name = $line.Substring(0, $separatorIndex).Trim()
        $value = $line.Substring($separatorIndex + 1).Trim().Trim('"').Trim("'")
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

<#
.SYNOPSIS
Reads a required configuration value from the current process environment.

.DESCRIPTION
Returns the environment variable value when present. Missing or blank values are
treated as configuration errors so provisioning fails early and predictably.

.PARAMETER Name
Environment variable name to read.

.OUTPUTS
System.String

.EXCEPTION
Throws when the environment variable is missing or blank.
#>
function Get-Config {
    param(
        [Parameter(Mandatory)][string]$Name
    )

    $value = [Environment]::GetEnvironmentVariable($Name, "Process")
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "$Name is required. Set it in infra/.env."
    }

    return $value
}

<#
.SYNOPSIS
Reads a required boolean configuration value from the current process environment.

.DESCRIPTION
Accepts common true values: 1, true, yes, y. Accepts common false values:
0, false, no, n.

.PARAMETER Name
Environment variable name to read.

.OUTPUTS
System.Boolean

.EXCEPTION
Throws when the variable is missing, blank, or not a supported boolean value.
#>
function Get-BoolConfig {
    param(
        [Parameter(Mandatory)][string]$Name
    )

    $value = Get-Config -Name $Name

    switch ($value.Trim().ToLowerInvariant()) {
        { $_ -in @("1", "true", "yes", "y") } { return $true }
        { $_ -in @("0", "false", "no", "n") } { return $false }
        default { throw "Invalid boolean value for ${Name}: $value" }
    }
}

<#
.SYNOPSIS
Resolves a file path relative to the repository root.

.DESCRIPTION
Returns an absolute path for either an already-rooted path or a path relative to
the repository root.

.PARAMETER Path
Absolute path or repository-relative path.

.PARAMETER RepoRoot
Absolute path to the repository root.

.OUTPUTS
System.String

.EXCEPTION
Throws when the path cannot be resolved.
#>
function Resolve-RepoPath {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$RepoRoot
    )

    if ([IO.Path]::IsPathRooted($Path)) {
        return (Resolve-Path $Path).Path
    }

    return (Resolve-Path (Join-Path $RepoRoot $Path)).Path
}

<#
.SYNOPSIS
Converts a SecureString into plaintext for Azure CLI parameter passing.

.DESCRIPTION
Azure CLI receives deployment parameters as strings. This helper performs the
conversion in one place and clears the unmanaged buffer afterwards.

.PARAMETER Value
SecureString value to convert.

.OUTPUTS
System.String
#>
function ConvertFrom-SecureStringToPlainText {
    param([Parameter(Mandatory)][securestring]$Value)

    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    } finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    }
}

<#
.SYNOPSIS
Runs an Azure CLI command and fails on a non-zero exit code.

.PARAMETER Arguments
Azure CLI arguments excluding the leading az command.

.EXCEPTION
Throws when Azure CLI returns a non-zero exit code.
#>
function Invoke-Az {
    param([Parameter(Mandatory)][string[]]$Arguments)

    & az @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command failed: az $($Arguments -join ' ')"
    }
}

<#
.SYNOPSIS
Runs an Azure CLI command and parses the JSON output.

.PARAMETER Arguments
Azure CLI arguments excluding the leading az command. The command should emit
JSON output.

.OUTPUTS
System.Object

.EXCEPTION
Throws when Azure CLI returns a non-zero exit code or the output is not valid JSON.
#>
function Invoke-AzJson {
    param([Parameter(Mandatory)][string[]]$Arguments)

    $output = (& az @Arguments) -join [Environment]::NewLine
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command failed: az $($Arguments -join ' ')"
    }

    return $output | ConvertFrom-Json
}

Export-ModuleMember -Function @(
    "ConvertFrom-SecureStringToPlainText",
    "Get-BoolConfig",
    "Get-Config",
    "Import-DotEnv",
    "Invoke-Az",
    "Invoke-AzJson",
    "Resolve-RepoPath"
)
