Huloop Custom Installer Solution (WinForms, .NET 8)
=================================================

What you got:
- Installer.UI : WinForms application providing the wizard screens (Welcome, EULA, Component selection, Download progress, Configuration, Finish)
- Installer.Core: Core logic for manifest reading, download & extract, INI editing, service install
- Installer.Models: Component models used by Core/UI
- Installer.Bootstrap: Small helper console app to publish the UI and optionally call WiX for MSI packaging

How flow works:
1. Run Installer.UI (in Debug via Visual Studio or publish self-contained via dotnet publish)
2. The UI fetches UpdateManifest.json from https://qa.huloop.ai/latest/ and matches selected components by id.
3. Download and extract components into the APPDIR
4. Configuration screen writes settings.ini and installs the service (uses sc.exe)
5. Use Installer.Bootstrap to publish and optionally package with WiX (see notes below).

Important notes:
- The sample uses a manifest-based API (Get UpdateManifest.json) as requested.
- For production, ensure UpdateManifest.json includes accurate SHA256 & sizes.
- Service installation uses sc.exe and requires Administrator privileges.
- MSI packaging is optional; the solution includes a bootstrap helper that will call WiX CLI if installed.
- Test on clean VMs to validate admin prompts, service behaviors, and network downloads.

Build & Run:
- Open the solution in Visual Studio 2022+ (with .NET 8 SDK)
- Set Installer.UI as startup project and run
- Or use the bootstrap helper:
    dotnet run -p Installer.Bootstrap/Installer.Bootstrap.csproj

To create MSI:
- Install WiX toolset 4 and the dotnet wix global tool:
    dotnet tool install --global wix --version 4.*
- Place your WiX .wxs files in a 'wix' folder at the solution root
- Run Installer.Bootstrap which will attempt to call 'wix build'

Security & Signing:
- Sign the EXE and MSI using your code signing cert in CI pipeline.

