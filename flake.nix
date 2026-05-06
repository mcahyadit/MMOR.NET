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
        pkgs = import inputs.nixpkgs {inherit system;};
      in {
        packages = {
          default = pkgs.buildDotnetModule (finalAttrs: {
            inherit pname;
            version = "2.2.14.1";
            src = pkgs.lib.cleanSource ./.;
            packNupkg = true;

            # Still haven't figured out how to only install nupkg without lib
            # ..for now just install the most portable one
            dotnetInstallFlags = "--framework=netstandard2.1";

            dotnet-sdk = with pkgs.dotnetCorePackages;
              combinePackages [
                sdk_9_0
                sdk_8_0
              ];
            dotnet-runtime = with pkgs.dotnetCorePackages;
              combinePackages [
                runtime_9_0
                runtime_8_0
              ];
            DOTNET_ROOT = "${finalAttrs.dotnet-sdk}/share/dotnet";

            nugetDeps = inputs.nuget-packageslock2nix.lib {
              inherit system;
              name = pname;
              lockfiles = [
                ./packages.lock.json
              ];
            };

            meta = {
              license = pkgs.lib.licenses.mit;
            };
          });

          docs = let
            xmldoc2md = pkgs.buildDotnetGlobalTool {
              pname = "XMLDoc2Markdown";
              version = "5.0.0";

              nugetHash = "sha256-RVVgaLgQ5Z8olPBrYvrXsjb1WSTe/EuwxRxJPhh5Le4=";

              executables = ["xmldoc2md"];

              meta = {
                description = "Tool to generate markdown from C# XML documentation.";
                homepage = "https://charlesdevandiere.github.io/xmldoc2md";
                license = pkgs.lib.licenses.mit;
                mainProgram = "xmldoc2md";
              };
            };
          in
            pkgs.stdenvNoCC.mkDerivation {
              name = "${pname}-docs";
              packNupkg = false;

              src = pkgs.lib.cleanSource ./.;

              buildInputs = [
                pkgs.zensical
                xmldoc2md
              ];

              buildPhase = ''
                xmldoc2md "${
                  self.packages.${system}.default.overrideAttrs (old: {
                    dotnetFlags = old.dotnetFlags ++ [" -p:GenerateDocumentationFile=true"];
                  })
                }/lib/${pname}/${pname}.dll" --output ./docs/api
                zensical build
                # docfx ./docs/docfx.json --output ./out/docs
              '';

              installPhase = ''
                mkdir -p "$out"
                cp -r ./site/** "$out/"
              '';
            };

          dotnet-8 = self.packages.${system}.default.overrideAttrs (old: {
            packNupkg = false;
            doCheck = true;
            dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_8_0;
            dotnetFlags = "-p:TargetFramework=net8.0";
            dotnetInstallFlags = "";
          });
          dotnet-9 = self.packages.${system}.default.overrideAttrs (old: {
            packNupkg = false;
            doCheck = true;
            dotnet-sdk = pkgs.dotnetCorePackages.sdk_9_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_9_0;
            dotnetFlags = "-p:TargetFramework=net9.0";
            dotnetInstallFlags = "";
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
