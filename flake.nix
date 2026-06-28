{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    systems.url = "github:nix-systems/default";
    flake-parts.url = "github:hercules-ci/flake-parts";
    treefmt-nix = {
      url = "github:numtide/treefmt-nix";
      inputs.nixpkgs.follows = "nixpkgs";
    };

    nuget-packageslock2nix = {
      url = "github:mdarocha/nuget-packageslock2nix";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs = {...} @ inputs:
    inputs.flake-parts.lib.mkFlake {inherit inputs;} {
      imports = [
        inputs.treefmt-nix.flakeModule
      ];

      systems = import inputs.systems;
      perSystem = {
        config,
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
            dotnetInstallFlags = "-p:TargetFramework=${tfm}";
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
                  ./Directory.Build.props
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

          netstandard = mkDotnetLib "netstandard2.1";
          dotnet-8 = mkDotnetLib "net8.0";
          dotnet-9 = mkDotnetLib "net9.0";
          dotnet-10 = mkDotnetLib "net10.0";
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
          inputsFrom = builtins.attrValues self'.packages ++ [config.treefmt.build.devShell];
          packages = with pkgs; [
            roslyn-ls
            lemminx
            vscode-json-languageserver

            basedpyright
            ruff

            prek
            nixd
          ];
          env = {inherit DOTNET_ROOT;};
        };

        treefmt = {
          programs = {
            clang-format = {
              enable = true;
              includes = ["*.cs"];
            };
            statix.enable = true;
            alejandra.enable = true;
            prettier = {
              enable = true;
              excludes = ["packages.lock.json"];
            };
            xmllint = {
              enable = true;
              includes = ["*.csproj" "*.props"];
            };
          };
          settings = {
            # https://github.com/numtide/treefmt-nix/pull/466
            toml = {
              command = "${pkgs.lib.getExe pkgs.tombi}";
              option = ["format"];
              includes = ["*.toml"];
            };
          };
        };
      };
    };
}
