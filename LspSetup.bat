del /f /q *.csproj
del /f /q *.sln
del /f /q *.slnx

dotnet new classlib -f netstandard2.1 --langVersion 9.0
del /f Class1.cs
dotnet add package "Newtonsoft.Json"
dotnet add package "System.Collections.Immutable"

dotnet new sln
dotnet sln add .

dotnet restore

