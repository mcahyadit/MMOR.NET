{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = {self, ...} @ inputs:
    inputs.flake-utils.lib.eachDefaultSystem (
      system: let
        pkgs = import inputs.nixpkgs {inherit system;};
      in {
        devShells.default = pkgs.mkShellNoCC {
          packages =
            [self.formatter.${system}]
            ++ (with pkgs; [
              dotnetCorePackages.sdk_9_0
              roslyn-ls
              clang-tools

              docfx
              vscode-json-languageserver
              prettierd

              just
              just-lsp

              prek
              nixd
              alejandra
            ]);
        };
        formatter = pkgs.alejandra;
      }
    );
}
