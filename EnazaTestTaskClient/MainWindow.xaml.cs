using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace EnazaTestTaskClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class  MainWindow : Window
    {
        IMessageService messageService;

        public MainWindow()
        {
            InitializeComponent();
            
            messageService = new MessageService("http://localhost:52175");

            messageService.OnMessage((message) =>
            {
                Dispatcher?.Invoke(() =>
                {
                    ReceiveTextBox.Text += $"{message}\r\n";
                });
            });

            messageService.OnDisconnect(async (error) =>
            {
                Dispatcher?.Invoke(() =>
                {
                    SendButton.IsEnabled = false;
                    GetStoreButton.IsEnabled = false;
                });
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await messageService.ConnectAsync();
            });

            messageService.OnConnect(() =>
            {
                Dispatcher?.Invoke(() =>
                {
                    SendButton.IsEnabled = true;
                    GetStoreButton.IsEnabled = true;
                });
            });

            _ = messageService.ConnectAsync();
        }

        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await messageService.SendMessageAsync(SendTextBox.Text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Debug log");
            }
        }

        private async void getLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StoreTextBox.Text = await messageService.GetStore();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Debug log");
            }
        }

    }
}
