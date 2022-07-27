using ModLoader.Content;
using Paris.Engine;
using Paris.Engine.Audio;
using Paris.Engine.Context;
using Paris.Engine.Data;
using Paris.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomSkins
{
    internal class SoundPatch
    {
        private SFX Sound { get; set; }

        private int ID { get; set; }

        private SFX OriginalSound { get; set; }

        private List<SFX> SoundList => (List<SFX>) typeof(AudioManager).GetField("_sounds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(AudioManager.Singleton);

        private byte[] Audio { get; set; }

        private bool initialized = false;

        private string SoundId { get; set; }

        public SoundPatch(string id, byte[] sound)
        {
            Audio = sound;
            SoundId = id;
        }

        public void Init()
        {
            if (initialized)
                return;
            try
            {
                SoundSettings soundSettings = ContextManager.Singleton.LoadContent<SoundSettings>(EngineSettings.Singleton.SoundsSettings, true);
                SoundSettings.SoundInfo info = soundSettings.Sounds.FirstOrDefault(s => string.Equals(SoundId, s.SoundID, StringComparison.OrdinalIgnoreCase));
                List<SFXChannel> channels = (List<SFXChannel>)typeof(AudioManager).GetField("_soundChannels", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(AudioManager.Singleton);
                List<SFXChannel> sfxChannelList = new List<SFXChannel>();

                foreach (string playbackChannel in info.PlaybackChannels)
                {
                    string channelName = playbackChannel;
                    SFXChannel sfxChannel = channels.Find(x => x.Name == channelName);
                    if (sfxChannel != null)
                        sfxChannelList.Add(sfxChannel);
                }

                ID = AudioManager.Singleton.GetSFXIDByName(info.SoundID);
                OriginalSound = SoundList[ID];
                Sound = new SFX(info.SoundID, Audio, info.Volume / 100f, info.PoolSize, info.LoopType, sfxChannelList.ToArray(), info.CutOffType, new Range(info.PitchMin, info.PitchMax), info.IsVO, info.Cooldown);
            }
            catch(Exception ex)
            {
                
            }
            initialized = true;
        }

        public void Apply()
        {
            SoundList[ID] = Sound;
        }

        public void Reset()
        {
            SoundList[ID] = OriginalSound;
        }
       
    }
}
