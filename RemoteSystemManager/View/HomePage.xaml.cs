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
        private ComputerViewModel _computerViewModel;

        public HomePage()
        {
            InitializeComponent();

            _computerViewModel = ComputerViewModel.Instance;
            this.DataContext = _computerViewModel;
            DataGridComputerManagement.ItemsSource = _computerViewModel.Computers;
            DataGridProgramManagement.ItemsSource = _computerViewModel.ListManagedPrograms;
            ListViewProgramNames.ItemsSource = _computerViewModel.GetAllProgramNames;
        }

        private void ListViewProgramNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Program> filteredPrograms = new List<Program>();
            if ((sender as ListView).SelectedItem.ToString() == "전체")
            {
                filteredPrograms = _computerViewModel.GetAllPrograms().ToList();
            }
            else
            {
                filteredPrograms = _computerViewModel.GetAllPrograms().Where((program) => program.ProgramName == (sender as ListView).SelectedItem.ToString()).ToList();
            }

            for (int i = _computerViewModel.ListManagedPrograms.Count - 1; i >= 0; i--)
            {
                var item = _computerViewModel.ListManagedPrograms[i];
                if (!filteredPrograms.Contains(item))
                {
                    _computerViewModel.ListManagedPrograms.Remove(item);
                }
            }
            foreach (var item in filteredPrograms)
            {
                if (!_computerViewModel.ListManagedPrograms.Contains(item))
                {
                    _computerViewModel.ListManagedPrograms.Add(item);
                }
            }
        }
    }
}
