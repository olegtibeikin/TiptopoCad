using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiptopo.Model
{
    public class Line
    {
        public string id { get; set; }
        public string name { get; set; }
        public string note { get; set; }
        public long color { get; set; }
        public LineType type { get; set; }
        public LineForm form { get; set; }
        public bool reversed { get; set; }
        public List<Vertex> vertices { get; set; }
    }
}
