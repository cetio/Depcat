using Depcat.IO.TMod;
using PeNet;
using PeNet.Header.Pe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Windows.Forms;
using Terraria.ModLoader;

namespace Depcat
{
    public class Importer
    {
        public static readonly string TModPath = Path.Combine(ModLoader.ModPath, "..");
        public static readonly DirectoryInfo DepcatPath = new DirectoryInfo(Path.Combine(ModLoader.ModPath, "..", "Depcat"));

        static Importer()
        {
            Directory.CreateDirectory(DepcatPath.FullName);
        }

        public FileInfo ResolveImport(string name)
        {
            return DepcatPath.EnumerateFiles().FirstOrDefault(x => x.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase));
        }

        public ExportFunction ResolveImport(string name, string entrypoint)
        {
            FileInfo file = ResolveImport(name);
            if (file.Extension != ".dll")
                return null;
            return new PeFile(file.FullName).ExportedFunctions.FirstOrDefault(x => x.Name == entrypoint);
        }

        public unsafe void Call(string name, string entrypoint, params dynamic[] args)
        {
            ExportFunction func = ResolveImport(name, entrypoint);
            
            switch (args.Length)
            {
                case 0:
                    ((delegate* unmanaged<void>)func.Address)();
                    break;
                case 1:
                    ((delegate* unmanaged<dynamic, void>)func.Address)(args[0]);
                    break;
                case 2:
                    ((delegate* unmanaged<dynamic, dynamic, void>)func.Address)(args[0], args[1]);
                    break;
                case 3:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2]);
                    break;
                case 4:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2], args[3]);
                    break;
                case 5:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2], args[3], args[4]);
                    break;
                case 6:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5]);
                    break;
                case 7:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                    break;
                case 8:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    break;
                case 9:
                    ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, void>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                    break;
            }
        }

        public unsafe dynamic Call<T>(string name, string entrypoint, params dynamic[] args)
        {
            ExportFunction func = ResolveImport(name, entrypoint);

            switch (args.Length)
            {
                case 1:
                    return ((delegate* unmanaged<dynamic, T>)func.Address)(args[0]);
                case 2:
                    return ((delegate* unmanaged<dynamic, dynamic, T>)func.Address)(args[0], args[1]);
                case 3:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2]);
                case 4:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2], args[3]);
                case 5:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2], args[3], args[4]);
                case 6:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5]);
                case 7:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                case 8:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                case 9:
                    return ((delegate* unmanaged<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, T>)func.Address)(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                default:
                    return ((delegate* unmanaged<T>)func.Address)();
            }
        }

        public void ResolveDepcats()
        {
            foreach (FileInfo file in new DirectoryInfo(ModLoader.ModPath).EnumerateFiles())
            {
                if (!IsDepcat(file, out TModFile tmodFile))
                    continue;

                foreach (TModEntry entry in tmodFile.Entries.Where(x => x.FullName.StartsWith("Depcat/")))
                {
                    try
                    {
                        File.WriteAllBytes(Path.Combine(TModPath, entry.FullName), entry.Data);
                    }
                    catch (IOException)
                    {
                        // We don't really care, we can just get on with it.
                        continue;
                    }
                }
            }
        }

        public bool IsDepcat(FileInfo file, out TModFile tmodFile)
        {
            return TModFile.TryReadFromPath(file.FullName, out tmodFile) && tmodFile.Entries.Any(x => x.FullName.StartsWith("Depcat/"));
        }
    }
}
