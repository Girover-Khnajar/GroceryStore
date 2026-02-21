param(
    [Parameter(Position = 0)]
    [ValidateSet("api", "ui")]
    [string]$Project = "api"
)

switch ($Project) {
    "api" {
        Write-Host "Starting GroceryStore API..." -ForegroundColor Cyan
        dotnet run --project "src/backend/GroceryStore.Api"
    }
    "ui" {
        Write-Host "Starting GroceryStore UI..." -ForegroundColor Cyan
        dotnet watch run --project "src/frontend/GroceryStore.Ui"
    }
}
