set -o allexport
source vars.env
set +o allexport

dotnet run -c Release --launch-profile Dev_Blog_Prod