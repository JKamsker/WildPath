param(
    [Parameter(Mandatory = $true)]
    [string] $CsvPath,

    [Parameter(Mandatory = $true)]
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Convert-ToNumber([string] $value) {
    $normalized = ($value -replace '[^0-9.,-]', '') -replace ',', ''
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return 0.0
    }

    return [double]::Parse(
        $normalized,
        [System.Globalization.CultureInfo]::InvariantCulture)
}

function Convert-ToBytes([string] $value) {
    $normalized = $value.Trim()
    if ($normalized -eq '-' -or $normalized -eq '') {
        return 0.0
    }

    return Convert-ToNumber $normalized
}

function Get-CaseName([string] $method) {
    return $method -replace '_(DynamicString|GeneratedLiteral|CompiledExpression)$', ''
}

function Get-VariantName([string] $method) {
    if ($method.EndsWith('_DynamicString', [System.StringComparison]::Ordinal)) {
        return 'Dynamic string'
    }

    if ($method.EndsWith('_GeneratedLiteral', [System.StringComparison]::Ordinal)) {
        return 'Generated literal'
    }

    return 'Compiled expression'
}

function Add-Text(
    [System.Text.StringBuilder] $builder,
    [double] $x,
    [double] $y,
    [string] $text,
    [string] $anchor = 'middle',
    [int] $size = 14,
    [string] $weight = '400') {
    $encoded = [System.Security.SecurityElement]::Escape($text)
    [void] $builder.AppendLine(
        "<text x=""$x"" y=""$y"" text-anchor=""$anchor"" font-size=""$size"" font-weight=""$weight"">$encoded</text>")
}

function Import-BenchmarkCsv([string] $path) {
    $lines = Get-Content -Path $path
    $lines = @($lines | Where-Object { [string]::IsNullOrWhiteSpace($_) -eq $false })
    if ($lines.Count -eq 0) {
        throw "Benchmark CSV '$path' is empty."
    }

    $lines[0] = $lines[0].TrimStart([char] 0xFEFF)
    if ($lines[0].StartsWith('sep=', [System.StringComparison]::OrdinalIgnoreCase)) {
        $lines = @($lines | Select-Object -Skip 1)
    }

    $header = $lines[0]
    $semicolonCount = @($header.ToCharArray() | Where-Object { $_ -eq ';' }).Count
    $commaCount = @($header.ToCharArray() | Where-Object { $_ -eq ',' }).Count
    $delimiter = if ($semicolonCount -gt $commaCount) {
        ';'
    }
    else {
        ','
    }

    $rows = $lines | ConvertFrom-Csv -Delimiter $delimiter
    $firstRow = $rows | Select-Object -First 1
    if ($null -eq $firstRow -or
        $null -eq $firstRow.PSObject.Properties['Method'] -or
        $null -eq $firstRow.PSObject.Properties['Mean'] -or
        $null -eq $firstRow.PSObject.Properties['Allocated']) {
        throw "Benchmark CSV '$path' does not contain Method, Mean, and Allocated columns."
    }

    return $rows
}

$rows = Import-BenchmarkCsv $CsvPath | ForEach-Object {
    [pscustomobject]@{
        Case = Get-CaseName $_.Method
        Variant = Get-VariantName $_.Method
        MeanNs = Convert-ToNumber $_.Mean
        AllocatedBytes = Convert-ToBytes $_.Allocated
    }
}

if ($rows.Count -eq 0) {
    throw "No benchmark rows were found in '$CsvPath'."
}

$cases = @('Exact', 'Recursive', 'SimpleWildcard', 'ComplexWildcard', 'Tagged')
$variants = @('Dynamic string', 'Generated literal', 'Compiled expression')
$colors = @{
    'Dynamic string' = '#d95f02'
    'Generated literal' = '#1b9e77'
    'Compiled expression' = '#7570b3'
}

$maxMean = ($rows | Measure-Object -Property MeanNs -Maximum).Maximum
$width = 1180
$height = 720
$left = 110
$right = 40
$top = 84
$bottom = 128
$plotWidth = $width - $left - $right
$plotHeight = $height - $top - $bottom
$groupWidth = $plotWidth / $cases.Count
$barWidth = 42
$barGap = 10
$scale = $plotHeight / $maxMean

