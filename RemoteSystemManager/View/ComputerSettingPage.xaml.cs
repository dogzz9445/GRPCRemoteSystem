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
    /// ComputerSettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ComputerSettingPage : Page
    {
        private ComputerViewModel _computerViewModel;

        public ComputerSettingPage()
        {
            InitializeComponent();

            _computerViewModel = ComputerViewModel.Instance;
            this.DataContext = _computerViewModel;
            DataGridComputerManagement.ItemsSource = _computerViewModel.Computers;
        }

    }
}
