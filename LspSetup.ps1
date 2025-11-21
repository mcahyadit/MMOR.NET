#!/usr/bin/env -S pwsh -noprofile

# Remove older stuffs
Remove-Item -Path "*.csproj", "*.sln" -Force -ErrorAction SilentlyContinue

dotnet new classlib -f netstandard2.1 --langVersion 9.0
Remove-Item -Path "Class1.cs" -Force -ErrorAction SilentlyContinue
dotnet add (Get-Item -Path "*.csproj").Name package "Newtonsoft.Json"
dotnet add (Get-Item -Path "*.csproj").Name package "System.Collections.Immutable"

dotnet new sln
dotnet sln add (Get-Item -Path "*.csproj").Name

dotnet restore
