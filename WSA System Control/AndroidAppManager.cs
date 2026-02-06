using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace WSA_System_Control
{
    public class AndroidAppManager
    {
        private const string AdbPort = "58526";

        private class StartApp
        {
            public string Name { get; set; } = string.Empty;
            public string AppID { get; set; } = string.Empty;
        }

        private static List<AndroidApp>? _cachedApps;

        public static async Task<List<AndroidApp>> GetInstalledAppsAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && _cachedApps != null)
            {
                return _cachedApps;
            }

            var apps = new List<AndroidApp>();
            
            // Ensure ADB is connected to WSA
            await RunAdbCommandAsync($"connect 127.0.0.1:{AdbPort}");

            // Get Windows Start Apps to match names
            var startApps = await GetWindowsStartAppsAsync();
            
            // Get Shortcuts to extract icons
            var shortcuts = GetShortcutPaths();

            // Get ALL packages to find system apps that might be in Start Menu (like Play Store)
            string allPackagesOutput = await RunAdbCommandAsync("shell pm list packages");
            var allPackages = allPackagesOutput.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Replace("package:", "").Trim());

            foreach (var packageName in allPackages)
            
            {
                // Only process packages that follow standard naming (e.g. "com.example.app")
                // This generically filters out system processes like "android", "root", etc., 
                // and prevents false positive matches against Windows apps.
                if (!packageName.Contains('.')) continue;

                var match = startApps.FirstOrDefault(a => a.AppID.Contains(packageName, StringComparison.OrdinalIgnoreCase));
                
                if (match != null)
                {
                    string appName = match.Name;
                    
                    // Avoid duplicates if a package somehow appears twice
                    if (!apps.Any(a => a.PackageName == packageName))
                    {
                        // Try to find icon
                        Image? icon = null;
                        if (shortcuts.TryGetValue(appName, out string shortcutPath))
                        {
                            icon = ExtractIcon(shortcutPath);
                        }

                        apps.Add(new AndroidApp(packageName, appName, icon));
                    }
                }
            }

            _cachedApps = [.. apps.OrderBy(a => a.AppName)];
            return _cachedApps;
        }

        private static async Task<List<StartApp>> GetWindowsStartAppsAsync()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -Command \"Get-StartApps | Select-Object Name, AppID | ConvertTo-Json\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    if (string.IsNullOrEmpty(output)) return new List<StartApp>();

                    return JsonSerializer.Deserialize<List<StartApp>>(output) ?? new List<StartApp>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PowerShell Error: {ex.Message}");
                return new List<StartApp>();
            }
        }

        private static string GetPrettyName(string packageName)
        {
            // Simple logic to make package names look a bit better if they are the only thing we have
            var parts = packageName.Split('.');
            if (parts.Length > 0)
            {
                string lastPart = parts[parts.Length - 1];
                return char.ToUpper(lastPart[0]) + lastPart.Substring(1);
            }
            return packageName;
        }

        private static async Task<string> RunAdbCommandAsync(string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "adb.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    return output ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ADB Error: {ex.Message}");
                return string.Empty;
            }
        }

        public static void LaunchApp(string packageName)
        {
            Process proc = new Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = "WSAClient.exe";
            proc.StartInfo.Arguments = $"/launch wsa://{packageName}";
            proc.Start();
        }

        private static Dictionary<string, string> GetShortcutPaths()
        {
            var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var locations = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
            };

            foreach (var location in locations)
            {
                if (Directory.Exists(location))
                {
                    try
                    {
                        var files = Directory.GetFiles(location, "*.lnk", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            string name = Path.GetFileNameWithoutExtension(file);
                            if (!paths.ContainsKey(name))
                            {
                                paths[name] = file;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error scanning shortcuts: {ex.Message}");
                    }
                }
            }
            return paths;
        }

        private static Image? ExtractIcon(string path)
        {
            try
            {
                // Try to resolve shortcut target to get clean icon without overlay
                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType != null)
                {
                    dynamic? shell = Activator.CreateInstance(shellType);
                    if (shell != null)
                    {
                        dynamic shortcut = shell.CreateShortcut(path);
                        string iconLocation = shortcut.IconLocation;

                        if (!string.IsNullOrEmpty(iconLocation))
                        {
                            // IconLocation format is usually "Path,Index"
                            string[] parts = iconLocation.Split(',');
                            string iconFile = parts[0];
                            int iconIndex = 0;
                            if (parts.Length > 1)
                            {
                                int.TryParse(parts[1], out iconIndex);
                            }
                            
                            // Expand environment variables
                            iconFile = Environment.ExpandEnvironmentVariables(iconFile);

                            if (File.Exists(iconFile))
                            {
                                return ExtractIconFromPath(iconFile, iconIndex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resolving shortcut icon: {ex.Message}");
            }

            // Fallback to extraction from shortcut file (may include overlay)
            try
            {
                if (File.Exists(path))
                {
                    using (var icon = Icon.ExtractAssociatedIcon(path))
                    {
                        return icon?.ToBitmap();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting icon: {ex.Message}");
            }
            return null;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private static Image? ExtractIconFromPath(string path, int index)
        {
            try 
            {
                IntPtr[] phiconLarge = new IntPtr[1];
                IntPtr[] phiconSmall = new IntPtr[1];

                // Extract 1 icon
                if (ExtractIconEx(path, index, phiconLarge, phiconSmall, 1) > 0)
                {
                    Image? image = null;
                    if (phiconLarge[0] != IntPtr.Zero)
                    {
                        using (var icon = Icon.FromHandle(phiconLarge[0]))
                        {
                            image = icon.ToBitmap();
                        }
                        DestroyIcon(phiconLarge[0]);
                    }
                    
                    if (phiconSmall[0] != IntPtr.Zero)
                    {
                         DestroyIcon(phiconSmall[0]);
                    }

                    return image;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting icon from path: {ex.Message}");
            }
            return null;
        }
    }
}
