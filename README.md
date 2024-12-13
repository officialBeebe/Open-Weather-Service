# Open Weather Service

This is a .NET CLI tool for *Open Weather API* written using C#. Once configured it can be built and installed to your system using the instructions below.

## Configure

This application requires an Open Weather API key to function. You can signup for a key [here](http://www.example.com/.

Simply include this key in *config.conf* and follow the next step to build.

## Build

```dotnet build -c Release; dotnet pack -c Release```


## Install

Navigate to the /bin/release directory.

```dotnet tool install --global --add-source ./ OpenWeatherService```

## Run

TODO: Include a breakdown of CLI options [lat/long, key?]

```ow```

