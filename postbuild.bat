@ECHO OFF

powershell -Command "$files = Get-ChildItem -LiteralPath '%cd%\BIN\DEBUG\net8.0-windows' -Filter 'MMA.*' | Where-Object { $_.Name -notin @('MMA.deps.json', 'MMA.pdb') }; Compress-Archive -Path $files.FullName -DestinationPath '%cd%\LatestBuild\MMA.zip' -Update"

>> "%cd%\LatestBuild\MMA.zi_" REM/
xcopy /Y /F %cd%\LatestBuild\MMA.zip %cd%\LatestBuild\MMA.zi_

IF EXIST %cd%\LatestBuild\MMA.zip DEL %cd%\LatestBuild\MMA.zip
