using IWshRuntimeLibrary;
using RBX_Alt_Manager;
using RBX_Alt_Manager.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using File = System.IO.File;

namespace Auto_Update
{
    public partial class AutoUpdater : Form
    {
        public AutoUpdater()
        {
            InitializeComponent();
        }

        private string FileName = "Update.zip";
        private readonly string UpdatePath = Path.Combine(Program.DataDirectory.FullName, "Update");
        private long TotalDownloadSize = 0;
        private delegate void SafeCallDelegateSetProgress(int Progress);
        private delegate void SafeCallDelegateSetStatus(string Text);

        private async void AutoUpdater_Load(object sender, EventArgs e)
        {
            DirectoryInfo UpdateDir = new DirectoryInfo(UpdatePath);

            if (UpdateDir.Exists) UpdateDir.RecursiveDelete();

            FileName = Path.Combine(Directory.GetCurrentDirectory(), FileName);

            await Download();
        }

        private void SetStatus(string Text)
        {
            if (Status.InvokeRequired)
            {
                SafeCallDelegateSetStatus setStatus = new SafeCallDelegateSetStatus(SetStatus);
                Status.Invoke(setStatus, new object[] { Text });
            }
            else
                Status.Text = Text;

            return;
        }
        private void SetProgress(int Progress)
        {
            if (ProgressBar.InvokeRequired)
            {
                SafeCallDelegateSetProgress setProgress = new SafeCallDelegateSetProgress(SetProgress);
                ProgressBar.Invoke(setProgress, new object[] { Progress });
            }
            else
                ProgressBar.Value = Progress;

            return;
        }

        static double B2MB(double bytes) => Math.Round(bytes / 1024f / 1024f, 2);

        private void progressChanged(float value)
        {
            SetStatus(value == 1 ? "Extracting Files..." : $"Downloaded {string.Format("{0:0}", value * 100f)}% of {string.Format("{0:0.00}", B2MB(TotalDownloadSize))}MB");
            SetProgress((int)(value * 100f));
        }

