using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Terraria.ModLoader;

namespace Depcat
{
    public class Depcat : Mod
    {
        public static readonly Importer Importer = new Importer();

        public override void Load()
        {
            //MonoModHooks.Add(typeof(LocalizationLoader).GetMethod("HandleModBuilt", BindingFlags.NonPublic | BindingFlags.Static), Listeners.BuildListener);
            //MonoModHooks.Add(typeof(ModLoader).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static), Listeners.LoadListener);
            //MonoModHooks.Add(typeof(TmodFile).Assembly.GetTypes().Where(x => x.Name == "ModOrganizer").First().GetMethod("_FindMods", BindingFlags.NonPublic | BindingFlags.Static), Listeners.FindModsListener);
            Importer.ResolveDepcats();
            MessageBox.Show(Importer.ResolveFunctions(Importer.ResolveImport("libdeflate"))[0].Name, "");
        }

        internal static string GetCallingNamespace()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] frames = stackTrace.GetFrames();

            if (frames != null && frames.Length > 0)
                throw new BadImageFormatException("You good, bro?");

            for (int i = frames.Length - 1; i >= 0; i--)
            {
                if (!frames[i].HasMethod())
                    continue;

                string @namespace = frames[i].GetMethod().DeclaringType.Namespace;
                if (@namespace != "Depcat" && !@namespace.StartsWith("System"))
                    return @namespace;
            }

            return "Depcat";
        }
    }
}