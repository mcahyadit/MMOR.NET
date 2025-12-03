#!/usr/bin/env -S bash -exo pipefail

rm -f *.csproj
rm -f *.sln
rm -f *.slnx

dotnet new classlib -f netstandard2.1 --langVersion 9.0
rm Class1.cs
dotnet add package "Newtonsoft.Json"
dotnet add package "System.Collections.Immutable"

dotnet new sln
dotnet sln add .

dotnet restore

