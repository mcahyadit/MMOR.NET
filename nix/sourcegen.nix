{inputs, ...}: {
  perSystem = {
    pkgs,
    system,
    ...
  }: {
    packages.sourcegen = let
      pname = "MMOR.Roslyn";
      version = pkgs.lib.strings.trim (builtins.readFile ../analyzers/Roslyn/VERSION.txt);
      nugetDeps = inputs.nuget-packageslock2nix.lib {
        name = "${pname}-${version}-nugetDeps";
        inherit system;
        lockfiles = [
          ../analyzers/Roslyn/packages.lock.json
          ../analyzers/SourceGen/packages.lock.json
        ];
      };
    in
      pkgs.buildDotnetModule (finalAttrs: {
        inherit pname version;
        src = pkgs.lib.fileset.toSource {
          root = ../.;
          fileset = pkgs.lib.fileset.intersection (pkgs.lib.fileset.fromSource
            (pkgs.lib.sources.cleanSource ../.)) (
            pkgs.lib.fileset.unions [
              ../analyzers
              ../Directory.Build.props
            ]
          );
        };

        packNupkg = true;
        dontPublish = true;

        inherit (pkgs) dotnet-sdk;
        dotnet-runtime = finalAttrs.dotnet-sdk;
        # Needed by checkPhase to find the dotnet in path
        DOTNET_ROOT = "${finalAttrs.dotnet-sdk}/share/dotnet";

        inherit nugetDeps;

        projectFile = "analyzers/Roslyn/MMOR.Roslyn.csproj";

        meta = {
          license = pkgs.lib.licenses.mit;
        };
      });
  };
}
