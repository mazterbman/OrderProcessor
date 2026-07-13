# ===== Этап 1: сборка (SDK) =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файлы проекта и восстанавливаем зависимости
COPY . .
RUN dotnet restore

# Публикуем в папку /app/publish
RUN dotnet publish OrderProcessor.API/OrderProcessor.API.csproj -c Release -o /app/publish

# ===== Этап 2: запуск (runtime) =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Переносим из этапа build только готовые файлы
COPY --from=build /app/publish .

# Точка входа: чем запускаем приложение?
ENTRYPOINT ["dotnet", "OrderProcessor.API.dll"]