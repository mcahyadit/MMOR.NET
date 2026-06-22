{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";

    nuget-packageslock2nix = {
      url = "github:mdarocha/nuget-packageslock2nix";
      inputs.nixpkgs.follows = "nixpkgs";
    };
  };

  outputs = {self, ...} @ inputs:
    inputs.flake-utils.lib.eachDefaultSystem (
      system: let
        pname = "MMOR.NET";
        version = "2.2.16.0";
        pkgs = import inputs.nixpkgs {inherit system;};
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

            dotnet-sdk = with pkgs.dotnetCorePackages;
              combinePackages [
                sdk_10_0
                sdk_9_0
                sdk_8_0
              ];
            dotnet-runtime = finalAttrs.dotnet-sdk;
            # Needed by checkPhase to find the dotnet in path
            DOTNET_ROOT = "${finalAttrs.dotnet-sdk}/share/dotnet";

            # dotnet-10 removed netstandard2.1 as tfm
            # https://github.com/dotnet/source-build/discussions/5329
            dotnetFlags = ''-p:TargetFrameworks="net8.0;net9.0;net10.0"'';

            nugetDeps = inputs.nuget-packageslock2nix.lib {
              inherit system;
              lockfiles = [
                ./packages.lock.json
              ];
            };

            meta = {
              license = pkgs.lib.licenses.mit;
            };
          });

          # TODO:
          # docs = pkgs.callPackage ./nix/docs.nix {
          #   inherit pkgs pname version;
          #   baselib = self.packages.${system}.dotnet-10;
          # };

          dotnet-8 = self.packages.${system}.default.overrideAttrs (old: {
            packNupkg = false;
            dontPublish = false;
            doCheck = true;
            dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_8_0;
            dotnetFlags = "-p:TargetFramework=net8.0";
          });
          dotnet-9 = self.packages.${system}.default.overrideAttrs (old: {
            packNupkg = false;
            dontPublish = false;
            doCheck = true;
            dotnet-sdk = pkgs.dotnetCorePackages.sdk_9_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_9_0;
            dotnetFlags = "-p:TargetFramework=net9.0";
          });
          dotnet-10 = self.packages.${system}.default.overrideAttrs (old: {
            packNupkg = false;
            dontPublish = false;
            doCheck = true;
            dotnet-sdk = pkgs.dotnetCorePackages.sdk_10_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_10_0;
            dotnetFlags = "-p:TargetFramework=net10.0";
          });
        };

        apps.docs = {
          type = "app";
          program = "${pkgs.writeShellScriptBin "serve" ''
            ${pkgs.python3}/bin/python3 -m http.server 8000 \
              --directory ${self.packages.${system}.docs}
          ''}/bin/serve";
        };

        checks = {
          dotnet-8-tests = self.packages.${system}.dotnet-8;
          dotnet-9-tests = self.packages.${system}.dotnet-9;
        };

        devShells.default = pkgs.mkShellNoCC {
          inputsFrom = builtins.attrValues self.packages.${system};
          packages =
            [self.formatter.${system}]
            ++ (with pkgs; [
              roslyn-ls
              vscode-langservers-extracted
              clang-tools
              xmlformat

              prettierd

              just
              just-lsp

              prek
              nixd
            ]);
        };
        formatter = pkgs.alejandra;
      }
    );
}
