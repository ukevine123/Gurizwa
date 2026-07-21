#!/bin/bash

# Function to display usage
function usage() {
    echo "Usage: $0 [--migrate <MigrationName>] [--drop] [--update] [--script]"
    exit 1
}

# Check if at least one argument is provided
if [ -z "$1" ]; then
    echo "Error: No arguments provided."
    usage
fi

# Go into the src/Web directory
cd src/Web || exit 1

# Parse the first argument
case "$1" in
    --migrate)
        # Check if a migration name is provided
        if [ -z "$2" ]; then
            echo "Error: Migration name is required for --migrate."
            usage
        fi
        MIGRATION_NAME=$2
        echo "Running migration with name: $MIGRATION_NAME"
        dotnet ef migrations add "$MIGRATION_NAME" --project ../Infrastructure/Infrastructure.csproj
        ;;
    --drop)
        echo "Dropping the database..."
        dotnet ef database drop --project ../Infrastructure/Infrastructure.csproj
        ;;
    --update)
        echo "Updating the database..."
        dotnet ef database update --project ../Infrastructure/Infrastructure.csproj
        ;;
    --script)
        echo "Generating SQL script..."
        dotnet ef migrations script --idempotent --project ../Infrastructure/Infrastructure.csproj --output ./migrations.sql
        ;;
    --remove)
        echo "Removing the last migration..."
        dotnet ef migrations remove --project ../Infrastructure/Infrastructure.csproj
        ;;
    *)
        echo "Error: Invalid argument."
        usage
        ;;
esac