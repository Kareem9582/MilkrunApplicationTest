Param(
    [string]$Username = "test_user",
    [string]$Password = "test_password"
)

dotnet user-secrets init

# Set credentials
dotnet user-secrets set "BasicAuth:Username" "$Username" 
dotnet user-secrets set "BasicAuth:Password" "$Password"


