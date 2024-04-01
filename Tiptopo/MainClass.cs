using System.Reflection;
using System.IO;
using Autodesk.AutoCAD.Runtime;
using WF = System.Windows.Forms;
using AS = Autodesk.AutoCAD.ApplicationServices;
using Newtonsoft.Json;
using Tiptopo.Model;
using System.Threading;
using System.Windows;
using System;

namespace Tiptopo
{

    public class MainClass
    {
        [CommandMethod("Tiptopo")]
        public void RunTiptopo()
        {
            Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MaterialDesignThemes.Wpf.dll"));
            Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MaterialDesignColors.dll"));
            Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Microsoft.Xaml.Behaviors.dll"));

            WF.OpenFileDialog fileDialog = new WF.OpenFileDialog();
            var result = fileDialog.ShowDialog();

            if (result == WF.DialogResult.OK)
            {
                TiptopoModel tiptopo = null;
                try
                {
                    tiptopo = JsonConvert.DeserializeObject<TiptopoModel>(File.ReadAllText(fileDialog.FileName));
                }
                catch
                {
                    switch (Thread.CurrentThread.CurrentCulture.ToString())
                    {
                        case "ru-RU":
                            AS.Application.ShowAlertDialog("Ошибка чтения файла!");
                            break;
                        default:
                            AS.Application.ShowAlertDialog("Error reading file!");
                            break;
                    }
                    
                    return;
                }
                if(tiptopo != null)
                {
                    try
                    {
                        var mainWindow = new MainWindow(tiptopo);
                        AS.Application.ShowModalWindow(mainWindow);
                    }
                    catch(System.Exception ex) {
                        AS.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(ex.Message);
                    }
                }
            }
        }
    }
}
