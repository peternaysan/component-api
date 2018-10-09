using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.IO;
using System.Configuration;

namespace gac.aes.wpf.testapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void b1_Click(object sender, RoutedEventArgs e)
        {
            var apiUrl = ConfigurationManager.AppSettings["AesApiUrl"];
            var client = new HttpClient();
            var json = this.GetDummyJsonString();
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var result = client.PostAsync(apiUrl, content).Result;
            var Id=result.Content.ReadAsStringAsync();
            urlText.Text += $"\\{Id}";
            var url = urlText.Text;
            webControl.Address = url;
        }

        private string GetDummyJsonString()
        {
            using (StreamReader r = new StreamReader("data.json"))
            {
                string json = r.ReadToEnd();
                return json;
            }
        }
    }
}
