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
using System.Windows.Shapes;

namespace Stroika
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        DbService db = new DbService();
        List<OrderItemModel> currentItems = new List<OrderItemModel>();
        private int? _orderId = null;

        public OrderWindow()
        {
            InitializeComponent();
            HeaderText.Text = "Новая заявка";
            this.Title = "Новая заявка";

            PartnerCombo.ItemsSource = db.GetAllPartners();
            ProductCombo.ItemsSource = db.GetAllProducts();
        }

        public OrderWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            HeaderText.Text = $"Редактирование заявки №{orderId}";
            this.Title = $"Заявка №{orderId}";

            PartnerCombo.ItemsSource = db.GetAllPartners();
            ProductCombo.ItemsSource = db.GetAllProducts();

            LoadOrderData();
        }

        private void LoadOrderData()
        {
            if (!_orderId.HasValue) return;

            int partnerId = db.GetPartnerIdByOrderId(_orderId.Value);
            PartnerCombo.SelectedValue = partnerId;

            currentItems = db.GetOrderItems(_orderId.Value);
            RefreshGrid();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукцию из списка!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = ProductCombo.SelectedItem as ProductModel;
            if (!int.TryParse(QtyBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество (целое положительное число)!",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existing = currentItems.FirstOrDefault(x => x.ProductId == product.Id);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                currentItems.Add(new OrderItemModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.MinPartnerPrice,
                    Quantity = qty
                });
            }

            RefreshGrid();
            QtyBox.Text = "1";
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button?.Tag as OrderItemModel;
            if (item != null)
            {
                currentItems.Remove(item);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            ItemsGrid.ItemsSource = null;
            ItemsGrid.ItemsSource = currentItems;
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = currentItems.Sum(x => x.Total);
            TotalText.Text = total.ToString("N2");
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (PartnerCombo.SelectedValue == null)
            {
                MessageBox.Show("Выберите партнера!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (currentItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну продукцию в заявку!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int partnerId = (int)PartnerCombo.SelectedValue;

                if (_orderId.HasValue)
                {
                    db.UpdateOrder(_orderId.Value, partnerId, currentItems);
                }
                else
                {
                    db.CreateOrder(partnerId, currentItems);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении заявки:\n" + ex.Message,
                    "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
