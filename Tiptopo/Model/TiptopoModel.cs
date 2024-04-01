using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiptopo.Model
{
    public class TiptopoModel
    {
        public List<Line> lines { get; set; }
        public List<Measurement> measurements { get; set; }
        public List<MapText> texts { get; set; }
    }
}
