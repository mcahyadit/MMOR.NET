{
  pkgs,
  pname,
  version,
  assemblies,
  ...
}: let
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

  nf-css = pkgs.fetchurl {
    url = "https://raw.githubusercontent.com/ryanoasis/nerd-fonts/refs/heads/master/css/nerd-fonts-generated.min.css";
    sha256 = "sha256-B/CjCZtU+ZLBfrNzUKCAlNffeh7a5S/o5oYG9Vsm0DQ=";
  };

  nf-ttf = fetchTarball {
    url = "https://github.com/ryanoasis/nerd-fonts/releases/download/v3.4.0/NerdFontsSymbolsOnly.tar.xz";
    sha256 = "sha256:09q0in7g136h6fj1pzz020av43pcqyzhr6zhv25zmskxraw88hh3";
  };
in
  pkgs.stdenvNoCC.mkDerivation {
    pname = "${pname}-docs";
    inherit version;

    src = pkgs.lib.fileset.toSource {
      root = ../.;
      fileset = pkgs.lib.fileset.intersection (pkgs.lib.fileset.fromSource (pkgs.lib.sources.cleanSource ../.)) (
        pkgs.lib.fileset.unions [
          ../zensical.toml
          ../docs
        ]
      );
    };

    buildInputs = [
      pkgs.fd
      pkgs.zensical
      pkgs.woff2
      xmldoc2md
    ];

    configurePhase = ''
      mkdir -p ./docs/stylesheets
      cp "${nf-css}" ./docs/stylesheets/nerd-fonts-generated.min.css

      FONTS_DIR="./docs/fonts";
      mkdir -p "$FONTS_DIR"
      cp "${nf-ttf}/SymbolsNerdFont-Regular.ttf" "$FONTS_DIR"

      for font in $(fd --extension ttf --base-directory "$FONTS_DIR"); do
        woff2_compress "$FONTS_DIR/$font"
        rm "$FONTS_DIR/$font"
      done

      sed -i 's|Symbols-2048-em Nerd Font Complete.woff2|SymbolsNerdFont-Regular.woff2|' \
        ./docs/stylesheets/nerd-fonts-generated.min.css
    '';

    buildPhase = ''
      for assembly in ${toString assemblies}; do
        SRC="$assembly/lib"
        for lib in $(fd --extension xml --base-directory "$SRC"); do
          name="''${lib##*/}" # basename
          name="''${name%.*}" # remove extension
          xmldoc2md "$SRC/''${lib//xml/dll}" --output "./docs/api/$name"
          for doc in $(fd --extension md --base-directory "./docs/api/$name"); do
            # Ignore index.md
            full_path="./docs/api/$name/$doc"
            basename="''${doc##*/}"
            [ "$basename" != "index.md" ] || continue

            sed -i 's|`csharp|`cs|' "$full_path"
            sed -i 's|`[[:digit:]]||g' "$full_path"
          done
        done
      done

      zensical build
    '';

    installPhase = ''
      mkdir -p "$out"
      cp -r ./site/** "$out/"
      chmod +x "$out/bin/serve.py"
    '';

    meta = {
      mainProgram = "serve.py";
    };
  }
