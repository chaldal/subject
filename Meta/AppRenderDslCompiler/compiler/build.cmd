setlocal
cd /d %~dp0
rmdir bin /s /q
rmdir obj /s /q
dotnet build -c Release