#! /bin/bash

PROJECT=$1
shift

dotnet restore
dotnet build --no-restore --configuration ${DOTNET_BUILD_CONFIGURATION} ${PROJECT}
