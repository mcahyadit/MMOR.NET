#!/usr/bin/env -S bash -x

# Remove older stuffs
rm *.csproj
rm *.sln

dotnet new classlib -f netstandard2.1 --langVersion 9.0
rm Class1.cs
dotnet package add "Newtonsoft.Json" --project *.csproj
dotnet package add "System.Collections.Immutable" --project *.csproj

dotnet new sln
dotnet sln add *.csproj

dotnet restore

