# Open Weather Service

## Build

dotnet build -c Release; dotnet pack -c Release


## Install

dotnet tool uninstall --global OpenWeatherService; dotnet tool install --global --add-source ./ OpenWeatherService


## Verify Install

dotnet tool list -g


## Run

ow *args*

