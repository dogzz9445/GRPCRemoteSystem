using RemoteSystemManager.Common;
using RemoteSystemManager.Model;
using ScenarioGeneratorApp.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RemoteSystemManager.ViewModel
{
    public class ComputerViewModel : BindableBase
    {
        #region 싱글톤 구현
        private static ComputerViewModel _instance;

        private ComputerViewModel()
        {
            Computers = new ItemObservableCollection<Computer>();
            _listManagedPrograms = new ItemObservableCollection<Program>();

            ReadViewModel(ComputersConfigFileName);
        }

        public static ComputerViewModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ComputerViewModel();
                }
                return _instance;
            }
        }
        #endregion

        private const string ComputersConfigFileName = "Data/computers.json";
        private static Func<Action, Task> callOnUiThread = async (handler) =>
            await Application.Current.Dispatcher.InvokeAsync(handler);

        private ItemObservableCollection<Computer> _computers;
        private ItemObservableCollection<Program> _listManagedPrograms;
        private ObservableCollection<string> _listManagedProgramNames;
        private Computer _selectedViewComputer;
        private Computer _selectedEditingComputer;

        private DelegateCommand _saveComputersCommand;
        private DelegateCommand _readComputersCommand;
        private DelegateCommand _addComputerCommand;
        private DelegateCommand _removeComputerCommand;
        private DelegateCommand _addProgramCommand;
        private DelegateCommand _removeProgramCommand;


        public ItemObservableCollection<Computer> Computers { get => _computers; set => SetCollectionProperty(ref _computers, value); }
        public ItemObservableCollection<Program> ListManagedPrograms { get => _listManagedPrograms; set => SetCollectionProperty(ref _listManagedPrograms, value); }

        public ObservableCollection<string> ListManagedProgramNames
        {
            get => _listManagedProgramNames;
            set => _listManagedProgramNames = value;
        }

        public Computer SelectedViewComputer { get => _selectedViewComputer; set => SetProperty(ref _selectedViewComputer, value); }
        public Computer SelectedEditingComputer { get => _selectedEditingComputer; set => SetProperty(ref _selectedEditingComputer, value); }

        public bool? IsAllSelectedComputers
        {
            get
            {
                var selected = Computers.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count == 1 ? selected.Single() : (bool?)null;
            } 
            set
            {
                if (value.HasValue)
                {
                    SelectAll(value.Value, Computers);
                    RaisePropertyChangedEvent();
                }
            }
        }
        public bool? IsAllSelectedPrograms
        {
            get
            {
                var selected = ListManagedPrograms.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count == 1 ? selected.Single() : (bool?)null;
            }
            set
            {
                if (value.HasValue)
                {
                    SelectAll(value.Value, ListManagedPrograms);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public void SelectAll(bool select, IEnumerable<Computer> computers)
        {
            foreach (var model in computers)
            {
                model.IsSelected = select;
            }
        }

        public void SelectAll(bool select, IEnumerable<Program> programs)
        {
            foreach (var model in programs)
            {
                model.IsSelected = select;
            }
        }
        public List<String> GetAllProgramNames => GetEnumerableAllProgramNames().ToList();

        public DelegateCommand SaveComputersCommand 
        {
            get
            {
                return _saveComputersCommand ?? (_saveComputersCommand = new DelegateCommand(
                    async () =>
                    {
                        await SaveViewModel(ComputersConfigFileName);
                    }));
            } 
        }

        public DelegateCommand ReadComputersCommand
        {
            get
            {
                return _readComputersCommand ??= new DelegateCommand(
                    () =>
                    {
                        ReadViewModel(ComputersConfigFileName);
                    });
            }
        }

        public IEnumerable<Program> GetAllPrograms()
        {
            if (Computers != null)
            {
                foreach (var computer in Computers)
                {
                    foreach (var program in computer.Programs)
                    {
                        yield return program;
                    }
                }
            }
        }

        private IEnumerable<String> GetEnumerableAllProgramNames()
        {
            yield return "전체";
            if (Computers != null)
            {
                foreach (var computer in Computers)
                {
                    foreach (var program in computer.Programs)
                    {
                        yield return program.ProgramName;
                    }
                }
            }
        }

        #region 설정 저장 및 불러오기
        private async void ReadViewModel(string fileName)
        {
            if (File.Exists(fileName))
            {
                var computers = await JsonManager.ReadJsonFile<List<Computer>>(fileName);
                await callOnUiThread(() =>
                {
                    Computers.Clear();
                    if (computers != null)
                    {
                        computers.ForEach(Computers.Add);
                    }
                    GetAllPrograms().ToList().ForEach(_listManagedPrograms.Add);
                    IsAllSelectedComputers = false;
                    IsAllSelectedPrograms = false;
                });
            }
        }

        private async Task SaveViewModel(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            Directory.CreateDirectory(directory);

            if (Computers.Count > 0)
            {
                await JsonManager.WritJsonFile(fileName, Computers);
            }
        }
        #endregion
    }
}
