using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using IngestManager.Models.TelegramBot;

namespace IngestManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel ViewModel { get; } = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CreateEmptyOrder();
            //await ViewModel.Message();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ViewModel.CompleteOrderAsync(ViewModel.Model.CurrentOrder);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ViewModel.Model.CurrentOrder.Status = Models.OrderStatus.Обрабатывается;
        }
    }
}
