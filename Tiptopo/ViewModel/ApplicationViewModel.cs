using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Tiptopo.Model;
#if NCAD
using HostMgd.Windows;
#else
using Autodesk.AutoCAD.Windows;
#endif

namespace Tiptopo.ViewModel
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        private Utils utils = new Utils();
        private LineItem selectedLine;
        private BlockItem selectedBlock;
        private MainWindow mainWindow;
        private TiptopoModel tiptopo;
        public ObservableCollection<LineItem> Lines { get; set; }
        public ObservableCollection<BlockItem> Blocks { get; set; }

        public List<string> LineTypeItems { get; set; }
        public List<string> Layers { get; set; }
        public List<string> BlockNames { get; set; }

        public ApplicationViewModel(MainWindow mainWindow, TiptopoModel tiptopo)
        {
            this.mainWindow = mainWindow;
            this.tiptopo = tiptopo;

            Lines = new ObservableCollection<LineItem>(utils.GetLineItems(tiptopo.lines));

            if (Lines.Any())
            {
                SelectedLine = Lines.First();
            }

            Blocks = new ObservableCollection<BlockItem>(utils.GetBlockItems(tiptopo.measurements));
            if (Blocks.Any())
            {
                SelectedBlock = Blocks.First();
            }

            BlockNames = utils.GetBlockNameList();
            BlockNames.Sort();

            LineTypeItems = utils.GetLineTypeList();

            Layers = utils.GetLayerList();
            Layers.Sort();
        }

        private RelayCommand pickEntityCommand;
        public RelayCommand PickEntityCommand
        {
            get
            {
                return pickEntityCommand ??
                    (pickEntityCommand = new RelayCommand(obj =>
                    {
                        mainWindow.Hide();
                        var prop = utils.PickEntity();
                        if(SelectedLine != null && prop != null)
                        {
                            SelectedLine.LayerName = prop.LayerName;
                            SelectedLine.LineTypeName = prop.Name;
                            SelectedLine.LineTypeScale = prop.Scale;
                            SelectedLine.AcadColor = prop.Color;
                        }
                        mainWindow.Show();
                    }));
            }
        }

        private RelayCommand selectColorDialogCommand;
        public RelayCommand SelectColorDialogCommand
        {
            get
            {
                return selectColorDialogCommand ??
                    (selectColorDialogCommand = new RelayCommand(obj =>
                    {
                        ColorDialog cd = new ColorDialog();

                        System.Windows.Forms.DialogResult dr =

                          cd.ShowDialog();

                        if (dr == System.Windows.Forms.DialogResult.OK)

                        {
                            SelectedLine.AcadColor = cd.Color;

                        }
                    }));
            }
        }

        private RelayCommand pickBlockCommand;
        public RelayCommand PickBlockCommand
        {
            get
            {
                return pickBlockCommand ??
                    (pickBlockCommand = new RelayCommand(obj =>
                    {
                        mainWindow.Hide();
                        var prop = utils.PickBlock(mainWindow);
                        if(SelectedBlock != null && prop != null)
                        {
                            SelectedBlock.LayerName = prop.LayerName;
                            SelectedBlock.Scale = prop.Scale;
                            SelectedBlock.BlockName = prop.Name;
                        }
                        mainWindow.Show();
                    }));
            }
        }

        private RelayCommand getItemsCommand;
        public RelayCommand GetItemsCommand
        {
            get
            {
                return getItemsCommand ??
                    (getItemsCommand = new RelayCommand(obj =>
                    {
                        var items = utils.GetItems(mainWindow);
                        if (items == null) return;

                        var lineList = Lines.ToList();
                        lineList.ForEach(line =>
                        {
                            var configLine = items.LineItems.FirstOrDefault(x => x.LineType == line.LineType && x.TiptopoColor == line.TiptopoColor);
                            if(configLine != null)
                            {
                                line.LayerName = configLine.LayerName;
                                line.LineTypeName = configLine.LineTypeName;
                                line.LineTypeScale= configLine.LineTypeScale;
                                line.AcadColor = configLine.AcadColor;
                            }

                        });
                        var blockList = Blocks.ToList();
                        blockList.ForEach(block =>
                        {
                            var configBlock = items.BlockItems.FirstOrDefault(x => x.PointType == block.PointType && x.Code == block.Code);
                            if(configBlock != null) 
                            {
                                block.BlockName = configBlock.BlockName;
                                block.LayerName = configBlock.LayerName;
                                block.Scale = configBlock.Scale;
                            }
                        });
                    }));
            }
        }

        private RelayCommand drawAllCommand;
        public RelayCommand DrawAllCommand
        {
            get
            {
                return drawAllCommand ??
                    (drawAllCommand = new RelayCommand(obj => {
                        try
                        {
                            utils.DrawAll(tiptopo, Lines.ToList(), Blocks.ToList());
                        }
                        catch (Exception e)
                        {
                            utils.WriteMessage(e.Message + "\n" + e.StackTrace);
                        }
                        
                        mainWindow.Close();
                    }));
            }
        }

        private RelayCommand saveItemsCommand;
        public RelayCommand SaveItemsCommand
        {
            get
            {
                return saveItemsCommand ??
                    (saveItemsCommand = new RelayCommand(obj =>
                    {
                        utils.SaveItems(new ItemsModel { LineItems = Lines.ToList(), BlockItems = Blocks.ToList() });
                    }));
            }
        }

        public LineItem SelectedLine
        {
            get { return selectedLine; }
            set
            {
                selectedLine = value;
                OnPropertyChanged("SelectedLine");
            }
        }

        public BlockItem SelectedBlock
        {
            get { return selectedBlock; }
            set
            {
                selectedBlock = value;
                OnPropertyChanged("SelectedBlock");
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
