using Stroika.Models;
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

namespace Stroika
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DbService db = new DbService();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            OrdersList.ItemsSource = db.GetOrders();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new OrderWindow();
            if (orderWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void OrdersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedOrder = OrdersList.SelectedItem as OrderViewModel;
            if (selectedOrder != null)
            {
                var orderWindow = new OrderWindow(selectedOrder.Id);
                if (orderWindow.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }
    }
}
