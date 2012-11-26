[Setup]
#define use_dotnetfx40
#define use_vc2010
#define AppName "Better Explorer"
AppName={#AppName}
AppVersion=2.0.2
AppID={{83385EFA-85BD-4D64-B3ED-745AFA19E984}
DefaultDirName={pf}\{#AppName}
UninstallDisplayName=Better Explorer
ArchitecturesInstallIn64BitMode=x64
MinVersion=0,6.1.7600
AppVerName=2.0.2
DefaultGroupName={#AppName}
ShowLanguageDialog=auto
LanguageDetectionMethod=locale
#include "scripts\products.iss"
#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"
#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40full.iss"
#endif
#ifdef use_vc2010
#include "scripts\products\vcredist2010.iss"
#endif

[Files]
Source: "C:\Projects\BetterExplorer\Installer\bin\iswin7.dll"; DestDir: "{tmp}"; Flags: dontcopy

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"
Name: "bulgarian"; MessagesFile: "compiler:Languages\Bulgarian.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "greek"; MessagesFile: "compiler:Languages\Greek.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Code]
procedure iswin7_add_glass(Handle:HWND; Left, Top, Right, Bottom : Integer; GDIPLoadMode: boolean);
external 'iswin7_add_glass@files:iswin7.dll stdcall';

procedure iswin7_add_button(Handle:HWND);
external 'iswin7_add_button@files:iswin7.dll stdcall';

procedure iswin7_free;
external 'iswin7_free@files:iswin7.dll stdcall';

procedure InitializeWizard();
begin
  iswin7_add_button(WizardForm.BackButton.Handle);
  iswin7_add_button(WizardForm.NextButton.Handle);
  iswin7_add_button(WizardForm.CancelButton.Handle);
  iswin7_add_glass(WizardForm.Handle, 0, 0, 0, 47, True);
end;

function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();


	// if no .netfx 4.0 is found, install the client (smallest)
#ifdef use_dotnetfx40
	if (not netfxinstalled(NetFx40Client, '') and not netfxinstalled(NetFx40Full, '')) then
		dotnetfx40full();
#endif

#ifdef use_vc2010
	vcredist2010();
#endif
	Result := true;
end;

procedure DeinitializeSetup();
begin
  iswin7_free;
end;
