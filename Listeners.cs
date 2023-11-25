using System.Threading;

namespace Depcat
{
    internal static class Listeners
    {
        internal delegate void orig_Load(CancellationToken token);
        internal delegate dynamic[] orig__FindMods(bool ignoreModsFolder, bool ignoreWorkshop, bool logDuplicates);

        internal static void LoadListener(orig_Load orig, CancellationToken token)
        {
            Depcat.Importer.ResolveDepcats();
            orig(token);
        }

        internal static dynamic[] FindModsListener(orig__FindMods orig, bool ignoreModsFolder, bool ignoreWorkshop, bool logDuplicates)
        {
            Depcat.Importer.ResolveDepcats();
            return orig(ignoreModsFolder, ignoreWorkshop, logDuplicates);
        }
    }
}
