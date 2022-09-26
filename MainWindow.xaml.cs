using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IngestManager.Entities;

namespace IngestManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Model localModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            localModel = new Model();
            DataContext = localModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var order = new Order();
            localModel.Orders.Add(order);
        }
    }
}
