_: {
  perSystem = {
    config,
    self',
    pkgs,
    ...
  }: {
    devShells.default = pkgs.mkShellNoCC {
      inputsFrom = builtins.attrValues self'.packages ++ [config.treefmt.build.devShell];
      packages = with pkgs; [
        roslyn-ls
        lemminx
        vscode-json-languageserver
        yaml-language-server

        basedpyright
        ruff

        prek
        nixd
      ];
      env = {
        inherit (self'.packages.default) DOTNET_ROOT;
      };
    };
  };
}
