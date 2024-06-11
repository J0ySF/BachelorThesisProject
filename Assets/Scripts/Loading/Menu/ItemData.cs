using System.IO;

namespace Loading.Menu
{
    /// <summary>
    /// Item data for a loading menu entry.
    /// </summary>
    public sealed class ItemData : UI.ScrollMenu.ItemData
    {
        public string FileName { get; }
        public override string Label { get; }
        
        public ItemData(string fileName)
        {
            FileName = fileName;
            Label = Path.GetFileNameWithoutExtension(fileName);
        }
    }
}
