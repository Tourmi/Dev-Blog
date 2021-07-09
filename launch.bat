@echo off

for /f "delims== tokens=1,2" %%G in (vars.env) do set %%G=%%H

dotnet run --launch-profile Dev_Blog_Prod