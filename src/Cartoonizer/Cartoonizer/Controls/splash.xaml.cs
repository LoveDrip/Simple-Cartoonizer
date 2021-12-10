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

namespace Cartoonizer
{
    /// <summary>
    /// Interaction logic for splash.xaml
    /// </summary>
    public partial class splash : Window
    {
        public splash()
        {
            InitializeComponent();
        }
        private void onBtnGender(object sender, RoutedEventArgs e) {
            bool isFemale = false;
            if (sender == btnMale)
            {
                isFemale = false;
            }
            else {
                isFemale = true;
            }
            MainWindow mainWindow = new MainWindow(isFemale);
            mainWindow.Show();
            Close();
        }
    }
}
