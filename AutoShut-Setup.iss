; Script Inno Setup pour AutoShut
; Téléchargez Inno Setup depuis : https://jrsoftware.org/isdl.php

#define MyAppName "AutoShut"
#define MyAppVersion "1.0"
#define MyAppPublisher "Your Company Name"
#define MyAppURL "https://www.yourwebsite.com/"
#define MyAppExeName "AutoShut.exe"
#define MyAppDescription "Blender Render Manager"

[Setup]
; Informations de base
AppId={{A1B2C3D4-E5F6-7890-ABCD-1234567890AB}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=
InfoAfterFile=
OutputDir=Build\Installer
OutputBaseFilename=AutoShut-Setup-v{#MyAppVersion}
SetupIconFile=AutoShut\Resources\AppIcon\appicon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Privilèges (admin requis pour l'installation système)
PrivilegesRequired=admin

; Désinstallation
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Tous les fichiers du dossier Build\Windows
Source: "Build\Windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: N'utilisez pas "Flags: ignoreversion" sur des fichiers système partagés

[Icons]
; Icône dans le menu Démarrer
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "{#MyAppDescription}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
; Icône sur le Bureau (si sélectionné)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "{#MyAppDescription}"
; Icône dans la barre de lancement rapide (si sélectionné)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
; Proposer de lancer l'application après l'installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Vérifier si .NET 10 est installé
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Vérifier si .NET est installé
  if not IsDotNetInstalled() then
  begin
    if MsgBox('.NET 10 Runtime n''est pas installé sur votre système.' + #13#10 + 
              'L''application nécessite .NET 10 pour fonctionner.' + #13#10#13#10 +
              'Voulez-vous continuer l''installation ? Vous devrez installer .NET 10 manuellement.' + #13#10 +
              'Téléchargement : https://dotnet.microsoft.com/download', 
              mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;