$svg = [System.Text.StringBuilder]::new()
[void] $svg.AppendLine("<svg xmlns=""http://www.w3.org/2000/svg"" width=""$width"" height=""$height"" viewBox=""0 0 $width $height"">")
[void] $svg.AppendLine('<rect width="100%" height="100%" fill="#ffffff"/>')
[void] $svg.AppendLine('<g font-family="Segoe UI, Arial, sans-serif" fill="#202124">')
Add-Text $svg ($width / 2) 34 'WildPath path resolution benchmark comparison' 'middle' 22 '700'
Add-Text $svg ($width / 2) 58 'Mean time in nanoseconds; labels include allocated bytes per operation' 'middle' 13

for ($tick = 0; $tick -le 4; $tick++) {
    $value = [math]::Round($maxMean * $tick / 4)
    $y = $top + $plotHeight - ($value * $scale)
    [void] $svg.AppendLine("<line x1=""$left"" y1=""$y"" x2=""$($width - $right)"" y2=""$y"" stroke=""#e8eaed""/>")
    Add-Text $svg ($left - 12) ($y + 5) "$value" 'end' 12
}

[void] $svg.AppendLine("<line x1=""$left"" y1=""$top"" x2=""$left"" y2=""$($top + $plotHeight)"" stroke=""#5f6368""/>")
[void] $svg.AppendLine("<line x1=""$left"" y1=""$($top + $plotHeight)"" x2=""$($width - $right)"" y2=""$($top + $plotHeight)"" stroke=""#5f6368""/>")

for ($caseIndex = 0; $caseIndex -lt $cases.Count; $caseIndex++) {
    $case = $cases[$caseIndex]
    $groupLeft = $left + ($caseIndex * $groupWidth)
    $barsWidth = ($variants.Count * $barWidth) + (($variants.Count - 1) * $barGap)
    $barsLeft = $groupLeft + (($groupWidth - $barsWidth) / 2)

    for ($variantIndex = 0; $variantIndex -lt $variants.Count; $variantIndex++) {
        $variant = $variants[$variantIndex]
        $row = $rows | Where-Object { $_.Case -eq $case -and $_.Variant -eq $variant } | Select-Object -First 1
        if ($null -eq $row) {
            continue
        }

        $barHeight = [math]::Max(1, $row.MeanNs * $scale)
        $x = $barsLeft + ($variantIndex * ($barWidth + $barGap))
        $y = $top + $plotHeight - $barHeight
        $meanLabel = [math]::Round($row.MeanNs, 1)
        $allocLabel = [int] $row.AllocatedBytes

        [void] $svg.AppendLine("<rect x=""$x"" y=""$y"" width=""$barWidth"" height=""$barHeight"" fill=""$($colors[$variant])"" rx=""3""/>")
        Add-Text $svg ($x + ($barWidth / 2)) ($y - 8) "$meanLabel" 'middle' 11
        Add-Text $svg ($x + ($barWidth / 2)) ($top + $plotHeight + 20 + ($variantIndex * 15)) "$allocLabel B" 'middle' 11
    }

    Add-Text $svg ($groupLeft + ($groupWidth / 2)) ($height - 42) $case 'middle' 13 '700'
}

$legendX = $left
$legendY = $height - 24
for ($index = 0; $index -lt $variants.Count; $index++) {
    $variant = $variants[$index]
    $x = $legendX + ($index * 230)
    [void] $svg.AppendLine("<rect x=""$x"" y=""$($legendY - 11)"" width=""14"" height=""14"" fill=""$($colors[$variant])"" rx=""2""/>")
    Add-Text $svg ($x + 22) $legendY $variant 'start' 12
}

Add-Text $svg 24 ($top + ($plotHeight / 2)) 'Mean ns' 'middle' 13 '700'
[void] $svg.AppendLine('</g>')
[void] $svg.AppendLine('</svg>')

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

Set-Content -Path $OutputPath -Value $svg.ToString() -Encoding UTF8
