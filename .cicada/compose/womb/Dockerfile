FROM exokomodo/development:womb-cicada-base-latest

ARG DOTNET_BUILD_CONFIGURATION=Debug
RUN ls

RUN dotnet restore

RUN dotnet build --no-restore \
	--configuration ${DOTNET_BUILD_CONFIGURATION} \
	Womb
