{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    systems.url = "github:nix-systems/default";
    flake-parts.url = "github:hercules-ci/flake-parts";

    nuget-packageslock2nix = {
      url = "github:mdarocha/nuget-packageslock2nix";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs = {...} @ inputs:
    inputs.flake-parts.lib.mkFlake {inherit inputs;} {
      systems = import inputs.systems;
      perSystem = {
        self',
        pkgs,
        system,
        ...
      }: let
        pname = "MMOR.NET";
        version = "2.2.16.2";

        dotnet-sdk = with pkgs.dotnetCorePackages;
          combinePackages [
            sdk_10_0
            sdk_9_0
            sdk_8_0
          ];
        DOTNET_ROOT = "${dotnet-sdk}/share/dotnet";

        mkDotnetLib = tfm:
          self'.packages.default.overrideAttrs (old: {
            packNupkg = false;
            dontPublish = false;
            doCheck = true;
            dotnet-sdk = pkgs.dotnetCorePackages.${"sdk_${tfm}_0"};
            dotnet-runtime = pkgs.dotnetCorePackages.${"runtime_${tfm}_0"};
            dotnetFlags = "-p:TargetFramework=net${tfm}.0";
          });
      in {
        packages = {
          default = pkgs.buildDotnetModule (finalAttrs: {
            inherit pname version;
            src = pkgs.lib.fileset.toSource {
              root = ./.;
              fileset = pkgs.lib.fileset.intersection (pkgs.lib.fileset.fromSource (pkgs.lib.sources.cleanSource ./.)) (
                pkgs.lib.fileset.unions [
                  ./src
                  ./MMOR.NET.csproj
                  ./packages.lock.json
                ]
              );
            };

            packNupkg = true;
            dontPublish = true;

            inherit dotnet-sdk;
            dotnet-runtime = finalAttrs.dotnet-sdk;
            # Needed by checkPhase to find the dotnet in path
            inherit DOTNET_ROOT;

            buildInputs = [
              (pkgs.dotnetCorePackages.fetchNupkg {
                # Issue with nixpkgs.dotnet-sdk-10
                # Patch in NETStandard2.1
                pname = "NETStandard.Library.Ref";
                version = "2.1.0";
                hash = "sha256-Ruovy9EKgXaFuFr3zgw5fRKUS9yBIJ4nLeHgXv0zx4o=";
              })
            ];

            nugetDeps = inputs.nuget-packageslock2nix.lib {
              name = "${pname}-${version}-nugetDeps";
              inherit system;
              lockfiles = [
                ./packages.lock.json
              ];
            };

            meta = {
              license = pkgs.lib.licenses.mit;
            };
          });

          docs = pkgs.callPackage ./nix/docs.nix {
            inherit pkgs pname version;
            assemblies = [self'.packages.dotnet-8];
          };

          dotnet-8 = mkDotnetLib "8";
          dotnet-9 = mkDotnetLib "9";
          dotnet-10 = mkDotnetLib "10";
        };

        apps.docs = {
          type = "app";
          program = "${pkgs.writeShellScriptBin "serve" ''
            ${pkgs.python3}/bin/python3 -m http.server 8000 \
              --directory ${self'.packages.docs}
          ''}/bin/serve";
        };

        checks = {
          default = self'.packages.default;
        };

        devShells.default = pkgs.mkShellNoCC {
          inputsFrom = builtins.attrValues self'.packages;
          packages =
            [self'.formatter]
            ++ (with pkgs; [
              roslyn-ls
              vscode-langservers-extracted
              clang-tools
              xmlstarlet

              prettierd

              just
              just-lsp

              prek
              nixd
            ]);
          env = {inherit DOTNET_ROOT;};
        };
        formatter = pkgs.alejandra;
      };
    };
}
