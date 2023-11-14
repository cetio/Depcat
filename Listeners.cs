namespace Depcat
{
    /*internal static class Listeners
    {
        internal delegate void orig_HandleModBuilt(string mod);
        internal delegate void orig_Load(CancellationToken token);
        internal delegate dynamic[] orig__FindMods(bool ignoreModsFolder, bool ignoreWorkshop, bool logDuplicates);

        internal static void BuildListener(orig_HandleModBuilt orig, string mod)
        {
            orig(mod);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
            bundle:
                try
                {
                    DirectoryInfo sourcePath = new DirectoryInfo(Path.Combine(ModLoader.ModPath, "..", $"ModSources\\{mod}\\"));
                    DirectoryInfo modsPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "..", "..", "Mods"));
                    DirectoryInfo depcatPath = sourcePath.GetDirectories().Where(x => x.Name == "Depcat").FirstOrDefault();
                    string modPath = Path.Combine(modsPath.FullName, $"{sourcePath.Name}.TMod");

                    if (depcatPath != null)
                        IO.Bundle(modPath, depcatPath.GetFiles());
                }
                catch (IOException)
                {
                    Thread.Sleep(1000);
                    goto bundle;
                }
            }).Start();
        }

        internal static void LoadListener(orig_Load orig, CancellationToken token)
        {
            foreach (FileInfo file in new DirectoryInfo(ModLoader.ModPath).EnumerateFiles())
            {
                if (IO.IsDepcatFile(file.FullName))
                    IO.Unbundle(file.FullName);
            }

            orig(token);
        }

        internal static dynamic[] FindModsListener(orig__FindMods orig, bool ignoreModsFolder, bool ignoreWorkshop, bool logDuplicates)
        {
            foreach (FileInfo file in new DirectoryInfo(ModLoader.ModPath).EnumerateFiles())
            {
                if (IO.IsDepcatFile(file.FullName))
                    IO.Unbundle(file.FullName);
            }

            return orig(ignoreModsFolder, ignoreWorkshop, logDuplicates);
        }
    }*/
}
