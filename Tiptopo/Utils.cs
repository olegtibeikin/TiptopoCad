using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tiptopo.Model;
using Line = Tiptopo.Model.Line;
using TiptopoLineType = Tiptopo.Model.LineType;
using WF = System.Windows.Forms;
using DColor = System.Drawing.Color;

#if NCAD
using HostMgd.ApplicationServices;
using Teigha.Colors;
using Teigha.DatabaseServices;
using HostMgd.EditorInput;
using Teigha.Geometry;
#else
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
#endif

namespace Tiptopo
{
    public class Utils
    {
        Editor editor;

        public Utils() 
        {
            editor = Application.DocumentManager.MdiActiveDocument.Editor;
        }

        public void WriteMessage(string message)
        {
            editor.WriteMessage(message);
        }

        public List<string> GetLineTypeList()
        {
            List<string> items = null;

            DoActionWithinTransaction((tr, db) => 
            {
                items = ((LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead))
                    .Cast<ObjectId>()
                    .Select(id => (LinetypeTableRecord)tr.GetObject(id, OpenMode.ForRead))
                    .Select(lt => lt.Name)
                    .ToList();
            });

            return items;
        }

        public List<string> GetLayerList()
        {
            List<string> items = null;

            DoActionWithinTransaction((tr, db) =>
            {
                items = ((LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead))
                    .Cast<ObjectId>()
                    .Select(id => (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead))
                    .Select(lt => lt.Name)
                    .ToList();
            });

            return items;
        }

        public List<string> GetBlockNameList()
        {
            List<string> items = null;

            DoActionWithinTransaction((tr, db) =>
            {
                items = ((BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead))
                    .Cast<ObjectId>()
                    .Select(id => (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead))
                    .Select(btr => btr.Name)
                    .Where(name => !name.StartsWith("*"))
                    .ToList();
            });

            return items;
        }

        public List<LineItem> GetLineItems(List<Line> lines)
        {
            var distinctLines = lines.GroupBy(l => new { l.color, l.type })
                                 .Select(g => g.First())
                                 .ToList();
            return distinctLines.Select(line =>
            {
                var imageSource = Enum.GetName(typeof(TiptopoLineType), line.type) + "PathStyle";
                var colorRGB = $"#{(line.color & 0xFFFFFF):X6}";
                return new LineItem { 
                    LineType = line.type,
                    TiptopoColor  = colorRGB,
                    AcadColor = GetAcadColorFromHexRGB(colorRGB)
                    
            };
            }
            ).ToList();
        }

        public EntityProperties PickEntity()
        {
            EntityProperties properties = null;
            DoActionWithinTransaction((tr, db) =>
            {
                PromptSelectionOptions options = new PromptSelectionOptions
                {
                    SingleOnly = true,
                    SinglePickInSpace = true
                };

                var result = editor.GetSelection(options);
                if (result.Status == PromptStatus.OK)
                {
                    Entity entity = (Entity)tr.GetObject(result.Value.OfType<SelectedObject>().First().ObjectId, OpenMode.ForRead);
                    properties = new EntityProperties(entity.Linetype, entity.Layer, entity.LinetypeScale, entity.Color);
                }
            });

            return properties;
        }

        public string GetHexRGBFromAcadColor(Color color, string layerName)
        {
            if (color.ColorIndex == 256)
            {
                DoActionWithinTransaction((tr, db) =>
                {
                    var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    if (layerTable.Has(layerName))
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(layerTable[layerName], OpenMode.ForRead);
                        color = ltr.Color;
                    }
                });
            }
            return "#" + color.ColorValue.R.ToString("X2") + color.ColorValue.G.ToString("X2") + color.ColorValue.B.ToString("X2");
        }

