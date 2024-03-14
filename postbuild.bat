@ECHO OFF

IF EXIST %cd%\BIN\DEBUG\net8.0-windows\MMA.deps.json DEL %cd%\BIN\DEBUG\net8.0-windows\MMA.deps.json
IF EXIST %cd%\BIN\DEBUG\net8.0-windows\MMA.pdb DEL %cd%\BIN\DEBUG\net8.0-windows\MMA.pdb

powershell -Command "Compress-Archive -Path: %cd%\BIN\DEBUG\net8.0-windows\MMA.* -DestinationPath %cd%\LatestBuild\MMA.zip -Update"

>> "%cd%\LatestBuild\MMA.zi_" REM/
xcopy /Y /F %cd%\LatestBuild\MMA.zip %cd%\LatestBuild\MMA.zi_

IF EXIST %cd%\LatestBuild\MMA.zip DEL %cd%\LatestBuild\MMA.zip
