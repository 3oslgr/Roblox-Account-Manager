using BrightIdeasSoftware;
using Microsoft.Win32;
using RBX_Alt_Manager.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RBX_Alt_Manager.Forms
{
    public partial class UWPInstanceManager : Form
    {
        private bool DevModeEnabled = false;
        private string InstallationPath = Path.Combine(AppContext.BaseDirectory, "UWP_Instances");
        private string OriginalPath;
        private CancellationTokenSource RefreshToken;
        public static string Status;

        public static Dictionary<string, UserInstance> UserInstances = new Dictionary<string, UserInstance>();

        public static UWPInstanceManager Instance
        {
            get
            {
                m_Instance ??= new UWPInstanceManager();

                return m_Instance;
            }
        }

        public static bool Exists { get => m_Instance != null; }

        private static UWPInstanceManager m_Instance;

        public UWPInstanceManager()
        {
            InitializeComponent();

            Utilities.Rescale(this);
            ApplyTheme();

            if (!Directory.Exists(InstallationPath)) Directory.CreateDirectory(InstallationPath);

            RegistryKey LocalRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            if (LocalRegistry?.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock")?.GetValue("AllowDevelopmentWithoutDevLicense") is int dInt)
                DevModeEnabled = Convert.ToBoolean(dInt);

            if (!DevModeEnabled && Utilities.YesNoPrompt("Developer Mode Required", "You must enable developer mode to use the UWP Instance Manager", "Would you like change your settings now? (Enable 'Install apps from any source')", false))
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c start ms-settings:developers",
                    CreateNoWindow = true,
                });

            Task.Run(() =>
            {
                SetStatus("Finding UWP Installation");

                RunPowerShellCommand("Get-AppxPackage *Roblox* | Format-List -Property InstallLocation", Line =>
                {
                    Program.Logger.Info($"{Line} | IsRoblox: {Line.Contains(@"WindowsApps\ROBLOXCORPORATION.ROBLOX_")}");

                    if (Line.Contains(@"WindowsApps\ROBLOXCORPORATION.ROBLOX_"))
                        OriginalPath = Line.Substring(Line.IndexOf(":\\") - 1);
                });

                this.InvokeIfRequired(() =>
                {
                CheckPath:

                    if (string.IsNullOrEmpty(OriginalPath))
                        if (Utilities.YesNoPrompt("UWP Multi Instance Manager", "Couldn't find Roblox UWP installation, make sure you have the UWP version installed", "Would you like to manually locate UWP Roblox?") && UwpExeDialog.ShowDialog() == DialogResult.OK && UwpExeDialog.FileName.EndsWith("Windows10Universal.exe"))
                        {
                            OriginalPath = Path.GetDirectoryName(UwpExeDialog.FileName);
                            goto CheckPath;
                        }
                        else
                        {
                            m_Instance = null;
                            Dispose();
                        }
                });

                RefreshInstanceIDs();
            }).ContinueWith(t =>
            {
                if (t.Exception != null)
                    throw t.Exception;
            });
        }

        #region Events

        private void UWPInstanceManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            foreach (string Username in Utilities.OpenAccountSelection())
                Task.Run(() => RegisterAccountProcess(AccountManager.AccountsList.FirstOrDefault(x => x.Username == Username)));
        }

        private void InstanceListBox_DragOver(object sender, DragEventArgs e)
        {
            if (sender != null && sender is ObjectListView)
                e.Effect = DragDropEffects.Copy;
        }

        private void InstanceListBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.CommaSeparatedValue))
            {
                string Data = (string)e.Data.GetData(DataFormats.CommaSeparatedValue);

                foreach (string Line in Data.Split('\n'))
                {
                    string Username = string.Empty;

                    Match match = Regex.Match(Line, @"""?(\w+)""?");

                    if (match.Success)
                        Username = match.Groups[1].Value;

                    if (!string.IsNullOrEmpty(Username))
                        Task.Run(() => RegisterAccountProcess(AccountManager.AccountsList.FirstOrDefault(x => x.Username == Username)));
                }
            }
        }

        private void UninstallSelectedButton_ButtonClick(object sender, EventArgs e)
        {
            if (InstanceListBox.SelectedObjects.Count == 0) return;

            if (!Utilities.YesNoPrompt("Uninstall Selected Instances", $"Are you sure you want to uninstall {InstanceListBox.SelectedObjects.Count} UWP instance(s)?", "", false)) return;

            foreach (UserInstance Instance in InstanceListBox.SelectedObjects)
            {
                SetStatus("Uninstalling Instance(s)");

                Task.Run(() =>
                {
                    RunPowerShellCommand($"Get-AppxPackage ROBLOXCORPORATION.ROBLOX.{Instance.Username} | Remove-AppxPackage");
                    RefreshInstanceIDs();
                });
            }
        }

        private void UninstallAllButton_Click(object sender, EventArgs e)
        {
            if (Utilities.YesNoPrompt("Uninstall All Instances", "Are you sure you want to uninstall every UWP instance?", "", false))
            {
                SetStatus("Uninstalling Instances(s)");

                Task.Run(() =>
                {
                    RunPowerShellCommand("Get-AppxPackage ROBLOXCORPORATION.ROBLOX.* | Remove-AppxPackage");
                    RefreshInstanceIDs();
                });                
            }
        }

        private void AttemptLoginButton_Click(object sender, EventArgs e)
        {
        }

        private void RefreshInstancesButton_Click(object sender, EventArgs e) => RefreshInstanceIDs();

        private void PoweredByLabel_MouseEnter(object sender, EventArgs e) => PoweredByLabel.Text = "https://github.com/Babyhamsta/UWP_MultiPlatform/tree/main";

        private void PoweredByLabel_MouseLeave(object sender, EventArgs e) => PoweredByLabel.Text = "Inspired by Babyhamsta";

        private void PoweredByLabel_Click(object sender, EventArgs e) => Process.Start("https://github.com/Babyhamsta/UWP_MultiPlatform/tree/main");

        #endregion

        #region Methods

        public void ShowWindow()
        {
            WindowState = FormWindowState.Normal;
            Show();
            BringToFront();
        }

        private void SetStatus(string Status) => ToolStrip.InvokeIfRequired(() => StatusLabel.Text = Status);

        private void RegisterAccountProcess(Account account)
        {
            if (account is null) return;

            SetStatus("Creating Account Instance(s)");

            this.InvokeIfRequired(() =>
            {
                foreach (Control Control in Controls)
                    if (Control is ObjectListView)
                        Control.AllowDrop = false;
                    else
                        Control.Enabled = false;
            });

            if (!Directory.Exists(OriginalPath))
                RunPowerShellCommand("Get-AppxPackage *Roblox* | Format-List -Property InstallLocation", Line =>
                {
                    if (Line.Contains(@"WindowsApps\ROBLOXCORPORATION.ROBLOX_"))
                        OriginalPath = Line.Substring(Line.IndexOf(":\\") - 1);
                });

            string Folder = Path.Combine(InstallationPath, account.Username);
            string ManifestPath = Path.Combine(Folder, "AppxManifest.xml");
            string SignaturePath = Path.Combine(Folder, "AppxSignature.p7x");
            string ExecutablePath = Path.Combine(Folder, "Windows10Universal.exe");

            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            if (!File.Exists(ManifestPath) || !File.Exists(ExecutablePath) || (File.Exists(ExecutablePath) && Utilities.FileSHA256(ExecutablePath) != Utilities.FileSHA256(Path.Combine(OriginalPath, "Windows10Universal.exe"))))
            {
                foreach (string dirPath in Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(OriginalPath, Folder));

                foreach (string newPath in Directory.GetFiles(OriginalPath, "*.*", SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(OriginalPath, Folder), true);
            }

            if (File.Exists(SignaturePath)) File.Delete(SignaturePath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(ManifestPath);

            if (AccountManager.General.Get<bool>("UWPMultiInstance"))
            {
                var attribute = xmlDoc.CreateAttribute("desktop4", "SupportsMultipleInstances", "http://schemas.microsoft.com/appx/manifest/desktop/windows10/4");
                attribute.InnerText = "true";
                xmlDoc["Package"]["Applications"]["Application"].SetAttributeNode(attribute);
            }

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("uap", "http://schemas.microsoft.com/appx/manifest/uap/windows10");
            namespaceManager.AddNamespace("manifest", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");

            XmlNode IdentityNode = xmlDoc.SelectSingleNode("/manifest:Package/manifest:Identity", namespaceManager);
            XmlNode VisualElementsNode = xmlDoc.SelectSingleNode("//uap:VisualElements", namespaceManager);
            XmlNode DefaultTitleNode = xmlDoc.SelectSingleNode("//uap:DefaultTile", namespaceManager);

            if (IdentityNode != null && VisualElementsNode != null && DefaultTitleNode != null)
            {
                string newTitle = $"Roblox {account.Username}";
                IdentityNode.Attributes["Name"].Value = $"ROBLOXCORPORATION.ROBLOX.{account.Username.Replace("_", "-")}";
                VisualElementsNode.Attributes["DisplayName"].Value = newTitle;
                DefaultTitleNode.Attributes["ShortName"].Value = newTitle;
                xmlDoc.Save(ManifestPath.Replace("AppxManifest2", "AppxManifest"));
            }

            RefreshToken?.Cancel();

            RunPowerShellCommand($@"Add-AppxPackage -Path \""{ManifestPath}\"" -register");

            RefreshToken = new CancellationTokenSource();
            CancellationToken CT = RefreshToken.Token;

            Task.Run(async () =>
            {
                await Task.Delay(1000);

                if (CT.IsCancellationRequested) return;

                RefreshInstanceIDs();
            }, RefreshToken.Token);
        }

        private Process RunPowerShellCommand(string command, Action<string> callback = null)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            Program.Logger.Info(process.StartInfo.Arguments);

            while (!process.StandardOutput.EndOfStream)
                if (callback != null)
                    callback(process.StandardOutput.ReadLine().Trim());
                else
                    Program.Logger.Info($"Output: {process.StandardOutput.ReadToEnd()}");

            while (!process.StandardError.EndOfStream)
                Program.Logger.Info($"Error: {process.StandardError.ReadToEnd()}");

            process.WaitForExit();

            return process;
        }

        private void RefreshInstanceIDs()
        {
            SetStatus("Refreshing Instances");

            RegistryKey UserRegistry = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            RegistryKey Apps = UserRegistry.OpenSubKey(@"SOFTWARE\Classes");

            if (Apps == null) throw new ArgumentNullException(nameof(Apps));

            Dictionary<string, string> ProgIdNames = new Dictionary<string, string>();

            foreach (var ProgId in Apps.GetSubKeyNames())
            {
                var Key = Apps.OpenSubKey(ProgId);

                if (Key is null) continue;

                var AppKey = Key.OpenSubKey(@"Application");
                var OpenKey = Key.OpenSubKey(@"shell\open");

                if (AppKey is null || OpenKey is null) continue;

                if (OpenKey.GetValue("PackageId") is string PackageID && !ProgIdNames.ContainsKey(PackageID))
                    ProgIdNames.Add(PackageID, Key.Name.Substring(Key.Name.LastIndexOf('\\') + 1));
            }

            string Username = string.Empty;

            UserInstances.Clear();

            RunPowerShellCommand("Get-AppxPackage ROBLOXCORPORATION.ROBLOX.* | Format-List -Property Name, PackageFullName", Line =>
            {
                if (Line.StartsWith("Name") && Regex.Match(Line, @"\.ROBLOX\.([\w+-]+)") is Match match && match.Groups.Count == 2)
                    Username = match.Groups[1].Value;
                else if (Line.StartsWith("PackageFullName") && !string.IsNullOrEmpty(Username))
                {
                    string PackageName = Line.Substring(Line.IndexOf(":") + 2);

                    if (ProgIdNames.ContainsKey(PackageName) && !UserInstances.ContainsKey(Username))
                        UserInstances.Add(Username, new UserInstance
                        {
                            Username = Username,
                            AppID = ProgIdNames[PackageName],
                            PackageID = PackageName
                        });

                    Username = string.Empty;
                }
            });

            if (UserInstances.Count > 0)
                AccountManager.General.Set("HasUWPInstances", "true");
            else
                AccountManager.General.RemoveProperty("HasUWPInstances");

            this.InvokeIfRequired(() =>
            {
                if (IsDisposed) return;

                SetStatus("");

                foreach (Control Control in Controls)
                    if (Control is ObjectListView)
                        Control.AllowDrop = true;
                    else
                        Control.Enabled = true;

                InstanceListBox.ClearObjects();

                InstanceListBox.SetObjects(UserInstances.Values);
            });
        }

        #endregion

        #region Protocol Registry Stuff

        // Thank you DanysysTeam for the original code. (https://github.com/DanysysTeam/PS-SFTA)
        // Thank you dylanlangston for the C# port. (https://gist.github.com/dylanlangston/f233121d699459f6ccfb01fd695585cb)

        public static void SetPTA(string progID, string protocol)
        {
            var userSID = GetUserSid();
            var userExperience = GetUserExperience();
            var userDateTime = GetHexDateTime();

            Program.Logger.Info($"UserSID: {userSID} | UserDateTime: {userDateTime} | UserExperience: {userExperience}");

            var baseInfo = $"{protocol}{userSID}{progID}{userDateTime}{userExperience}".ToLower();
            var hash = GetChoiceHash(baseInfo);

            Program.Logger.Info($"Hashing {baseInfo}");
            Program.Logger.Info($"Hash Result: {hash}");

            WriteProtocolKeys(progID, protocol, hash);
            RefreshRegistry();
        }

        static string GetUserSid() => System.Security.Principal.WindowsIdentity.GetCurrent().User.Value.ToLowerInvariant();

        static string GetUserExperience()
        {
            var hardcodedExperience = "User Choice set via Windows User Experience {D18B6DD5-6124-4341-9318-804003BAFA0B}";
            var userExperienceSearch = "User Choice set via Windows User Experience";
            var user32Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "Shell32.dll");
            using var fileStream = File.Open(user32Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var binaryReader = new BinaryReader(fileStream);
            var bytesData = binaryReader.ReadBytes(5242880);
            fileStream.Close();
            var dataString = Encoding.Unicode.GetString(bytesData);
            var position1 = dataString.IndexOf(userExperienceSearch);
            var position2 = dataString.IndexOf("}", position1);
            try { return dataString.Substring(position1, position2 - position1 + 1); }
            catch { return hardcodedExperience; }
        }

        static string GetHexDateTime()
        {
            var time = DateTime.Now;
            time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);
            var fileTime = time.ToFileTime();
            var high = fileTime >> 32;
            var low = fileTime & 0xFFFFFFFFL;
            return (high.ToString("X8") + low.ToString("X8")).ToLowerInvariant();
        }

        private static string GetChoiceHash(string baseInfo)
        {
            static long GetShiftRight(long value, int count) => (value & 0x80000000) == 0 ? value >> count : (value >> count) ^ 0xFFFF0000;

            Program.Logger.Info(baseInfo);

            List<byte> bytesBaseInfo = Encoding.Unicode.GetBytes(baseInfo).ToList();

            bytesBaseInfo.Add(0x00);
            bytesBaseInfo.Add(0x00);

            var MD5 = System.Security.Cryptography.MD5.Create();
            byte[] bytesMD5 = MD5.ComputeHash(bytesBaseInfo.ToArray());

            var lengthBase = (baseInfo.Length * 2) + 2;
            var length = (((lengthBase & 4) <= 1) ? 1 : 0) + (GetShiftRight(lengthBase, 2)) - 1;
            var bytes = bytesBaseInfo.ToArray();

            if (length > 1)
            {
                MAP map = new MAP();
                map.CACHE = 0;
                map.OUTHASH1 = 0;
                map.PDATA = 0;
                map.MD51 = (BitConverter.ToInt32(bytesMD5, 0) | 1) + (int)0x69FB0000L;
                map.MD52 = (BitConverter.ToInt32(bytesMD5, 4) | 1) + (int)0x13DB0000L;
                map.INDEX = (int)GetShiftRight(length - 2, 1);
                map.COUNTER = map.INDEX + 1;

                while (map.COUNTER > 0)
                {
                    map.R0 = BitConverter.ToInt32(BitConverter.GetBytes(BitConverter.ToInt32(bytes, (int)map.PDATA) + map.OUTHASH1), 0);
                    map.R1[0] = BitConverter.ToInt32(BitConverter.GetBytes(BitConverter.ToInt32(bytes, (int)map.PDATA + 4)), 0);
                    map.PDATA = map.PDATA + 8;
                    map.R2[0] = BitConverter.ToInt32(BitConverter.GetBytes((map.R0 * map.MD51) - (0x10FA9605L * GetShiftRight(map.R0, 16))), 0);
                    map.R2[1] = BitConverter.ToInt32(BitConverter.GetBytes((0x79F8A395L * map.R2[0]) + (0x689B6B9FL * GetShiftRight(map.R2[0], 16))), 0);
                    map.R3 = BitConverter.ToInt32(BitConverter.GetBytes((0xEA970001L * map.R2[1]) - (0x3C101569L * GetShiftRight(map.R2[1], 16))), 0);
                    map.R4[0] = BitConverter.ToInt32(BitConverter.GetBytes(map.R3 + map.R1[0]), 0);
                    map.R5[0] = BitConverter.ToInt32(BitConverter.GetBytes(map.CACHE + map.R3), 0);
                    map.R6[0] = BitConverter.ToInt32(BitConverter.GetBytes((map.R4[0] * map.MD52) - (0x3CE8EC25L * GetShiftRight(map.R4[0], 16))), 0);
                    map.R6[1] = BitConverter.ToInt32(BitConverter.GetBytes((0x59C3AF2DL * map.R6[0]) - (0x2232E0F1L * GetShiftRight(map.R6[0], 16))), 0);
                    map.OUTHASH1 = BitConverter.ToInt32(BitConverter.GetBytes((0x1EC90001L * map.R6[1]) + (0x35BD1EC9L * GetShiftRight(map.R6[1], 16))), 0);
                    map.OUTHASH2 = BitConverter.ToInt32(BitConverter.GetBytes(map.R5[0] + map.OUTHASH1), 0);
                    map.CACHE = map.OUTHASH2;
                    map.COUNTER = map.COUNTER - 1;
                }

                var outHash = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var buffer = BitConverter.GetBytes(map.OUTHASH1).Take(4).ToArray();
                buffer.CopyTo(outHash, 0);
                buffer = BitConverter.GetBytes(map.OUTHASH2).Take(4).ToArray();
                buffer.CopyTo(outHash, 4);

                map = new MAP();
                map.CACHE = 0;
                map.OUTHASH1 = 0;
                map.PDATA = 0;
                map.MD51 = BitConverter.ToInt32(bytesMD5, 0) | 1;
                map.MD52 = BitConverter.ToInt32(bytesMD5, 4) | 1;
                map.INDEX = (int)GetShiftRight(length - 2, 1);
                map.COUNTER = map.INDEX + 1;

                while (map.COUNTER > 0)
                {
                    map.R0 = BitConverter.ToInt32(BitConverter.GetBytes(BitConverter.ToInt32(bytes, (int)map.PDATA) + map.OUTHASH1), 0);
                    map.PDATA = map.PDATA + 8;
                    map.R1[0] = BitConverter.ToInt32(BitConverter.GetBytes(map.R0 * map.MD51), 0);
                    map.R1[1] = BitConverter.ToInt32(BitConverter.GetBytes((0xB1110000L * map.R1[0]) - (0x30674EEFL * GetShiftRight(map.R1[0], 16))), 0);
                    map.R2[0] = BitConverter.ToInt32(BitConverter.GetBytes((0x5B9F0000L * map.R1[1]) - (0x78F7A461L * GetShiftRight(map.R1[1], 16))), 0);
                    map.R2[1] = BitConverter.ToInt32(BitConverter.GetBytes((0x12CEB96DL * GetShiftRight(map.R2[0], 16)) - (0x46930000L * map.R2[0])), 0);
                    map.R3 = BitConverter.ToInt32(BitConverter.GetBytes((0x1D830000L * map.R2[1]) + (0x257E1D83L * GetShiftRight(map.R2[1], 16))), 0);
                    map.R4[0] = BitConverter.ToInt32(BitConverter.GetBytes(map.MD52 * (map.R3 + BitConverter.ToInt32(bytes, (int)map.PDATA - 4))), 0);
                    map.R4[1] = BitConverter.ToInt32(BitConverter.GetBytes((0x16F50000L * map.R4[0]) - (0x5D8BE90BL * GetShiftRight(map.R4[0], 16))), 0);
                    map.R5[0] = BitConverter.ToInt32(BitConverter.GetBytes((0x96FF0000L * map.R4[1]) - (0x2C7C6901L * GetShiftRight(map.R4[1], 16))), 0);
                    map.R5[1] = BitConverter.ToInt32(BitConverter.GetBytes((0x2B890000L * map.R5[0]) + (0x7C932B89L * GetShiftRight(map.R5[0], 16))), 0);
                    map.OUTHASH1 = BitConverter.ToInt32(BitConverter.GetBytes((0x9F690000L * map.R5[1]) - (0x405B6097L * GetShiftRight(map.R5[1], 16))), 0);
                    map.OUTHASH2 = BitConverter.ToInt32(BitConverter.GetBytes(map.OUTHASH1 + map.CACHE + map.R3), 0);
                    map.CACHE = map.OUTHASH2;
                    map.COUNTER = map.COUNTER - 1;
                }

                buffer = BitConverter.GetBytes(map.OUTHASH1).Take(4).ToArray();
                buffer.CopyTo(outHash, 8);
                buffer = BitConverter.GetBytes(map.OUTHASH2).Take(4).ToArray();
                buffer.CopyTo(outHash, 12);

                var outHashBase = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                var hashValue1 = BitConverter.ToInt32(outHash, 8) ^ BitConverter.ToInt32(outHash, 0);
                var hashValue2 = BitConverter.ToInt32(outHash, 12) ^ BitConverter.ToInt32(outHash, 4);

                buffer = BitConverter.GetBytes(hashValue1);
                buffer.CopyTo(outHashBase, 0);
                buffer = BitConverter.GetBytes(hashValue2);
                buffer.CopyTo(outHashBase, 4);

                return Convert.ToBase64String(outHashBase);
            }

            return string.Empty;
        }

        static void WriteProtocolKeys(string progId, string protocol, string progHash)
        {
            var keyPath = $@"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\{protocol}\UserChoice";
            Registry.SetValue(keyPath, "Hash", progHash);
            Registry.SetValue(keyPath, "ProgId", progId);
        }

        static string[] ReadProtocolKeys(string protocol)
        {
            var keyPath = $@"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\{protocol}\UserChoice";
            return new string[]
            {
                (string)Registry.GetValue(keyPath, "Hash", null),
                (string)Registry.GetValue(keyPath, "ProgId", null)
            };
        }

        static void RefreshRegistry() => Natives.SHChangeNotify(0x8000000, 0, IntPtr.Zero, IntPtr.Zero);

        private class MAP
        {
            public long PDATA = 0,
                CACHE = 0,
                COUNTER = 0,
                INDEX = 0,
                MD51 = 0,
                MD52 = 0,
                OUTHASH1 = 0,
                OUTHASH2 = 0,
                R0 = 0,
                R3 = 0;
            public Dictionary<int, long>
                R1 = new Dictionary<int, long>() { { 0, 0 }, { 1, 0 } },
                R2 = new Dictionary<int, long>() { { 0, 0 }, { 1, 0 } },
                R4 = new Dictionary<int, long>() { { 0, 0 }, { 1, 0 } },
                R5 = new Dictionary<int, long>() { { 0, 0 }, { 1, 0 } },
                R6 = new Dictionary<int, long>() { { 0, 0 }, { 1, 0 } };
        }

        #endregion

        #region Theme

        public void ApplyTheme()
        {
            BackColor = ThemeEditor.FormsBackground;
            ForeColor = ThemeEditor.FormsForeground;

            ApplyTheme(Controls);
        }

        public void ApplyTheme(Control.ControlCollection _Controls)
        {
            foreach (Control control in _Controls)
            {
                if (control is Button || control is CheckBox)
                {
                    if (control is Button)
                    {
                        Button b = control as Button;
                        b.FlatStyle = ThemeEditor.ButtonStyle;
                        b.FlatAppearance.BorderColor = ThemeEditor.ButtonsBorder;
                    }

                    if (!(control is CheckBox)) control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is TextBox || control is RichTextBox)
                {
                    if (control is Classes.BorderedTextBox)
                    {
                        Classes.BorderedTextBox b = control as Classes.BorderedTextBox;
                        b.BorderColor = ThemeEditor.TextBoxesBorder;
                    }

                    if (control is Classes.BorderedRichTextBox)
                    {
                        Classes.BorderedRichTextBox b = control as Classes.BorderedRichTextBox;
                        b.BorderColor = ThemeEditor.TextBoxesBorder;
                    }

                    control.BackColor = ThemeEditor.TextBoxesBackground;
                    control.ForeColor = ThemeEditor.TextBoxesForeground;
                }
                else if (control is Label)
                {
                    control.BackColor = ThemeEditor.LabelTransparent ? Color.Transparent : ThemeEditor.LabelBackground;
                    control.ForeColor = ThemeEditor.LabelForeground;
                }
                else if (control is ListBox || control is ObjectListView)
                {
                    if (control is ObjectListView view) view.HeaderStyle = ThemeEditor.ShowHeaders ? ColumnHeaderStyle.Clickable : ColumnHeaderStyle.None;
                    control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;
                }
                else if (control is StatusStrip || control is ToolStrip)
                {
                    ApplyTheme(control.Controls);

                    control.BackColor = ThemeEditor.ButtonsBackground;
                    control.ForeColor = ThemeEditor.ButtonsForeground;

                    if (control is StatusStrip statusStrip)
                        foreach (var item in statusStrip.Items)
                            if (item is ToolStripStatusLabel tLabel)
                                tLabel.LinkColor = ThemeEditor.LabelForeground;
                }
                else if (control is FastColoredTextBoxNS.FastColoredTextBox)
                    control.ForeColor = Color.Black;
                else if (control is FlowLayoutPanel || control is Panel || control is TabControl)
                    ApplyTheme(control.Controls);
            }
        }

        #endregion
    }

    public struct UserInstance
    {
        public string Username;
        public string PackageID;
        public string AppID;
    }
}