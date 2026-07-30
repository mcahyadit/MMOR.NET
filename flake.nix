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

  outputs = inputs:
    inputs.flake-parts.lib.mkFlake {inherit inputs;} {
      imports = [
        ./nix/dev-shell.nix
        ./nix/mmor-net.nix
        ./nix/sourcegen.nix
        inputs.treefmt-nix.flakeModule
        ./nix/treefmt.nix
      ];

      systems = import inputs.systems;
      perSystem = {
        self',
        pkgs,
        system,
        ...
      }: let
        mkDotnetLib = tfm:
          self'.packages.default.overrideAttrs (old: {
            packNupkg = false;
            dontPublish = false;
            dotnetInstallFlags = "-p:TargetFramework=${tfm}";
          });
      in {
        _module.args = {
          pkgs = import inputs.nixpkgs {
            inherit system;
            overlays = [
              (self: super: {
                dotnet-sdk = with super.dotnetCorePackages;
                  combinePackages [
                    sdk_10_0
                    sdk_9_0
                    sdk_8_0
                  ];
              })
            ];
          };
        };
        packages = {
          default = self'.packages.mmor-net;

          docs = pkgs.callPackage ./nix/docs.nix {
            inherit pkgs;
            inherit (self'.packages.default) pname version;
            assemblies = [self'.packages.dotnet-8];
          };

          netstandard = mkDotnetLib "netstandard2.1";
          dotnet-8 = mkDotnetLib "net8.0";
          dotnet-9 = mkDotnetLib "net9.0";
          dotnet-10 = mkDotnetLib "net10.0";
        };

        checks = {
          default = self'.packages.default;
        };
      };
    };
}
