using System.Windows.Media.Imaging;

namespace Tiptopo.Model
{
    public class LineTypeItem

    {
        public LineTypeItem(string name, BitmapImage bitmap)
        {
            Name = name;
            BitmapImage = bitmap;
        }
        public string Name { get; }
        public BitmapImage BitmapImage { get; }
    }
}
