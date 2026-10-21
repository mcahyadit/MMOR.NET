_: {
  perSystem = _: {
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
        taplo.enable = true;
        actionlint.enable = true;
      };
      settings.excludes = ["third_party/**"];
    };
  };
}
