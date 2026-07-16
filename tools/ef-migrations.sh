#!/bin/zsh
# Executable:
#    chmod +x ef-update.sh
# Usage:
#    ./ef-migrations.sh <MigrationName>
# Example:
#    ./ef-migrations.sh Initial

MIGRATION_NAME=${1:?"Migration name required. Usage: ./ef-migrations.sh <MigrationName>"}

dotnet ef migrations add "$MIGRATION_NAME" \
  --project CampusLibraryApi_4_Infrastructure/CampusLibraryApi_4_Infrastructure.csproj \
  --startup-project CampusLibraryApi/CampusLibraryApi.csproj \
  --context CampusLibraryApi._4_Infrastructure.Persistence.Database.AppDbContext \
  --configuration Debug \
  --output-dir _4_Infrastructure/Persistence/Migrations
