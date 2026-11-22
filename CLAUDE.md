# BASys .NET port

Port of the Java application in `java/basys2003` (KNX configuration tool) to .NET 10, Blazor Hybrid and WPF.

## Layout (`src/BASys.sln`)

| Project | Namespace | Purpose |
|---|---|---|
| BASys.DataModels | `BASys.DataModels` | XML based models ported from Java (`ArchitecturalDataModel`, `InstallationModel`, `EibDevicesDataModel`, `Project`), enums, address helpers |
| BASys.DataAccess | `BASys.DataAccess` | File access: project/device storage, local data directory (`LocalConfigurator`), product data reader, VD converter |
| BASys.Tests | `BASys.Tests` | xUnit tests (storage round trips, Java sample files) |
| BASys.Resources | `BASys.Resources` | `Strings.resx` (de, neutral) and `Strings.en.resx`, converted from the Java `Locale_*.properties` |
| BASys.Services | `BASys.Services` | Service interfaces and implementations (settings, projects, EIB data), localization; bus programming is a mock (`BASys.Services.Mock`) |
| BASys.ViewModels | `BASys.ViewModels` | CommunityToolkit.Mvvm view models and dialog view models |
| BASys.Views | `BASys.Views` | Razor class library with the whole UI (menu, toolbar, tree, structural view, dialogs) |
| BASys.Apps.Wpf | `BASys.Apps.Wpf` | WPF host with a `BlazorWebView`, nothing but hosting |

## Rules

Coding rules are in `DesignConstraints/Codierrichtlinien.md` (no `var`, braces always, single `return result;`,
private fields with `_` prefix above their property, block namespaces, English comments).
View models use manual properties with `SetProperty` to follow these rules.

## Notes

* WPF target is `net10.0-windows10.0.19041.0` (WebView2 needs the Windows SDK projection).
* Build: `dotnet build src/BASys.sln`
* Files use the format of the Java application: `<data>/projects/<name>/structure.xml` + `installation.xml`, `<data>/global-data/eib/eib-devices.xml`, product data `<data>/global-data/eib/devicedata/*.xml`, config `<data>/basys.properties` (ISO-8859-1). `<data>` is `%USERPROFILE%asys`, overridable with the environment variable `BASYS_DATA_DIR`.
* Bus monitor and telegram dialog use the real KNXnet/IP tunneling connection (`EibBusService`, library `src/knx.net` = submodule of the oalt fork, branch `netcore`, extended by `CemiReceived`/`SendCemi`; gateway is chosen via menu "EIB Diagnose > EIB Verbindung öffnen", address stored in basys.properties `bus.host`; gateway type Darmstadt = simulated bus). Device programming (`IEibProgrammingService`) is still a mockup (`BASys.Services/Mock`). The bus monitor opens in its own WPF window (`IToolWindowService`, `ToolWindow`, `ToolWindowRoot`), the telegram dialog is a modal dialog of the main window. Not ported yet: sensor creation (AddSensorCommand), undo/redo, KNXnet/IP.
* Tests: `dotnet test src/BASys.Tests`.
* UI styling uses a local copy of Bootstrap 5.3.3 (`BASys.Views/wwwroot/lib/bootstrap`) plus `css/app.css`.
  Dialogs are deliberately compact (small tables/controls); do not use Bootstrap's `.modal-backdrop`/`.modal` classes, only `.modal-content/.modal-header/.modal-body` inside `.dialog`.
* Palette drag & drop uses pointer events (`BASys.Views/wwwroot/js/dragdrop.js`, `DragDropState`, `DropZone`), not HTML5 drag & drop: WebView2 hosted in WPF did not deliver drag events to the page.
* Icons come from the local Bootstrap Icons sprite (`wwwroot/lib/bootstrap-icons`), used through `<Icon Name="..." />`; view models expose icon names, not files.
