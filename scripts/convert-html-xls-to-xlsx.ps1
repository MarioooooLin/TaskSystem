param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

function Escape-Xml([string]$Value) {
    if ($null -eq $Value) { return '' }
    return [System.Security.SecurityElement]::Escape($Value)
}

function Get-ColumnName([int]$Index) {
    $name = ''
    while ($Index -gt 0) {
        $Index--
        $name = [char](65 + ($Index % 26)) + $name
        $Index = [math]::Floor($Index / 26)
    }
    return $name
}

function Get-PlainText([string]$Html) {
    $text = [regex]::Replace($Html, '<br\s*/?>', "`n", 'IgnoreCase')
    $text = [regex]::Replace($text, '<[^>]+>', '')
    return [System.Net.WebUtility]::HtmlDecode($text).Trim()
}

$inputFullPath = [System.IO.Path]::GetFullPath($InputPath)
$outputFullPath = [System.IO.Path]::GetFullPath($OutputPath)
$html = [System.IO.File]::ReadAllText($inputFullPath, [System.Text.Encoding]::UTF8)

$sheetNames = [regex]::Matches(
    $html,
    '<x:ExcelWorksheet><x:Name>(.*?)</x:Name>',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
) | ForEach-Object { [System.Net.WebUtility]::HtmlDecode($_.Groups[1].Value) }

