using Microsoft.Xna.Framework;
namespace CustomSkins
{
    public class TexturePatch
    {
        public string Asset { get; set; }
        public string Patch { get; set; }

        public string PatchType { get; set; }

        internal Color[] PatchData { get; set; }

        internal bool ShouldMerge => PatchType.Equals("Merge",System.StringComparison.InvariantCultureIgnoreCase);
    }
}
