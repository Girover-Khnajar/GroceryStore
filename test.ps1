# This script is used to run specific test projects in a .NET solution.
# It allows the user to specify a keyword to run a specific test project or run all test projects if no keyword is provided.
# Usage:
#   - To run all test projects: .\run-tests.ps1
#   - To run a specific test project: .\run-tests.ps1 <keyword>
# Example:
#   - To run the domain tests: .\run-tests.ps1 domain
#   - To run the application tests: .\run-tests.ps1 application
#   - To run the infrastructure tests: .\run-tests.ps1 infrastructure
#   - To run the API tests: .\run-tests.ps1 api
#   - To run the shared tests: .\run-tests.ps1 shared
#   - To run the integration tests: .\run-tests.ps1 integration
# This script is designed to be run in a PowerShell environment with .NET SDK installed.
param(
    [string]$target
)

# Map keyword -> test project path
$testProjects = @{
    "domain"         = ".\tests\backend\GroceryStore.Domain.Tests\GroceryStore.Domain.Tests.csproj"
    "application"    = ".\tests\backend\GroceryStore.Application.Tests\GroceryStore.Application.Tests.csproj"
    "infrastructure" = ".\tests\backend\GroceryStore.Infrastructure.Tests\GroceryStore.Infrastructure.Tests.csproj"
    "api"            = ".\tests\backend\GroceryStore.Api.Tests\GroceryStore.Api.Tests.csproj"
    "app"            = ".\tests\frontend\GroceryStore.App.Tests\GroceryStore.App.Tests.csproj"
    "cqrs"           = ".\src\libs\CQRS\tests\CQRS.Tests.csproj"
}

# Build the solution before running tests
Write-Host " Building the solution..." -ForegroundColor Cyan
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host " Build failed. Exiting..." -ForegroundColor Red
    exit $LASTEXITCODE
}

function Run-Test {
    param (
        [string]$projectPath
    )

    Write-Host "`n Running tests in: $projectPath" -ForegroundColor Cyan
    dotnet test $projectPath --no-build --logger "console;verbosity=normal"
}

if ([string]::IsNullOrWhiteSpace($target)) {
    Write-Host " Running ALL test projects..." -ForegroundColor Green
    foreach ($proj in $testProjects.Values) {
        Run-Test -projectPath $proj
    }
}
else {
    $key = $testProjects.Keys | Where-Object { $_ -like "*$target*" } | Select-Object -First 1
    if ($key) {
        Write-Host " Running test project for '$key'" -ForegroundColor Yellow
        Run-Test -projectPath $testProjects[$key]
    }
    else {
        Write-Host " No test project found for keyword '$target'" -ForegroundColor Red
        Write-Host " Available options:" -ForegroundColor Yellow

        $i = 1
        foreach ($opt in $testProjects.Keys) {
            Write-Host "   $i- $opt" -ForegroundColor Yellow
            $i++
        }

        Write-Host " Use 'run-tests.ps1 <keyword>' to run a specific test project." -ForegroundColor Cyan
        Write-Host " For example: 'run-tests.ps1 domain' to run the domain tests." -ForegroundColor Cyan
        Write-Host " Or 'run-tests.ps1' to run all projects." -ForegroundColor Yellow
    }
}
