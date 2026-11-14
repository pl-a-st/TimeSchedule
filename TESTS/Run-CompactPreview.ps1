param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [string]$InputLegacyFile = ''
)
$ErrorActionPreference = 'Stop'
$proj = Join-Path $PSScriptRoot 'CompactStorePreviewTests.csproj'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (Test-Path $vswhere) {
    $build = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $build) { $build = (Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source }
if (-not $build) { throw 'Не найден MSBuild из Visual Studio. Откройте Developer PowerShell for Visual Studio.' }
& $build $proj '/t:Build' "/p:Configuration=$Configuration" '/p:Platform=AnyCPU' '/v:minimal' '/nologo'
if ($LASTEXITCODE -ne 0) { throw 'Сборка не прошла: реальные данные не затрагивались.' }
$exe = Join-Path $PSScriptRoot "bin\$Configuration\CompactStorePreviewTests.exe"
if (-not (Test-Path $exe)) { throw "Нет тестового EXE: $exe" }
$testRoot = Join-Path $env:TEMP ("TimeSchedule-CompactPreview-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null
$testLog = Join-Path $testRoot 'synthetic-tests.txt'
& $exe *>&1 | Out-File $testLog -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw "Синтетические тесты не прошли. Журнал: $testLog" }
Write-Host "PASS: $testLog"
if ($InputLegacyFile) {
    # Реальные данные только читаются и сначала копируются в TEMP. Прямой эксперимент на рабочем пути запрещён.
    $legacyTemp = Join-Path $testRoot 'private-copy'
    New-Item -ItemType Directory -Path $legacyTemp | Out-Null
    $copied = Join-Path $legacyTemp 'Tasks.bin'
    $sourceHashBefore = (Get-FileHash -LiteralPath $InputLegacyFile -Algorithm SHA256).Hash
    Copy-Item -LiteralPath $InputLegacyFile -Destination $copied -ErrorAction Stop
    $sourceHashAfter = (Get-FileHash -LiteralPath $InputLegacyFile -Algorithm SHA256).Hash
    $copiedHash = (Get-FileHash -LiteralPath $copied -Algorithm SHA256).Hash
    if ($sourceHashBefore -ne $sourceHashAfter -or $copiedHash -ne $sourceHashAfter) {
        throw 'Tasks.bin изменился во время копирования. Остановите работу с ним и повторите проверку на стабильной копии.'
    }
    $realLog = Join-Path $testRoot 'private-copy-metrics.txt'
    foreach ($size in @(64, 128, 256)) {
        $store = Join-Path $testRoot ("private-segments-$size")
        & $exe --preview-copy $copied $store $size *>&1 | Out-File $realLog -Encoding utf8 -Append
        if ($LASTEXITCODE -ne 0) { throw "Проверка совместимости для размера $size не прошла. Журнал: $realLog" }
    }
    Write-Host "PASS: $realLog"
    Write-Host 'Не передавайте в чат временные файлы с копией реального Tasks.bin. Достаточно журналов.'
}