        private async Task Download()
        {
#if DEBUG
            await Task.Run(Extract);
#else
            using var client = new HttpClient() { Timeout = TimeSpan.FromMinutes(20) };
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36");
            string Releases = await client.GetStringAsync("https://api.github.com/repos/ic3w0lf22/Roblox-Account-Manager/releases/tags/0.0");
            Match match = Regex.Match(Releases, @"""browser_download_url"":\s*""?([^""]+)");

            if (match.Success && match.Groups.Count >= 2)
            {
                if (match.Groups[1].Value.Contains(".rar"))
                {
                    Environment.Exit(247);
                    return;
                }

                string DownloadUrl = match.Groups[1].Value;

                TotalDownloadSize = (await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, DownloadUrl))).Content.Headers.ContentLength.Value;

                Progress<float> progress = new Progress<float>(progressChanged);

                using (var file = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    await client.DownloadAsync(DownloadUrl, file, progress);

                await Task.Run(Extract);
            }
#endif
        }

        private void Extract()
        {
            bool ErorrOccured = false;

            try
            {
                foreach (Process p in Process.GetProcessesByName("Roblox Account Manager"))
                    if (p.Id != Process.GetCurrentProcess().Id)
                        p.Kill();
            }
            catch { }

            FileInfo Current = new FileInfo(Application.ExecutablePath);

#if !DEBUG
            try
#endif
            {
                using ZipArchive archive = ZipFile.OpenRead(FileName);
                archive.ExtractToDirectory(UpdatePath);
                bool OldExecutableExists = File.Exists(Path.Combine(AppContext.BaseDirectory, "RBX Alt Manager.exe"));

                foreach (string s in Directory.GetFiles(UpdatePath))
                {
                    string FN = Path.Combine(AppContext.BaseDirectory, Path.GetFileName(s));

#if DEBUG
                    if (FN == Application.ExecutablePath) FN = Path.Combine(AppContext.BaseDirectory, "Test.exe");
#endif

                    if (File.Exists(Path.Combine(AppContext.BaseDirectory, FN)))
                        File.Delete(Path.Combine(AppContext.BaseDirectory, FN));
                }

                foreach (string s in Directory.GetDirectories(UpdatePath))
                {
                    DirectoryInfo dir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, Path.GetFileName(s)));

                    if (dir.Exists)
                        dir.RecursiveDelete();
                }

                DirectoryInfo UpdateDir = new DirectoryInfo(UpdatePath);

                foreach (FileInfo file in UpdateDir.GetFiles())
                    if (file.Name != Current.Name)
                        if (file.Name != "RBX Alt Manager.exe" || (file.Name == "RBX Alt Manager.exe" && OldExecutableExists)) // allows old shortcuts to keep working
                            file.MoveTo(Path.Combine(AppContext.BaseDirectory, file.Name));

                foreach (DirectoryInfo dir in UpdateDir.GetDirectories())
                {
                    dir.MoveTo(Path.Combine(AppContext.BaseDirectory, dir.Name));

                    foreach (FileInfo file in dir.GetFiles()) // remove old files from main directory
                    {
                        if (File.Exists(Path.Combine(AppContext.BaseDirectory, file.Name)))
                            File.Delete(Path.Combine(AppContext.BaseDirectory, file.Name));
                    }
                }

                UpdateDir.RecursiveDelete();
            }
#if !DEBUG
            catch (Exception x)
            {
                ErorrOccured = true;
                SetStatus("Error");
                Invoke(new Action(() =>
                {
                    var Result = MessageBox.Show(this, $"Something went wrong! \n{x.Message}\n{x.StackTrace}", "Auto Updater", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                    if (Result == DialogResult.Retry)
                    {
                        Application.Restart();
                        Environment.Exit(0);
                    }
                    else
                        Environment.Exit(0);
                }));
            }
#endif

            if (ErorrOccured)
                return;

            SetStatus("Done!");

#if !DEBUG
            File.Delete(FileName);
#endif

            string ProgramFN = Path.Combine(AppContext.BaseDirectory, "Roblox Account Manager.exe");

            if (RBX_Alt_Manager.Program.Elevated)
            {
                if (Utilities.YesNoPrompt("Roblox Account Manager", "Create Start-Menu shortcut?", "", false))
                {
                    string StartMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs", "Roblox Account Manager");

                    if (!Directory.Exists(StartMenuPath))
                        Directory.CreateDirectory(StartMenuPath);

                    IWshShortcut shortcut = (IWshShortcut)new WshShell().CreateShortcut(Path.Combine(StartMenuPath, "Roblox Account Manager.lnk"));

                    shortcut.Description = "Roblox Account Manager";
                    shortcut.TargetPath = ProgramFN;
                    shortcut.WorkingDirectory = Directory.GetParent(ProgramFN).FullName;
                    shortcut.Save();
                }

                if (File.Exists(ProgramFN))
                    RunAsDesktopUser(ProgramFN);
            }
            else
                Process.Start(ProgramFN);

            Environment.Exit(0);
        }

        private static void RunAsDesktopUser(string fileName) // ahhh, pasting. (https://stackoverflow.com/a/40501607)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));

            // To start process as shell user you will need to carry out these steps:
            // 1. Enable the SeIncreaseQuotaPrivilege in your current token
            // 2. Get an HWND representing the desktop shell (Natives.GetShellWindow)
            // 3. Get the Process ID(PID) of the process associated with that window(Natives.GetWindowThreadProcessId)
            // 4. Open that process(Natives.OpenProcess)
            // 5. Get the access token from that process (Natives.OpenProcessToken)
            // 6. Make a primary token with that token(Natives.DuplicateTokenEx)
            // 7. Start the new process with that primary token(Natives.CreateProcessWithTokenW)

            var hProcessToken = IntPtr.Zero;
            // Enable SeIncreaseQuotaPrivilege in this process.  (This won't work if current process is not elevated.)
            try
            {
                var process = Natives.GetCurrentProcess();
                if (!Natives.OpenProcessToken(process, 0x0020, ref hProcessToken))
                    return;

                var tkp = new Natives.TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new Natives.LUID_AND_ATTRIBUTES[1]
                };

                if (!Natives.LookupPrivilegeValue(null, "SeIncreaseQuotaPrivilege", ref tkp.Privileges[0].Luid))
                    return;

                tkp.Privileges[0].Attributes = 0x00000002;

                if (!Natives.AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero))
                    return;
            }
            finally
            {
                Natives.CloseHandle(hProcessToken);
            }

            // Get an HWND representing the desktop shell.
            // CAVEATS:  This will fail if the shell is not running (crashed or terminated), or the default shell has been
            // replaced with a custom shell.  This also won't return what you probably want if Explorer has been terminated and
            // restarted elevated.
            var hwnd = Natives.GetShellWindow();
            if (hwnd == IntPtr.Zero)
                return;

            var hShellProcess = IntPtr.Zero;
            var hShellProcessToken = IntPtr.Zero;
            var hPrimaryToken = IntPtr.Zero;
            try
            {
                // Get the PID of the desktop shell process.
                if (Natives.GetWindowThreadProcessId(hwnd, out uint dwPID) == 0)
                    return;

                // Open the desktop shell process in order to query it (get the token)
                hShellProcess = Natives.OpenProcess(Natives.ProcessAccessFlags.QueryInformation, false, dwPID);
                if (hShellProcess == IntPtr.Zero)
                    return;

                // Get the process token of the desktop shell.
                if (!Natives.OpenProcessToken(hShellProcess, 0x0002, ref hShellProcessToken))
                    return;

                var dwTokenRights = 395U;

                // Duplicate the shell's process token to get a primary token.
                // Based on experimentation, this is the minimal set of rights required for Natives.CreateProcessWithTokenW (contrary to current documentation).
                if (!Natives.DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, Natives.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, Natives.TOKEN_TYPE.TokenPrimary, out hPrimaryToken))
                    return;

                // Start the target process with the new token.
                var si = new Natives.STARTUPINFO();
                var pi = new Natives.PROCESS_INFORMATION();
                if (!Natives.CreateProcessWithTokenW(hPrimaryToken, 0, fileName, "", 0, IntPtr.Zero, Path.GetDirectoryName(fileName), ref si, out pi))
                    return;
            }
            finally
            {
                Natives.CloseHandle(hShellProcessToken);
                Natives.CloseHandle(hPrimaryToken);
                Natives.CloseHandle(hShellProcess);
            }
        }
    }
}