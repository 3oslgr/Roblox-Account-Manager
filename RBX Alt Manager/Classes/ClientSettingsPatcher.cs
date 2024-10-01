using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.IO;

namespace RBX_Alt_Manager.Classes
{
    public static class ClientSettingsPatcher
    {
        public static void PatchSettings(Account Account = null)
        {
            DirectoryInfo VersionFolder = null;

            object RegistryValue = Registry.ClassesRoot.OpenSubKey(@"roblox\DefaultIcon")?.GetValue("");

            if (RegistryValue != null && RegistryValue is string RobloxPath)
                VersionFolder = Directory.GetParent(RobloxPath);

            if (VersionFolder == null || !VersionFolder.Exists) { Program.Logger.Error("[ClientSettingsPatcher::PatchSettings] Can't patch ClientAppSettings, folder doesn't exist"); return; }
            if (!VersionFolder.Name.StartsWith("version-")) { Program.Logger.Error("[ClientSettingsPatcher::PatchSettings] Can't patch ClientAppSettings, folder doesn't start with 'version-'"); return; }
            if (!File.Exists(Path.Combine(VersionFolder.FullName, "RobloxPlayerLauncher.exe"))) { Program.Logger.Error("[ClientSettingsPatcher::PatchSettings] Can't patch ClientAppSettings, RobloxPlayerBeta.exe not found"); return; }

            DirectoryInfo SettingsFolder = new DirectoryInfo(Path.Combine(VersionFolder.FullName, "ClientSettings"));

            if (!SettingsFolder.Exists) SettingsFolder.Create();

            string CustomFN = AccountManager.General.Exists("CustomClientSettings") ? AccountManager.General.Get<string>("CustomClientSettings") : string.Empty;
            string SettingsFN = Path.Combine(SettingsFolder.FullName, "ClientAppSettings.json");

            if (!string.IsNullOrEmpty(CustomFN) && File.Exists(CustomFN))
                File.Copy(CustomFN, SettingsFN);
            else if (AccountManager.General.Get<bool>("UnlockFPS"))
            {
                if (File.Exists(SettingsFN) && File.ReadAllText(SettingsFN).TryParseJson(out JObject Settings))
                {
                    int MaxFPS = AccountManager.General.Exists("MaxFPSValue") ? AccountManager.General.Get<int>("MaxFPSValue") : 120;

                    if (Account != null && Account.Fields.TryGetValue("MaxFPS", out string MaxFPSField))
                        int.TryParse(MaxFPSField, out MaxFPS);

                    Settings["DFIntTaskSchedulerTargetFps"] = MaxFPS;
                    File.WriteAllText(SettingsFN, Settings.ToString(Newtonsoft.Json.Formatting.None));
                }
                else
                    File.WriteAllText(SettingsFN, "{\"DFIntTaskSchedulerTargetFps\":240}");
            }
        }
    }
}