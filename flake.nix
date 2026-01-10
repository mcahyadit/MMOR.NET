{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = {
    self,
    nixpkgs,
    flake-utils,
    ...
  }:
    flake-utils.lib.eachDefaultSystem (
      system: let
        pkgs = import nixpkgs {inherit system;};
      in {
        devShellFragments.default = {
          packages = with pkgs; [
            dotnet-sdk_9
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
          ];
        };
        devShells.default = pkgs.mkShellNoCC self.devShellFragments.${system}.default;
      }
    );
}
