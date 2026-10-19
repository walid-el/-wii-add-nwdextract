NwdExtract (Navisworks Add-in)
==============================

What this is
- An in-process Navisworks add-in that exports element rows from the open document to CSV.

Build
- Open the project in Visual Studio on Windows.
- Ensure the Navisworks installation path is correct in `NwdExtract.csproj`.
- Optional override: `dotnet build -c Release /p:NavisDir="C:\Program Files\Autodesk\Navisworks Manage 2025"`

Install
1. Build the project.
2. Copy `NwdExtract.dll` and `NwdExtract.addin` to a Navisworks AddIns folder:
   - `%APPDATA%\Autodesk\Navisworks Manage 2025\Plugins\`
   - or `%PROGRAMDATA%\Autodesk\Navisworks Manage 2025\Plugins\`
3. Start Navisworks and run the plugin from the Add-Ins menu.

Notes
- The add-in targets .NET Framework 4.8 and x64.
- It uses the Navisworks API in-process, so it runs inside Navisworks rather than as a standalone console app.
- If your exporter/property names differ from the defaults in `NwdExtractPlugin.cs`, update the candidate arrays to match your model.
