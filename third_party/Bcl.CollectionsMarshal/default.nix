{
  pkgs,
  dotnet-sdk,
  dotnet-runtime,
  ...
}:
pkgs.buildDotnetModule {
  pname = "Bcl.CollectionsMarshal";
  version = "2.1.0.0";
  src = pkgs.lib.fileset.toSource {
    root = ./.;
    fileset = pkgs.lib.fileset.intersection (pkgs.lib.fileset.fromSource (pkgs.lib.sources.cleanSource ./.)) (
      pkgs.lib.fileset.unions [
        ./Bcl.CollectionsMarshal
        ./netstandard
        ./Directory.Build.props
      ]
    );
  };
  projectFile = "./Bcl.CollectionsMarshal/Bcl.CollectionsMarshal.csproj";
  packNupkg = true;

  dotnetFlags = "-p:TargetFramework=netstandard2.1";

  inherit dotnet-sdk;
  inherit dotnet-runtime;

  meta = {
    description = "CollectionsMarshal for .NET Standard 2.0/2.1";
    homepage = "https://github.com/DaZombieKiller/Bcl.CollectionsMarshal";
    license = pkgs.lib.licenses.mit;
  };
}
