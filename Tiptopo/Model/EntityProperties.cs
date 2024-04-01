#if NCAD
using Teigha.Colors;
#else
using Autodesk.AutoCAD.Colors;
#endif

namespace Tiptopo.Model
{
    public class EntityProperties
    {
        public EntityProperties(string name, string layerName, double scale, Color color) 
        {
            Name = name;
            LayerName = layerName;
            Scale = scale;
            Color = color;
        }
        public string Name { get; }
        public string LayerName { get; }
        public double Scale { get; }
        public Color Color { get; }
    }
}
