_: {
  perSystem = {pkgs, ...}: {
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
}