        public Color GetAcadColorFromHexRGB(string hexRgbString)
        {
            hexRgbString = hexRgbString.Replace("#", "");

            int r = int.Parse(hexRgbString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hexRgbString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hexRgbString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }

        public EntityProperties PickBlock()
        {
            EntityProperties entityProperties = null;

            DoActionWithinTransaction((tr, db) =>
            {
                PromptSelectionOptions options = new PromptSelectionOptions
                {
                    SingleOnly = true,
                    SinglePickInSpace = true
                };
                SelectionFilter filter = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, "INSERT") });

                while (true)
                {

                    var result = editor.GetSelection(options, filter);

                    if (result.Status == PromptStatus.OK)
                    {
                        BlockReference blockReference = (BlockReference)tr.GetObject(result.Value.OfType<SelectedObject>().First().ObjectId, OpenMode.ForRead);
                        entityProperties = new EntityProperties(blockReference.Name, blockReference.Layer, blockReference.ScaleFactors.X, blockReference.Color);
                        break;
                    }
                    else if (result.Status == PromptStatus.Error)
                    {
                        Application.ShowAlertDialog("Необходимо выбрать блок!");
                    }
                    else
                    {
                        break;
                    }
                }
            });

            return entityProperties;
        }

        public List<BlockItem> GetBlockItems(List<Measurement> measurements)
        {
            string path = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            string dllDirectoryPath = Path.GetDirectoryName(path);
            string imgDirectoryPath = Path.Combine(dllDirectoryPath, "img");

            return measurements.Select(measurement => new { measurement.type, measurement.code })
                .Distinct()
                .Where(pair => pair.type != PointType.Point)
            .Select(pair =>
            {
                var imageSource = Path.Combine(imgDirectoryPath, Enum.GetName(typeof(PointType), pair.type) + ".png");
                return new BlockItem { PointType = pair.type, Code = pair.code, ImageSource = imageSource };
            })
            .ToList();
        }

        public ItemsModel GetItems()
        {
            ItemsModel items = null;
            WF.OpenFileDialog fileDialog = new WF.OpenFileDialog();
            fileDialog.Filter = "tconfig files (*.tconfig)|*.tconfig|All files (*.*)|*.*";
            var result = fileDialog.ShowDialog();
            if (result == WF.DialogResult.OK)
            {
                try
                {
                    items = JsonConvert.DeserializeObject<ItemsModel>(File.ReadAllText(fileDialog.FileName));
                    return items;
                }
                catch
                {
                    Application.ShowAlertDialog("Ошибка чтения файла!");
                }
            }
            return null;
        }

        public void SaveItems(ItemsModel items)
        {
            WF.SaveFileDialog fileDialog = new WF.SaveFileDialog();
            fileDialog.Filter = "tconfig files (*.tconfig)|*.tconfig|All files (*.*)|*.*";
            var result = fileDialog.ShowDialog();
            if (result == WF.DialogResult.OK)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(items);
                    File.WriteAllText(fileDialog.FileName, json);
                }
                catch
                {
                    Application.ShowAlertDialog("Ошибка!");
                }
            }
        }

        public void DrawAll(TiptopoModel tiptopo, List<LineItem> lineItems, List<BlockItem> blockItems)
        {
            if (tiptopo == null) { return; }
            tiptopo.measurements.ForEach(measurement => { 
                Point3d point3D = measurement.position.toPoint3d();
                var pointColor = measurement.isMeasured ? Color.FromColor(DColor.FromArgb((int)measurement.color)) : Color.FromColor(DColor.Red);
                AddPoint(point3D, measurement.name, measurement.note, pointColor);
                var blockItem = blockItems.FirstOrDefault(x => x.PointType == measurement.type && x.Code == measurement.code);
                
                if(blockItem != null)
                {
                    AddBlock(measurement, blockItem);
                }
            });
            tiptopo.lines.ForEach(line =>
            {
                var lineItem = lineItems.FirstOrDefault(x => x.LineType == line.type && x.TiptopoColor == $"#{(line.color & 0xFFFFFF):X6}") ?? new LineItem();
                AddLine(line, tiptopo.measurements, lineItem);
            });
            tiptopo.texts.ForEach(mapText => 
            {
                AddMapText(mapText);
            });
        }

        private void AddLine(Line line, List<Measurement> measurements, LineItem lineItem)
        {
            var vertices = line.vertices;
            vertices.Sort((x, y) => x.index.CompareTo(y.index));

            var points = vertices.Select(x => measurements.FirstOrDefault(m => m.id == x.measurementId))
                .Where(measurement => measurement != null)
                .Select(measurement => new Point2d(measurement.position.x, measurement.position.y))
                .ToList();
            if(line.reversed) points.Reverse();

            if (points.Count <= 1) return;

            switch(line.form)
            {
                case LineForm.Circle:
                    {
                        AddCircle(line, points, lineItem);
                        return;
                    }
                case LineForm.Spline:
                    {
                        AddSpline(line, points, lineItem);
                        return;
                    }
                case LineForm.Arc:
                    { 
                        AddArc(line, points, lineItem);
                        return;
                    }
                default:
                    break;
            }

            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = tr.GetObject(db.BlockTableId,
                    OpenMode.ForRead) as BlockTable;
                BlockTableRecord tableRecord = tr.GetObject(blockTable[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline polyline = new Polyline())
                {
                    bool closed = points.First() == points.Last();
                    foreach (Point2d point in points)
                    {
                        if (points.IndexOf(point) == points.Count - 1 && closed)
                        {
                            break;
                        }
                        polyline.AddVertexAt(points.IndexOf(point), point, 0, 0, 0);
                    }
                    polyline.Closed = closed;
                    polyline.Layer = lineItem.LayerName;
                    polyline.LinetypeScale = lineItem.LineTypeScale;
                    polyline.Linetype = lineItem.LineTypeName;
                    polyline.Plinegen = true;
                    polyline.Color = lineItem.AcadColor;
                    var notePosition = new Point3d(points.First().X, points.First().Y, 0.0);
                    AddNote(notePosition, line.note);

                    tableRecord.AppendEntity(polyline);
                    tr.AddNewlyCreatedDBObject(polyline, true);
                }
            });
        }

        private void AddSpline(Line line, List<Point2d> points, LineItem lineItem)
        {

            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = tr.GetObject(db.BlockTableId,
                    OpenMode.ForRead) as BlockTable;
                BlockTableRecord tableRecord = tr.GetObject(blockTable[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                var point3dCollection = new Point3dCollection();
                foreach (Point2d point in points)
                {
                    point3dCollection.Add(ToPoint3D(point));
                }

                using (Spline spline = new Spline(point3dCollection, 3, 0))
                {
                    spline.Layer = lineItem.LayerName;
                    spline.LinetypeScale = lineItem.LineTypeScale;
                    spline.Linetype = lineItem.LineTypeName;
                    spline.Color = lineItem.AcadColor;

                    var notePosition = new Point3d(points.First().X, points.First().Y, 0.0);
                    AddNote(notePosition, line.note);

                    tableRecord.AppendEntity(spline);
                    tr.AddNewlyCreatedDBObject(spline, true);
                }
            });
        }

        private Point3d ToPoint3D(Point2d point2D)
        {
            return new Point3d(point2D.X, point2D.Y, 0.0);
        }

        private void AddArc(Line line, List<Point2d> points, LineItem lineItem)
        {
            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = tr.GetObject(db.BlockTableId,
                    OpenMode.ForRead) as BlockTable;
                BlockTableRecord tableRecord = tr.GetObject(blockTable[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                CircularArc3d cArc = new CircularArc3d(ToPoint3D(points[0]), ToPoint3D(points[1]), ToPoint3D(points[2]));

                var center = cArc.Center; ;
                var normal = cArc.Normal;
                var vector = cArc.ReferenceVector;
                var plane = new Plane(center, normal);
                var angle = vector.AngleOnPlane(plane);
                var radius = cArc.Radius;
                var startAngle = cArc.StartAngle;
                var endAngle = cArc.EndAngle;

                using (Arc arc = new Arc(center, normal, radius, startAngle + angle, endAngle + angle))
                {
                    arc.Layer = lineItem.LayerName;
                    arc.LinetypeScale = lineItem.LineTypeScale;
                    arc.Linetype = lineItem.LineTypeName;
                    arc.Color = lineItem.AcadColor;
                    var notePosition = new Point3d(points.First().X, points.First().Y, 0.0);
                    AddNote(notePosition, line.note);

                    tableRecord.AppendEntity(arc);
                    tr.AddNewlyCreatedDBObject(arc, true);
                }
            });
        }

        private void AddCircle(Line line, List<Point2d> points, LineItem lineItem)
        {
            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = tr.GetObject(db.BlockTableId,
                    OpenMode.ForRead) as BlockTable;
                BlockTableRecord tableRecord = tr.GetObject(blockTable[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;
                CircularArc3d cArc = new CircularArc3d(ToPoint3D(points[0]), ToPoint3D(points[1]), ToPoint3D(points[2]));

                var center = cArc.Center; ;
                var normal = cArc.Normal;
                var radius = cArc.Radius;

                using (Circle circle = new Circle(center, normal, radius))
                {
                    circle.Layer = lineItem.LayerName;
                    circle.LinetypeScale = lineItem.LineTypeScale;
                    circle.Linetype = lineItem.LineTypeName;
                    circle.Color = lineItem.AcadColor;
                    var notePosition = new Point3d(points.First().X, points.First().Y, 0.0);
                    AddNote(notePosition, line.note);

                    tableRecord.AppendEntity(circle);
                    tr.AddNewlyCreatedDBObject(circle, true);
                }
            });
        }

        private void AddBlock(Measurement measurement, BlockItem blockItem)
        {
            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

                if (blockTable.Has(blockItem.BlockName))
                {
                    ObjectId blockId = blockTable[blockItem.BlockName];
                    if (blockId != null)
                    {
                        Point3d position = measurement.position.toPoint3dZeroHeight();
                        BlockTableRecord modelSpace =
                            (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                        using (BlockReference blockReference = new BlockReference(position, blockId))
                        {
                            blockReference.Layer = blockItem.LayerName;
                            blockReference.ScaleFactors = new Scale3d(blockItem.Scale, blockItem.Scale, blockItem.Scale);

                            blockReference.Color = Color.FromColor(DColor.FromArgb((int)measurement.color));

                            modelSpace.AppendEntity(blockReference);
                            tr.AddNewlyCreatedDBObject(blockReference, true);

                            if(blockItem.BlockName == "Колодец!№%")
                            {
                                BlockTableRecord blockDef = blockTable["Колодец!№%"].GetObject(OpenMode.ForRead) as BlockTableRecord;
                                foreach (ObjectId id in blockDef)
                                {
                                    DBObject obj = id.GetObject(OpenMode.ForRead);
                                    AttributeDefinition attDef = obj as AttributeDefinition;
                                    if ((attDef != null) && (!attDef.Constant))
                                    {
                                        using (AttributeReference attRef = new AttributeReference())
                                        {
                                            attRef.SetAttributeFromBlock(attDef, blockReference.BlockTransform);
                                            attRef.TextString = string.IsNullOrEmpty(blockItem.Code) ? "Л" : blockItem.Code;
                                            //Add the AttributeReference to the BlockReference
                                            blockReference.AttributeCollection.AppendAttribute(attRef);
                                            tr.AddNewlyCreatedDBObject(attRef, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private void AddPoint(Point3d point3D, string name, string note, Color color)
        {
            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace =
                    (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                LayerTable layerTable = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                // draw points
                using (DBPoint point = new DBPoint(point3D))
                {
                    var layerName = "Point";
                    if (!layerTable.Has(layerName))
                    {
                        LayerTableRecord layerRecord = new LayerTableRecord();
                        layerRecord.Name = layerName;
                        layerTable.UpgradeOpen();
                        layerTable.Add(layerRecord);
                        tr.AddNewlyCreatedDBObject(layerRecord, true);
                    }
                    point.Layer = layerName;
                    point.Color = color;
                    modelSpace.AppendEntity(point);
                    tr.AddNewlyCreatedDBObject(point, true);
                }

                // draw point names
                if (!string.IsNullOrEmpty(name))
                {
                    using (DBText text = new DBText())
                    {
                        var layerName = "Point name";
                        if (!layerTable.Has(layerName))
                        {
                            LayerTableRecord layerRecord = new LayerTableRecord();
                            layerRecord.Name = layerName;
                            layerTable.UpgradeOpen();
                            layerTable.Add(layerRecord);
                            tr.AddNewlyCreatedDBObject(layerRecord, true);
                        }
                        text.Layer = layerName;

                        text.SetDatabaseDefaults();
                        text.TextString = name;
                        text.Position = new Point3d(point3D.X, point3D.Y, 0.0);
                        text.Height = 1.0;
                        modelSpace.AppendEntity(text);
                        tr.AddNewlyCreatedDBObject(text, true);
                    }
                }

                // draw note
                AddNote(new Point3d(point3D.X, point3D.Y, 0.0), note);

                // set point format
                db.Pdmode = 35;
                db.Pdsize = 0.5;
            });
        }

        private void AddMapText(MapText mapText)
        {
            if (string.IsNullOrEmpty(mapText.text)) { return ; }

            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace =
                    (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                LayerTable layerTable = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;

                using (DBText text = new DBText())
                {
                    var layerName = "Text";
                    if (!layerTable.Has(layerName))
                    {
                        LayerTableRecord layerRecord = new LayerTableRecord();
                        layerRecord.Name = layerName;
                        layerTable.UpgradeOpen();
                        layerTable.Add(layerRecord);
                        tr.AddNewlyCreatedDBObject(layerRecord, true);
                    }
                    text.Layer = layerName;

                    text.SetDatabaseDefaults();
                    text.TextString = mapText.text;
                    text.Position = new Point3d(mapText.x, mapText.y, 0.0);
                    text.Height = 1.0;
                    text.Color = Color.FromColor(DColor.FromArgb((int)mapText.color));
                    modelSpace.AppendEntity(text);
                    tr.AddNewlyCreatedDBObject(text, true);
                }
            });
        }

        private void AddNote(Point3d position, string note)
        {
            if (string.IsNullOrEmpty(note)) return;

            DoActionWithinTransaction((tr, db) =>
            {
                BlockTable blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace =
                    (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                LayerTable layerTable = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                if (!string.IsNullOrEmpty(note))
                {
                    using (DBText text = new DBText())
                    {
                        var layerName = "Note";
                        if (!layerTable.Has(layerName))
                        {
                            LayerTableRecord layerRecord = new LayerTableRecord();
                            layerRecord.Name = layerName;
                            layerTable.UpgradeOpen();
                            layerTable.Add(layerRecord);
                            tr.AddNewlyCreatedDBObject(layerRecord, true);
                        }
                        text.Layer = layerName;

                        text.SetDatabaseDefaults();
                        text.TextString = note;
                        text.Position = position;
                        text.Height = 1.0;
                        text.ColorIndex = 1;
                        modelSpace.AppendEntity(text);
                        tr.AddNewlyCreatedDBObject(text, true);
                    }
                }
            });
        }

        private void DoActionWithinTransaction(Action<Transaction, Database> action)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Database db = doc.Database;

            using (DocumentLock docloc = doc.LockDocument())
            {
                using (Transaction trans = doc.TransactionManager.StartTransaction())
                {
                    action(trans, db);
                    trans.Commit();
                }
            }
        }
    }
}
