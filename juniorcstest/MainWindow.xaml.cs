using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace juniorcstest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel ModelViewModel;

        public MainWindow()
        {
            InitializeComponent();
            ModelViewModel = new ViewModel();
            DataContext = ModelViewModel;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void OnUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            await ModelViewModel.UpdateBalance();
        }
    }
}