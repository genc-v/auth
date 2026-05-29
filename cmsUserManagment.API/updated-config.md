# Updated configuration.nix (Fixes `hardware.opengl` errors for newer NixOS)

It looks like you are on a very new version of NixOS (24.11 / unstable), where `hardware.opengl` has been renamed to `hardware.graphics`, and `driSupport` was made obsolete since it's enabled by default. 

Copy the following content into your `/etc/nixos/configuration.nix`.

```nix
# Edit this configuration file to define what should be installed on
# your system.

{ config, pkgs, ... }:

{
  imports = [
    ./hardware-configuration.nix
  ];

  ######################################
  # Boot
  ######################################

  boot.loader.systemd-boot.enable = true;
  boot.loader.efi.canTouchEfiVariables = true;

  ######################################{ config, pkgs, ... }:

{
  imports = [
    ./hardware-configuration.nix
  ];

  ######################################
  # Boot
  ######################################

  boot.loader.systemd-boot.enable = true;
  boot.loader.efi.canTouchEfiVariables = true;

  ######################################
  # Networking
  ######################################

  networking.hostName = "nixos";
  networking.networkmanager.enable = true;

  ######################################
  # Time / Locale
  ######################################

  time.timeZone = "Europe/Skopje";

  i18n.defaultLocale = "en_US.UTF-8";

  i18n.extraLocaleSettings = {
    LC_ADDRESS = "mk_MK.UTF-8";
    LC_IDENTIFICATION = "mk_MK.UTF-8";
    LC_MEASUREMENT = "mk_MK.UTF-8";
    LC_MONETARY = "mk_MK.UTF-8";
    LC_NAME = "mk_MK.UTF-8";
    LC_NUMERIC = "mk_MK.UTF-8";
    LC_PAPER = "mk_MK.UTF-8";
    LC_TELEPHONE = "mk_MK.UTF-8";
    LC_TIME = "mk_MK.UTF-8";
  };

  ######################################
  # Desktop & Graphics
  ######################################

  services.xserver.enable = true;

  services.displayManager.sddm.enable = true;
  services.desktopManager.plasma6.enable = true;

  services.xserver.xkb = {
    layout = "us";
    variant = "";
  };

  # Load NVIDIA driver for Xorg and Wayland
  services.xserver.videoDrivers = ["nvidia"];

  # Enable OpenGL
  hardware.opengl = {
    enable = true;
    driSupport = true;
    driSupport32Bit = true;
  };

  hardware.nvidia = {
    # Modesetting is required.
    modesetting.enable = true;

    # Nvidia power management.
    powerManagement.enable = false;
    powerManagement.finegrained = false;

    # Use the NVidia open source kernel module
    open = false;

    # Enable the Nvidia settings menu
    nvidiaSettings = true;

    # Select the appropriate driver version
    package = config.boot.kernelPackages.nvidiaPackages.stable;
  };

  ######################################
  # Hardware & Bluetooth
  ######################################

  # Enable Bluetooth
  hardware.bluetooth.enable = true;
  hardware.bluetooth.powerOnBoot = true;

  ######################################
  # Audio
  ######################################

  services.pulseaudio.enable = false;

  security.rtkit.enable = true;

  services.pipewire = {
    enable = true;
    alsa.enable = true;
    alsa.support32Bit = true;
    pulse.enable = true;
  };

  ######################################
  # Printing
  ######################################

  services.printing.enable = true;

  ######################################
  # User
  ######################################

  users.users.roses = {
    isNormalUser = true;
    description = "roses";

    extraGroups = [
      "networkmanager"
      "wheel"
      "docker"
    ];

    packages = with pkgs; [
      kdePackages.kate
    ];
  };

  ######################################
  # Allow unfree
  ######################################

  nixpkgs.config.allowUnfree = true;

  ######################################
  # Docker
  ######################################

  virtualisation.docker.enable = true;

  ######################################
  # Tailscale
  ######################################

  services.tailscale.enable = true;

  ######################################
  # Programs
  ######################################

  programs.firefox.enable = true;

  programs.git.enable = true;

  ######################################
  # Shell / Dev Tools
  ######################################

  environment.systemPackages = with pkgs; [

    ##################################
    # Browsers
    ##################################

    google-chrome

    ##################################
    # Editors / IDEs
    ##################################

    vscode
    neovim
    obsidian

    jetbrains.datagrip
    jetbrains.rider

    ##################################
    # Git tools
    ##################################

    git
    gh
    lazygit

    ##################################
    # Docker tools
    ##################################

    docker
    docker-compose
    lazydocker

    ##################################
    # JS / TS
    ##################################

    nodejs_22
    nodejs_24

    ##################################
    # .NET / C#
    ##################################

    dotnet-sdk_8
    dotnet-sdk_9

    ##################################
    # CLI tools
    ##################################

    wget
    curl
    unzip
    zip
    ripgrep
    fd
    fzf
    jq
    tree
    htop
    btop
    opencode
    codex


    ##################################
    # Music
    ##################################

    feishin

    ##################################
    # AI / Coding
    ##################################

    claude-code

    ##################################
    # Optional nice tools
    ##################################

    tmux
    zoxide
    starship

    ##################################
    # Gaming
    ##################################
    steam
    lutris
    heroic
    protonup-qt

  ];

  ######################################
  # Nix Features
  ######################################

  nix.settings.experimental-features = [
    "nix-command"
    "flakes"
  ];

  ######################################
  # State Version
  ######################################

  system.stateVersion = "25.11";
}

  # Networking
  ######################################

  networking.hostName = "nixos";
  networking.networkmanager.enable = true;

  ######################################
  # Time / Locale
  ######################################

  time.timeZone = "Europe/Skopje";

  i18n.defaultLocale = "en_US.UTF-8";

  i18n.extraLocaleSettings = {
    LC_ADDRESS = "mk_MK.UTF-8";
    LC_IDENTIFICATION = "mk_MK.UTF-8";
    LC_MEASUREMENT = "mk_MK.UTF-8";
    LC_MONETARY = "mk_MK.UTF-8";
    LC_NAME = "mk_MK.UTF-8";
    LC_NUMERIC = "mk_MK.UTF-8";
    LC_PAPER = "mk_MK.UTF-8";
    LC_TELEPHONE = "mk_MK.UTF-8";
    LC_TIME = "mk_MK.UTF-8";
  };

  ######################################
  # Desktop & Graphics
  ######################################

  services.xserver.enable = true;

  services.displayManager.sddm.enable = true;
  services.desktopManager.plasma6.enable = true;

  services.xserver.xkb = {
    layout = "us";
    variant = "";
  };

  # Load NVIDIA driver for Xorg and Wayland
  services.xserver.videoDrivers = ["nvidia"];

  # Enable Graphics (Renamed from hardware.opengl)
  hardware.graphics = {
    enable = true;
    enable32Bit = true;
  };

  hardware.nvidia = {
    # Modesetting is required.
    modesetting.enable = true;

    # Nvidia power management.
    powerManagement.enable = false;
    powerManagement.finegrained = false;

    # Use the NVidia open source kernel module
    open = false;

    # Enable the Nvidia settings menu
    nvidiaSettings = true;

    # Select the appropriate driver version
    package = config.boot.kernelPackages.nvidiaPackages.stable;
  };

  ######################################
  # Hardware & Bluetooth
  ######################################

  # Enable Bluetooth
  hardware.bluetooth.enable = true;
  hardware.bluetooth.powerOnBoot = true;

  ######################################
  # Audio
  ######################################

  services.pulseaudio.enable = false;

  security.rtkit.enable = true;

  services.pipewire = {
    enable = true;
    alsa.enable = true;
    alsa.support32Bit = true;
    pulse.enable = true;
  };

  ######################################
  # Printing
  ######################################

  services.printing.enable = true;

  ######################################
  # User
  ######################################

  users.users.roses = {
    isNormalUser = true;
    description = "roses";

    extraGroups = [
      "networkmanager"
      "wheel"
      "docker"
    ];

    packages = with pkgs; [
      kdePackages.kate
    ];
  };

  ######################################
  # Allow unfree
  ######################################

  nixpkgs.config.allowUnfree = true;

  ######################################
  # Docker
  ######################################

  virtualisation.docker.enable = true;

  ######################################
  # Tailscale
  ######################################

  services.tailscale.enable = true;

  ######################################
  # Programs
  ######################################

  programs.firefox.enable = true;

  programs.git.enable = true;

  ######################################
  # Shell / Dev Tools
  ######################################

  environment.systemPackages = with pkgs; [

    ##################################
    # Browsers
    ##################################

    google-chrome

    ##################################
    # Editors / IDEs
    ##################################

    vscode
    neovim
    obsidian

    jetbrains.datagrip
    jetbrains.rider

    ##################################
    # Git tools
    ##################################

    git
    gh
    lazygit

    ##################################
    # Docker tools
    ##################################

    docker
    docker-compose
    lazydocker

    ##################################
    # JS / TS
    ##################################

    nodejs_22
    nodejs_24

    ##################################
    # .NET / C#
    ##################################

    dotnet-sdk_8
    dotnet-sdk_9

    ##################################
    # CLI tools
    ##################################

    wget
    curl
    unzip
    zip
    ripgrep
    fd
    fzf
    jq
    tree
    htop
    btop
    opencode
    codex


    ##################################
    # Music
    ##################################

    feishin

    ##################################
    # AI / Coding
    ##################################

    claude-code

    ##################################
    # Optional nice tools
    ##################################

    tmux
    zoxide
    starship

    ##################################
    # Gaming
    ##################################
    steam
    lutris
    heroic
    protonup-qt

  ];

  ######################################
  # Nix Features
  ######################################

  nix.settings.experimental-features = [
    "nix-command"
    "flakes"
  ];

  ######################################
  # State Version
  ######################################

  system.stateVersion = "25.11";
}
```
