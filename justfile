configure:
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

clean:
  rm -f *.csproj
  rm -f *.sln
  rm -f *.slnx
  rm -rf ./build
  rm -rf ./bin
  rm -rf ./obj
  rm -rf ./docs/api

docs:
  ls *.csproj >/dev/null 2>&1 || just configure
  docfx ./docs/docfx.json --hostname 0.0.0.0 -o build/docfx --serve
