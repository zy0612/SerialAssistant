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
using System.IO.Ports;

namespace SerialAssistant
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
   
    
    public partial class MainWindow : Window
    {
        private SerialPort _serialPort;

        public MainWindow()
        {
            InitializeComponent();
            cmbPort.ItemsSource = SerialPort.GetPortNames();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            string portName = cmbPort.SelectedItem.ToString();
            string baudStr = cmbBaud.Text;
            int baudRate = int.Parse(baudStr);

            try
            {
                _serialPort = new SerialPort(portName, baudRate);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                AddRecord("系统", $"已打开 {portName} @ {baudRate}", System.Windows.Media.Brushes.Gray);

                btnOpen.IsEnabled = false;
                btnClose.IsEnabled = true;
                btnSend.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开串口失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                AddRecord("系统", $"打开 {portName} 失败：{ex.Message}", System.Windows.Media.Brushes.Red);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
        }


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }

            AddRecord("系统", "串口已关闭", System.Windows.Media.Brushes.Gray);

            btnOpen.IsEnabled = true;
            btnClose.IsEnabled = false;
            btnSend.IsEnabled = false;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadExisting();

            // 串口事件在后台线程运行，不能直接改界面
            // 必须用 Dispatcher.Invoke 回到主线程
            Dispatcher.Invoke(() =>
            {
                AddRecord("接收", data, System.Windows.Media.Brushes.Green);
            });
        }

        private void SendMessage()
        {
            string text = txtInput.Text;

            if (string.IsNullOrWhiteSpace(text))
                return;

            // 1. 先显示到自己界面上（蓝色）
            AddRecord("发送", text, System.Windows.Media.Brushes.Blue);

            // 2. 如果有打开串口，就真正发出去
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.WriteLine(text);
            }

            txtInput.Clear();
        }


        private void AddRecord(string direction, string text, System.Windows.Media.Brush color)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            string record = $"[{time}][{direction}]{text}";

            var item = new System.Windows.Controls.ListBoxItem();
            item.Content = record;
            item.Foreground = color;

            lstHIstory.Items.Insert(0,item);

            try
            {
                string logPath = @"D:\SerialAssistant_Log.txt";
                System.IO.File.AppendAllText(logPath, record + System.Environment.NewLine);
            }
            catch { }
        }
        private void TxtInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendMessage();
            }
        }
    }
}
