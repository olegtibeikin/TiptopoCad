using System;
using System.Threading;
using System.Windows;
using Tiptopo.Model;
using Tiptopo.ViewModel;

namespace Tiptopo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Utils utils = new Utils();

        public MainWindow(TiptopoModel tiptopo)
        {
            InitializeComponent();

            this.SetLanguageDictionary();

            DataContext = new ApplicationViewModel(this, tiptopo);
        }

        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "ru-RU":
                    dict.Source = new Uri("\\Tiptopo;component\\Resources\\StringResources.ru-RU.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("\\Tiptopo;component\\Resources\\StringResources.xaml", UriKind.Relative);
                    break;
            }
            
            this.Resources.MergedDictionaries.Add(dict);
        }
    }
}
