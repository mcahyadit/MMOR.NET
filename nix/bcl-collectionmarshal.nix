{inputs, ...}: {
  perSystem = {
    pkgs,
    system,
    ...
  }: {
    packages.bcl-collectionsmarshal = let
      pname = "Bcl.CollectionsMarshal";
      nugetDeps = inputs.nuget-packageslock2nix.lib {
        name = "${pname}-nugetDeps";
        inherit system;
        lockfiles = [
          ../third_party/Bcl.CollectionsMarshal/Bcl.CollectionsMarshal/packages.lock.json
          ../third_party/Bcl.CollectionsMarshal/netstandard/packages.lock.json
        ];
      };
    in
      pkgs.buildDotnetModule (finalAttrs: {
        inherit pname;
        version = "2.1.0.0";
        projectFile = "third_party/Bcl.CollectionsMarshal/Bcl.CollectionsMarshal/Bcl.CollectionsMarshal.csproj";
        src = pkgs.lib.fileset.toSource {
          root = ../.;
          fileset = pkgs.lib.fileset.intersection (pkgs.lib.fileset.fromSource
            (pkgs.lib.sources.cleanSource ../.)) (
            pkgs.lib.fileset.unions [
              ../third_party/Bcl.CollectionsMarshal
              ../Directory.Build.props
              ../global.json
            ]
          );
        };

        packNupkg = true;
        dontPublish = true;

        inherit (pkgs) dotnet-sdk;
        dotnet-runtime = finalAttrs.dotnet-sdk;
        DOTNET_ROOT = "${finalAttrs.dotnet-sdk}/share/dotnet";
        inherit nugetDeps;

        meta = {
          license = pkgs.lib.licenses.mit;
        };
      });
  };
}
