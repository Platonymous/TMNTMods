using ModLoader.Content;
using System.Collections.Generic;

namespace CustomSkins
{
    public class CustomSkin
    {
        public string Character { get; set;}
        public string Name { get; set;}

        public string MenuColor { get; set; } = "DarkSlateGray"; 
        
        public List<TexturePatch> Patches  { get; set;} = new List<TexturePatch>();

        internal IContentHelper Content { get; set; }
    }
}
