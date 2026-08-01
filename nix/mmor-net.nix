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
    packages.mmor-net = let
      pname = "MMOR.NET";
      version = lib.strings.trim (builtins.readFile ../VERSION.txt);
      nugetDeps = inputs.nuget-packageslock2nix.lib {
        name = "${pname}-${version}-nugetDeps";
        inherit system;
        lockfiles = [
          ../packages.lock.json
          ../tests/packages.lock.json
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
              ../src
              ../tests
              ../MMOR.NET.csproj
              ../Directory.Build.props
              ../packages.lock.json
              ../VERSION.txt
            ]
          );
        };

        packNupkg = true;
        dontPublish = true;

        inherit (pkgs) dotnet-sdk;
        dotnet-runtime = finalAttrs.dotnet-sdk;
        # Needed by checkPhase to find the dotnet in path
        DOTNET_ROOT = "${finalAttrs.dotnet-sdk}/share/dotnet";

        buildInputs = [
          self'.packages.sourcegen
          (pkgs.dotnetCorePackages.fetchNupkg {
            # Issue with nixpkgs.dotnet-sdk-10
            # Patch in NETStandard2.1
            pname = "NETStandard.Library.Ref";
            version = "2.1.0";
            hash = "sha256-Ruovy9EKgXaFuFr3zgw5fRKUS9yBIJ4nLeHgXv0zx4o=";
          })
          (pkgs.callPackage ../third_party/Bcl.CollectionsMarshal/default.nix {
            inherit pkgs;
            inherit (finalAttrs) dotnet-sdk;
            inherit (finalAttrs) dotnet-runtime;
          })
        ];
        inherit nugetDeps;

        testProject = "tests/MMOR.NET.Tests.csproj";
        doCheck = true;

        meta = {
          license = pkgs.lib.licenses.mit;
        };
      });
    checks = {
      unit-test = self'.packages.mmor-net;
    };
  };
}
