using RemoteSystemManager.Model;
using RemoteSystemManager.ViewModel;
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

namespace RemoteSystemManager.View
{
    /// <summary>
    /// HomePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomePage : Page
    {
        public ComputerViewModel ComputerViewModel { get; set; }

        public HomePage()
        {
            InitializeComponent();

            ComputerViewModel = ComputerViewModel.Instance;
            DataContext = ComputerViewModel;
            DataGridComputerManagement.ItemsSource = ComputerViewModel.Computers;
            DataGridProgramManagement.ItemsSource = ComputerViewModel.ListManagedPrograms;
            ListViewProgramNames.ItemsSource = ComputerViewModel.ListManagedProgramNames;
        }

        private void ListViewProgramNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComputerViewModel.UpdateManagedPrograms((sender as ListView)?.SelectedItem as string);
        }
    }
}
