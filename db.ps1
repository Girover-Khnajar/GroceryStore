param(
    [Alias("MigrationName")]
    [string]$Name = ""
)

# Paths to project files
$infrastructureProject = "src/backend/GroceryStore.Infrastructure"
$startupProject = "src/backend/GroceryStore.Api"

# Step 1: Force build both projects first
#Write-Host "Building Infrastructure and API projects..."
#dotnet build $infrastructureProject --configuration Debug
#dotnet build $startupProject --configuration Debug

# Add migration if a name is provided
if (![string]::IsNullOrWhiteSpace($Name)) {
    Write-Host "Adding migration: $Name"
    dotnet ef migrations add $Name `
        --project $infrastructureProject `
        --startup-project $startupProject `
        --output-dir Persistence/Migrations
}

# Apply the migration
Write-Host "Applying migration to database..."
dotnet ef database update `
    --project $infrastructureProject `
    --startup-project $startupProject

Write-Host "Done!"
