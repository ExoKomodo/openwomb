FROM exokomodo/development:womb-base-latest

ARG DOTNET_BUILD_CONFIGURATION=Debug

RUN dotnet restore

RUN dotnet build --no-restore \
	--configuration ${DOTNET_BUILD_CONFIGURATION} \
	Womb
