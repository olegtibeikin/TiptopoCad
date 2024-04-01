using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Internal;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tiptopo.Model
{

    public class LineItem : INotifyPropertyChanged
    {
        private LineType lineType;
        private string tiptopoColor;
        private Color acadColor;
        private string acadColorHexRGB;
        private string acadColorName;
        private string lineTypeName = "ByLayer";
        private string layerName = "0";
        private double lineTypeScale = 1.0;
        private Utils utils = new Utils();

        public LineType LineType
        {
            get { return lineType; }
            set { 
                lineType = value;
                OnPropertyChanged("LineType");
            }
        }
        public string TiptopoColor
        {
            get { return tiptopoColor; }
            set
            {
                tiptopoColor = value;
                OnPropertyChanged("TiptopoColor");
                
            }
        }
        public Color AcadColor
        {
            get
            {
                return acadColor;
            }
            set
            {
                acadColor = value;
                acadColorHexRGB = utils.GetHexRGBFromAcadColor(value, layerName);
                acadColorName = value.ToString();
                OnPropertyChanged("AcadColorHexRGB");
                OnPropertyChanged("AcadColorName");
            }
        }
        public string AcadColorHexRGB
        {
            get { return acadColorHexRGB; }
        }
        public string AcadColorName
        {
            get { return acadColorName; }
        }
        public string LineTypeName
        {
            get { return lineTypeName; }
            set
            {
                lineTypeName = value;
                OnPropertyChanged("LineTypeName");
            }
        }
        public string LayerName
        {
            get { return layerName; }
            set
            {
                layerName = value;
                OnPropertyChanged("LayerName");
                acadColorHexRGB = utils.GetHexRGBFromAcadColor(acadColor, layerName);
                OnPropertyChanged("AcadColorHexRGB");
            }
        }
        public double LineTypeScale
        {
            get { return lineTypeScale; }
            set
            {
                lineTypeScale = value;
                OnPropertyChanged("LineTypeScale");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
