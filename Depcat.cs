using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace Depcat
{
    public class Depcat : Mod
    {
        internal static readonly Importer Importer = new Importer();

        public override void Load()
        {
            MonoModHooks.Add(typeof(ModLoader).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static), Listeners.LoadListener);
            MonoModHooks.Add(typeof(TmodFile).Assembly.GetTypes().Where(x => x.Name == "ModOrganizer").First().GetMethod("_FindMods", BindingFlags.NonPublic | BindingFlags.Static), Listeners.FindModsListener);
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

        public static Importer GetImporter()
        {
            return Importer;
        }
    }
}