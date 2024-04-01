﻿using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiptopo.Model
{
    public class Measurement
    {
        public string id { get; set; }

        public string name { get; set; }
        public string note { get; set; }
        public bool isMeasured { get; set; }
        public PointType type { get; set; }
        public long color { get; set; }
        public Position position { get; set; }
        public string code { get; set; }

        public override string ToString()
        {
            return $"id: {id}, name: {name}, note: {note}, isMeasured: {isMeasured}, type: {type}, color: {color}, position: {position}, code: {code}";
        }
    }
}