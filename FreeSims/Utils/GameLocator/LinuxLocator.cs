using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FSO.Client.Utils.GameLocator
{
    public enum TS1InstallationType
    {
        Portable,
        Steam,
        Registry,
        Wine,
        Unknown
    }
    public class LinuxLocator : ILocator
    {
        public string FindTheSimsOnline()
        {
            return "game/TSOClient/";
        }

        public string FindTheSimsComplete()
        {
            return "game/The Sims/";
        }

        public List<(string description, string path, TS1InstallationType type)> GetAllTheSims1Installations()
        {
            var installations = new List<(string, string, TS1InstallationType)>();

            // Check relative path (portable install)
            string localDir = @"../The Sims/";
            if (File.Exists(Path.Combine(localDir, "GameData", "Behavior.iff")))
            {
                installations.Add(("Portable Install (Relative Directory)", localDir, TS1InstallationType.Portable));
            }

            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Check Steam (Proton/Steam Play)
            var steamPath = Path.Combine(homeDir, ".steam/steam/steamapps/common/The Sims/");
            if (File.Exists(Path.Combine(steamPath, "GameData", "Behavior.iff")))
            {
                installations.Add(("Steam - The Sims: Legacy Collection (Proton)", steamPath, TS1InstallationType.Steam));
            }

            // Check Wine (default prefix)
            var winePath = Path.Combine(homeDir, ".wine/drive_c/Program Files/Maxis/The Sims/");
            if (File.Exists(Path.Combine(winePath, "GameData", "Behavior.iff")))
            {
                installations.Add(("Wine - Default Prefix", winePath, TS1InstallationType.Wine));
            }

            // Check Wine (x86 prefix)
            var winePath32 = Path.Combine(homeDir, ".wine/drive_c/Program Files (x86)/Maxis/The Sims/");
            if (File.Exists(Path.Combine(winePath32, "GameData", "Behavior.iff")))
            {
                installations.Add(("Wine - Default Prefix (x86)", winePath32, TS1InstallationType.Wine));
            }

            // Check fallback location
            if (File.Exists(Path.Combine("game1/", "GameData", "Behavior.iff")))
            {
                installations.Add(("Fallback Location", "game1/", TS1InstallationType.Unknown));
            }

            return installations;
        }

    }
}
