; Inno Setup скрипт за „Грешна подредба“ (LayoutFixer)
; Версията идва от GitHub Actions (/DMyAppVersion=1.0.0).
; Ръчно: ISCC.exe /DMyAppVersion=1.0.0 installer\LayoutFixer.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "Грешна подредба"
#define MyAppPublisher "Радослав Чакъров"
#define MyAppURL "https://github.com/waitingboy-collab/layout-fixer"
#define MyAppExeName "LayoutFixer.exe"

[Setup]
; Уникален за тази програма. НЕ го сменяй след първия release.
AppId={{D72AE39D-C681-458F-85E5-0BCF7C3F3855}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\LayoutFixer
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\output
OutputBaseFilename=LayoutFixer-Setup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; Същото име като Program.MutexName — инсталаторът затваря работещата програма при ъпдейт
AppMutex=LayoutFixerSingleInstance
CloseApplications=yes

[Languages]
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "autostart"; Description: "Стартирай заедно с Windows"; GroupDescription: "Допълнително:"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Същият ключ ползва и менюто „Стартирай с Windows“ в програмата
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "LayoutFixer"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C taskkill /IM {#MyAppExeName} /F"; Flags: runhidden; RunOnceId: "StopLayoutFixer"

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\LayoutFixer"
