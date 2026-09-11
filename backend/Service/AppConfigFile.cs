using System;
using System.Configuration;
using System.IO;

namespace Quanto
{
    /// <summary>
    /// Single-file publish does not load App.config unless it sits next to the EXE
    /// as Quanto.Client.dll.config. This points ConfigurationManager at that file.
    /// </summary>
    public static class AppConfigFile
    {
        public static string Path { get; private set; }

        public static void Initialize()
        {
            var dir = AppContext.BaseDirectory;
            var dllConfig = System.IO.Path.Combine(dir, "Quanto.Client.dll.config");
            var exeConfig = System.IO.Path.Combine(dir, "Quanto.Client.exe.config");
            var appConfig = System.IO.Path.Combine(dir, "App.config");

            string source = null;
            if (File.Exists(dllConfig))
                source = dllConfig;
            else if (File.Exists(exeConfig))
                source = exeConfig;
            else if (File.Exists(appConfig))
                source = appConfig;

            if (source != null)
            {
                if (!File.Exists(dllConfig))
                    File.Copy(source, dllConfig, true);
                if (!File.Exists(exeConfig))
                    File.Copy(source, exeConfig, true);
            }

            Path = File.Exists(dllConfig) ? dllConfig : source;
            if (!string.IsNullOrEmpty(Path))
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", Path);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static Configuration Open()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
                Initialize();

            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
                return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var map = new ExeConfigurationFileMap { ExeConfigFilename = Path };
            return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        public static void SaveSetting(string key, string value)
        {
            var config = Open();
            var settings = config.AppSettings.Settings;
            if (settings[key] == null)
                settings.Add(key, value ?? "");
            else
                settings[key].Value = value ?? "";
            config.Save(ConfigurationSaveMode.Modified);

            try
            {
                ConfigurationManager.AppSettings.Set(key, value ?? "");
            }
            catch
            {
            }

            ConfigurationManager.RefreshSection("appSettings");
            SyncSiblingConfigFiles();
        }

        private static void SyncSiblingConfigFiles()
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
                return;
            var dir = System.IO.Path.GetDirectoryName(Path) ?? AppContext.BaseDirectory;
            foreach (var name in new[] { "Quanto.Client.dll.config", "Quanto.Client.exe.config", "App.config" })
            {
                var dest = System.IO.Path.Combine(dir, name);
                if (string.Equals(dest, Path, StringComparison.OrdinalIgnoreCase))
                    continue;
                try
                {
                    File.Copy(Path, dest, true);
                }
                catch
                {
                }
            }
        }
    }
}
