#!/bin/zsh
# Updates the database to the latest migration (or a specific one)
# Executable:
#   chmod +x ef-update.sh
# Usage:
#   ./ef-database-update.sh              → applies all pending migrations
#   ./ef-database-update.sh Initial      → updates to a specific migration
#   ./ef-database-update.sh 0            → rolls back all migrations

MIGRATION_TARGET=${1:-""}  # empty = latest

dotnet ef database update $MIGRATION_TARGET \
  --project CampusLibraryApi_4_Infrastructure/CampusLibraryApi_4_Infrastructure.csproj \
  --startup-project CampusLibraryApi/CampusLibraryApi.csproj \
  --context CampusLibraryApi._4_Infrastructure.Persistence.Database.AppDbContext \
  --configuration Debug