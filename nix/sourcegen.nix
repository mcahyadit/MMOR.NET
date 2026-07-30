{
  inputs,
  lib,
  ...
}: {
  perSystem = {
    self',
    pkgs,
    system,
    ...
  }: {
    packages.sourcegen = let
      pname = "MMOR.NET.SourceGen";
      version = lib.strings.trim (builtins.readFile ../VERSION.txt);
      nugetDeps = inputs.nuget-packageslock2nix.lib {
        name = "${pname}-${version}-nugetDeps";
        inherit system;
        lockfiles = [
          ../analyzers/SourceGen/packages.lock.json
        ];
      };
    in
      pkgs.buildDotnetModule (finalAttrs: {
        inherit pname version;
        src = pkgs.lib.sources.cleanSource ../analyzers/SourceGen;

        packNupkg = true;
        dontPublish = true;

        inherit (pkgs) dotnet-sdk;
        dotnet-runtime = finalAttrs.dotnet-sdk;
        # Needed by checkPhase to find the dotnet in path
        DOTNET_ROOT = "${finalAttrs.dotnet-sdk}/share/dotnet";

        inherit nugetDeps;

        meta = {
          license = pkgs.lib.licenses.mit;
        };
      });
  };
}
