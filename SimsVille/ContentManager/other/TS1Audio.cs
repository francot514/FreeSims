using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Content.Interfaces;
using FSO.Files.HIT;
using Microsoft.Xna.Framework.Audio;
using TSO.HIT.model;

namespace FSO.Client.ContentManager.other
{
    public class TS1Audio : IAudioProvider
    {
        public Dictionary<string, HITEventRegistration> Events => throw new NotImplementedException();

        public Dictionary<string, string> StationPaths => throw new NotImplementedException();

        public Dictionary<int, string> MusicModes => throw new NotImplementedException();

        public Hitlist GetHitlist(uint InstanceID, HITResourceGroup group)
        {
            throw new NotImplementedException();
        }

        public Patch GetPatch(uint id, HITResourceGroup group)
        {
            throw new NotImplementedException();
        }

        public SoundEffect GetSFX(Patch patch)
        {
            throw new NotImplementedException();
        }

        public Track GetTrack(uint value, uint fallback, HITResourceGroup group)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }
    }
}