$sheetMatches = [regex]::Matches(
    $html,
    '<div class="sheet">(.*?)</div>',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

if ($sheetMatches.Count -eq 0) {
    throw "No worksheet sections were found in $InputPath."
}

if ($sheetNames.Count -ne $sheetMatches.Count) {
    throw "Worksheet name count ($($sheetNames.Count)) does not match section count ($($sheetMatches.Count))."
}

$buildRoot = Join-Path ([System.IO.Path]::GetDirectoryName($outputFullPath)) ('.xlsx_' + [guid]::NewGuid().ToString('N'))
$relsRoot = Join-Path $buildRoot '_rels'
$xlRoot = Join-Path $buildRoot 'xl'
$xlRelsRoot = Join-Path $xlRoot '_rels'
$worksheetsRoot = Join-Path $xlRoot 'worksheets'

[System.IO.Directory]::CreateDirectory($relsRoot) | Out-Null
[System.IO.Directory]::CreateDirectory($xlRelsRoot) | Out-Null
[System.IO.Directory]::CreateDirectory($worksheetsRoot) | Out-Null

$utf8 = [System.Text.UTF8Encoding]::new($false)

try {
    for ($sheetIndex = 0; $sheetIndex -lt $sheetMatches.Count; $sheetIndex++) {
        $sheetBody = $sheetMatches[$sheetIndex].Groups[1].Value
        $rows = [regex]::Matches(
            $sheetBody,
            '<tr>(.*?)</tr>',
            [System.Text.RegularExpressions.RegexOptions]::Singleline
        )

        $sheetPath = Join-Path $worksheetsRoot ('sheet{0}.xml' -f ($sheetIndex + 1))
        $writer = [System.IO.StreamWriter]::new($sheetPath, $false, $utf8)
        try {
            $writer.Write('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>')
            $writer.Write('<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">')
            $writer.Write('<sheetViews><sheetView workbookViewId="0"><pane ySplit="1" topLeftCell="A2" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews>')
            $writer.Write('<sheetFormatPr defaultRowHeight="18"/>')
            $writer.Write('<cols><col min="1" max="1" width="24" customWidth="1"/><col min="2" max="2" width="34" customWidth="1"/><col min="3" max="3" width="24" customWidth="1"/><col min="4" max="4" width="12" customWidth="1"/><col min="5" max="5" width="44" customWidth="1"/><col min="6" max="20" width="54" customWidth="1"/></cols>')
            $writer.Write('<sheetData>')

            for ($rowIndex = 0; $rowIndex -lt $rows.Count; $rowIndex++) {
                $rowNumber = $rowIndex + 1
                $cells = [regex]::Matches(
                    $rows[$rowIndex].Groups[1].Value,
                    '<t[hd][^>]*>(.*?)</t[hd]>',
                    [System.Text.RegularExpressions.RegexOptions]::Singleline
                )

                $writer.Write('<row r="{0}">' -f $rowNumber)
                for ($cellIndex = 0; $cellIndex -lt $cells.Count; $cellIndex++) {
                    $columnNumber = $cellIndex + 1
                    $cellReference = (Get-ColumnName $columnNumber) + $rowNumber
                    $cellText = Get-PlainText $cells[$cellIndex].Groups[1].Value
                    $style = if ($rowIndex -eq 0) { ' s="1"' } else { '' }
                    $writer.Write(
                        '<c r="' + $cellReference + '" t="inlineStr"' + $style +
                        '><is><t xml:space="preserve">' + (Escape-Xml $cellText) +
                        '</t></is></c>'
                    )
                }
                $writer.Write('</row>')
            }

            $writer.Write('</sheetData><autoFilter ref="A1:F1"/></worksheet>')
        }
        finally {
            $writer.Dispose()
        }
    }

    $workbookSheets = [System.Text.StringBuilder]::new()
    $workbookRelationships = [System.Text.StringBuilder]::new()
    $contentTypeOverrides = [System.Text.StringBuilder]::new()

    for ($i = 0; $i -lt $sheetNames.Count; $i++) {
        $sheetId = $i + 1
        [void]$workbookSheets.Append(
            '<sheet name="' + (Escape-Xml $sheetNames[$i]) +
            '" sheetId="' + $sheetId + '" r:id="rId' + $sheetId + '"/>'
        )
        [void]$workbookRelationships.Append(
            '<Relationship Id="rId{0}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet{0}.xml"/>' -f
            $sheetId
        )
        [void]$contentTypeOverrides.Append(
            '<Override PartName="/xl/worksheets/sheet{0}.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>' -f
            $sheetId
        )
    }

    $stylesRelationshipId = $sheetNames.Count + 1
    [void]$workbookRelationships.Append(
        '<Relationship Id="rId{0}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>' -f
        $stylesRelationshipId
    )

    $workbookXml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' +
        '<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">' +
        '<sheets>' + $workbookSheets + '</sheets></workbook>'

    $workbookRelsXml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' +
        '<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">' +
        $workbookRelationships +
        '</Relationships>'

    $rootRelsXml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' +
        '<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">' +
        '<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>' +
        '</Relationships>'

    $stylesXml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' +
        '<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">' +
        '<fonts count="2"><font><sz val="11"/><name val="Microsoft JhengHei"/></font><font><b/><sz val="11"/><name val="Microsoft JhengHei"/></font></fonts>' +
        '<fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FFD9E2F3"/><bgColor indexed="64"/></patternFill></fill></fills>' +
        '<borders count="2"><border><left/><right/><top/><bottom/><diagonal/></border><border><left style="thin"/><right style="thin"/><top style="thin"/><bottom style="thin"/><diagonal/></border></borders>' +
        '<cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>' +
        '<cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf><xf numFmtId="0" fontId="1" fillId="1" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf></cellXfs>' +
        '</styleSheet>'

    $contentTypesXml = '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' +
        '<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">' +
        '<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>' +
        '<Default Extension="xml" ContentType="application/xml"/>' +
        '<Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>' +
        '<Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>' +
        $contentTypeOverrides +
        '</Types>'

    [System.IO.File]::WriteAllText((Join-Path $xlRoot 'workbook.xml'), $workbookXml, $utf8)
    [System.IO.File]::WriteAllText((Join-Path $xlRelsRoot 'workbook.xml.rels'), $workbookRelsXml, $utf8)
    [System.IO.File]::WriteAllText((Join-Path $relsRoot '.rels'), $rootRelsXml, $utf8)
    [System.IO.File]::WriteAllText((Join-Path $xlRoot 'styles.xml'), $stylesXml, $utf8)
    [System.IO.File]::WriteAllText((Join-Path $buildRoot '[Content_Types].xml'), $contentTypesXml, $utf8)

    if ([System.IO.File]::Exists($outputFullPath)) {
        [System.IO.File]::Delete($outputFullPath)
    }

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $outputStream = [System.IO.File]::Open(
        $outputFullPath,
        [System.IO.FileMode]::CreateNew,
        [System.IO.FileAccess]::ReadWrite
    )
    try {
        $archive = [System.IO.Compression.ZipArchive]::new(
            $outputStream,
            [System.IO.Compression.ZipArchiveMode]::Create,
            $false
        )
        try {
            foreach ($file in [System.IO.Directory]::EnumerateFiles($buildRoot, '*', [System.IO.SearchOption]::AllDirectories)) {
                $relativePath = $file.Substring($buildRoot.Length).TrimStart('\', '/').Replace('\', '/')
                $entry = $archive.CreateEntry($relativePath, [System.IO.Compression.CompressionLevel]::Optimal)
                $entryStream = $entry.Open()
                $inputStream = [System.IO.File]::OpenRead($file)
                try {
                    $inputStream.CopyTo($entryStream)
                }
                finally {
                    $inputStream.Dispose()
                    $entryStream.Dispose()
                }
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $outputStream.Dispose()
    }
}
finally {
    if ([System.IO.Directory]::Exists($buildRoot)) {
        [System.IO.Directory]::Delete($buildRoot, $true)
    }
}

Get-Item -LiteralPath $outputFullPath |
    Select-Object FullName, Length, LastWriteTime
