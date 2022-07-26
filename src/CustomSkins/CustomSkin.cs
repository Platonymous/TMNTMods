using ModLoader.Content;
using System.Collections.Generic;

namespace CustomSkins
{
    public class CustomSkin
    {
        internal string CharId => Character == "Leonardo" ? "Leo" : Character;
        public string Character { get; set;}
        public string Name { get; set;}

        public string MenuColor { get; set; } = "DarkSlateGray"; 
        
        public List<TexturePatch> Patches  { get; set;} = new List<TexturePatch>();

        internal List<SoundPatch> AudioPatches { get; set;} = new List<SoundPatch>();

        internal IContentHelper Content { get; set; }
    }
}
