@echo off
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo csc.exe not found
  exit /b 1
)
"%CSC%" /nologo /target:winexe /codepage:65001 /out:Keytrace.exe /win32icon:icon.ico ^
  /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
  src\AssemblyInfo.cs src\Theme.cs src\Store.cs src\Hook.cs src\Groups.cs src\MainForm.cs ^
  src\MainForm.Paint.cs src\CustomRangeDialog.cs src\GroupNameDialog.cs src\Program.cs
if errorlevel 1 exit /b 1
echo Built Keytrace.exe
