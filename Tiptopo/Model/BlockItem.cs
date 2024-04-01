using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tiptopo.Model
{
    public class BlockItem : INotifyPropertyChanged
    {
        private PointType pointType;
        private string code;
        private string imageSource;
        private string blockName;
        private string layerName = "0";
        private double scale = 1.0;

        public PointType PointType
        {
            get { return pointType; }
            set { 
                pointType = value;
                OnPropertyChanged("PointType");
            }
        }
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                OnPropertyChanged("Code");
            }
        }
        [JsonIgnore]
        public string ImageSource
        {
            get { return imageSource; }
            set
            {
                imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }
        public string BlockName
        {
            get { return blockName; }
            set
            {
                blockName = value;
                OnPropertyChanged("BlockName");
            }
        }
        public string LayerName
        {
            get { return layerName; }
            set
            {
                layerName = value;
                OnPropertyChanged("LayerName");
            }
        }
        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                OnPropertyChanged("Scale");
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
