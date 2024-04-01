using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
