using RemoteSystemManager.Common;
using RemoteSystemManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteSystemManager.ViewModel
{
    public class ComputerViewModel : BindableBase
    {
        #region 싱글톤 구현
        private static ComputerViewModel _instance;

        private ComputerViewModel()
        {
            Computers = new ItemObservableCollection<Computer>();

            InitTest();

            GetAllPrograms().ToList().ForEach(_listManagedPrograms.Add);
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

        private ItemObservableCollection<Computer> _computers;
        private ItemObservableCollection<Program> _listManagedPrograms = new ItemObservableCollection<Program>();
        private Computer _selectedViewComputer;
        private Computer _selectedEditingComputer;

        public ItemObservableCollection<Computer> Computers { get => _computers; set => _computers = value; }
        public ItemObservableCollection<Program> ListManagedPrograms { get => _listManagedPrograms; set => _listManagedPrograms = value; }
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
        public List<String> GetAllProgramNames { get => GetEnumerableAllProgramNames().ToList(); }

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


        #region 테스트
        private void InitTest()
        {
            Computer computer1 = new Computer()
            {
                ComputerIp = "127.0.0.1",
                ComputerName = "1호기"
            };
            computer1.Programs.Add(new Program()
            {
                ComputerName = computer1.ComputerName,
                ProgramName = "계기판프로그램",
                ProgramPath = @"C:\Users\dmjan\Downloads\test\SW-AD61800638E002-EXF-R0.exe"
            });
            Computer computer2 = new Computer()
            {
                ComputerIp = "127.0.0.2",
                ComputerName = "2호기"
            };
            computer2.Programs.Add(new Program()
            {
                ComputerName = computer2.ComputerName,
                ProgramName = "영상프로그램",
                ProgramPath = @"C:\영상\프로그램.exe"
            });
            computer2.Programs.Add(new Program()
            {
                ComputerName = computer2.ComputerName,
                ProgramName = "음성프로그램",
                ProgramPath = @"C:\영상\프로그램.exe"
            });
            Computers.Add(computer1);
            Computers.Add(computer2);
        }
        #endregion
    }
}
