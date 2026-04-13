# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
# Copia a solution e todos os projetos
COPY QuestionarioOnline.sln .
COPY QuestionarioOnline/ QuestionarioOnline/
COPY QuestionarioOnline.Application/ QuestionarioOnline.Application/
COPY QuestionarioOnline.CrossCutting/ QuestionarioOnline.CrossCutting/
COPY QuestionarioOnline.Domain/ QuestionarioOnline.Domain/
COPY QuestionarioOnline.Infrastructure/ QuestionarioOnline.Infrastructure/
COPY QuestionarioOnline.Workers.Function/ QuestionarioOnline.Workers.Function/
RUN dotnet restore QuestionarioOnline.sln
RUN dotnet publish QuestionarioOnline/QuestionarioOnline.Api.csproj -c Release -o out

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "QuestionarioOnline.Api.dll"]
