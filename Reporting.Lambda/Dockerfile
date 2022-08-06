FROM  public.ecr.aws/lambda/dotnet:6 as base

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-image
WORKDIR /app

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish --no-restore -c Release -o /publish

FROM base AS final
WORKDIR /var/task
COPY --from=build-image /publish .
CMD ["Reporting.Lambda::Reporting.Lambda.Function::FunctionHandler"]