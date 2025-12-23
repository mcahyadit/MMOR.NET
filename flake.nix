{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs =
    {
      nixpkgs,
      flake-utils,
      ...
    }:
    flake-utils.lib.eachDefaultSystem (
      system:
      let
        pkgs = import nixpkgs { inherit system; };
      in
      {
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnet-sdk_9
            roslyn-ls
            clang-tools

            docfx
            vscode-json-languageserver
            prettierd

            just

            pre-commit
            nixd # LSP for Nix
            nixfmt
          ];
        };
      }
    );
}
