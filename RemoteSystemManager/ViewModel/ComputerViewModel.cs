using RemoteSystemManager.Common;
using RemoteSystemManager.Model;
using ScenarioGeneratorApp.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using RemoteSystem.Remote;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;

namespace RemoteSystemManager.ViewModel
{
    public class ComputerViewModel : BindableBase
    {
        #region 싱글톤 구현

        // TODO: 설정으로 고치기
        private const string SERVER_INSECURE_PORT = "5000";
        private const string SERVER_SECURE_PORT = "5001";

        private static ComputerViewModel _instance;

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

        private ComputerViewModel()
        {
            Computers = new ItemObservableCollection<Computer>();
            ListManagedPrograms = new ItemObservableCollection<Program>();
            ListManagedProgramNames = new ObservableCollection<string>();

            Computers.ItemPropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Computer.ComputerIp))
                {
                    Console.WriteLine(e.PropertyName);
                }
            };

            ReadViewModel(ComputersConfigFileName);
            MonitoringTimer.Start();
        }

        public Timer MonitoringTimer { get; } = new Timer(1000);

        private static Func<Action, Task> callOnUiThread = async (handler) =>
            await Application.Current.Dispatcher.InvokeAsync(handler);

        private const string ComputersConfigFileName = "data/computers.json";

        private ItemObservableCollection<Computer> _computers;
        private ItemObservableCollection<Program> _listManagedPrograms;
        private ObservableCollection<string> _listManagedProgramNames;
        private Computer _selectedViewComputer;
        private string _selectedViewProgramName;
        private Program _selectedViewProgram;
        private Program _selectedControlProgram;
        private Computer _selectedEditingComputer;
        private Program _selectedEditingProgram;

        public bool IsMacAddressCheck
        {
            get => IsMacAddressCheck;
            set
            {
                IsMacAddressCheck = value;
                //if (IsMacAddressCheck)
                //{
                //    Task.Factory.StartNew(async () =>
                //    {
                //        await Task.Delay(10000);
                //        IsMacAddressCheck = false;
                //    });
                //}
            }
        }
        public bool IsComputerStatusCheck { get; set; } = false;

        #region Public 변수 정의

        public ItemObservableCollection<Computer> Computers
        {
            get => _computers;
            set => SetCollectionProperty(ref _computers, value);
        }

        public ItemObservableCollection<Program> ListManagedPrograms
        {
            get => _listManagedPrograms;
            set => SetCollectionProperty(ref _listManagedPrograms, value);
        }

        public ObservableCollection<string> ListManagedProgramNames
        {
            get => _listManagedProgramNames;
            set => SetProperty(ref _listManagedProgramNames, value);
        }

        public Computer SelectedViewComputer
        {
            get => _selectedViewComputer;
            set => SetProperty(ref _selectedViewComputer, value);
        }

        public Program SelectedViewProgram
        {
            get => _selectedViewProgram;
            set => SetProperty(ref _selectedViewProgram, value);
        }

        public string SelectedViewProgramName
        {
            get => _selectedViewProgramName;
            set => SetProperty(ref _selectedViewProgramName, value);
        }

        public Program SelectedControlProgram
        {
            get => _selectedControlProgram;
            set => SetProperty(ref _selectedControlProgram, value);
        }

        public Computer SelectedEditingComputer
        {
            get => _selectedEditingComputer;
            set => SetProperty(ref _selectedEditingComputer, value);
        }

        public Program SelectedEditingProgram
        {
            get => _selectedEditingProgram;
            set => SetProperty(ref _selectedEditingProgram, value);
        }

        #endregion

        public bool? IsAllSelectedComputers
        {
            get
            {
                var selected = Computers.Select(item => item.IsSelected).Distinct().ToList();
                return selected.Count == 1 ? selected.Single() : (bool?) null;
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
                return selected.Count == 1 ? selected.Single() : (bool?) null;
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

        #region 커맨드들 / 프로그램 시작 종료, 컴퓨터 시작 종료

        private DelegateCommand _runAllProgramsCommand;
        private DelegateCommand _killAllProgramsCommand;
        private DelegateCommand _runAllSelectedProgramsCommand;
        private DelegateCommand _killAllSelectedProgramsCommand;
        private DelegateCommand<Program> _runSingleProgramCommand;
        private DelegateCommand<Program> _killSingleProgramCommand;
        private DelegateCommand _runMultipleProgramsCommand;
        private DelegateCommand _killMultipleProgramsCommand;

        private DelegateCommand _startAllComputersCommand;
        private DelegateCommand _restartAllComputersCommand;
        private DelegateCommand _shutdownAllComputersCommand;
        private DelegateCommand _startSingleComputerCommand;
        private DelegateCommand _restartSingleComputerCommand;
        private DelegateCommand _shutdownSingleComputerCommand;
        private DelegateCommand _startMultipleComputersCommand;
        private DelegateCommand _restartMultipleComputersCommand;
        private DelegateCommand _shutdownMultipleComputersCommand;

        private DelegateCommand _saveComputersCommand;
        private DelegateCommand _readComputersCommand;

        private DelegateCommand _addComputerCommand;
        private DelegateCommand _removeComputerCommand;
        private DelegateCommand _addProgramCommand;
        private DelegateCommand _removeProgramCommand;

        public DelegateCommand RunAllProgramsCommand =>
            _runAllProgramsCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        foreach (var program in computer.Programs)
                        {
                            Task.Factory.StartNew(async () => await SendRunProgram(computer, program));
                        }
                    }
                });

        public DelegateCommand KillAllProgramsCommand =>
            _killAllProgramsCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        foreach (var program in computer.Programs)
                        {
                            Task.Factory.StartNew(async () => await SendKillProgram(computer, program));
                        }
                    }
                });

        public DelegateCommand RunAllSelectedProgramsCommand =>
            _runAllSelectedProgramsCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        foreach (var program in computer.Programs)
                        {
                            if (program.ProgramName == SelectedViewProgramName)
                            {
                                Task.Factory.StartNew(async () => await SendRunProgram(computer, program));
                            }
                        }
                    }
                });

        public DelegateCommand KillAllSelectedProgramsCommand =>
            _killAllSelectedProgramsCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        foreach (var program in computer.Programs)
                        {
                            if (program.ProgramName == SelectedViewProgramName)
                            {
                                Task.Factory.StartNew(async () => await SendKillProgram(computer, program));
                            }
                        }
                    }
                });

        public DelegateCommand<Program> RunSingleProgramCommand =>
            _runSingleProgramCommand ??= new DelegateCommand<Program>(
                (program) =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.ComputerName == program.ComputerName)
                        {
                            Task.Factory.StartNew(async () => await SendRunProgram(computer, program));
                        }
                    }
                });

        public DelegateCommand<Program> KillSingleProgramCommand =>
            _killSingleProgramCommand ??= new DelegateCommand<Program>(
                (program) =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.ComputerName == program.ComputerName)
                        {
                            Task.Factory.StartNew(async () => await SendKillProgram(computer, program));
                        }
                    }
                });

        public DelegateCommand RunMultipleProgramsCommand =>
            _runMultipleProgramsCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        foreach (var program in computer.Programs)
                        {
                            if (program.IsSelected)
                            {
                                Task.Factory.StartNew(async () => await SendRunProgram(computer, program));
                            }
                        }
                    }
                });

        public DelegateCommand KillMultipleProgramsCommand =>
            _killMultipleProgramsCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        foreach (var program in computer.Programs)
                        {
                            if (program.IsSelected)
                            {
                                Task.Factory.StartNew(async () => await SendKillProgram(computer, program));
                            }
                        }
                    }
                });

        public DelegateCommand StartAllComputersCommand =>
            _startAllComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        Task.Factory.StartNew(async () => await SendStartComputer(computer));
                    }
                });

        public DelegateCommand RestartAllComputersCommand =>
            _restartAllComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        Task.Factory.StartNew(async () => await SendRestartComputer(computer));
                    }
                });

        public DelegateCommand ShutdownAllComputersCommand =>
            _shutdownAllComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        Task.Factory.StartNew(async () => await SendShutdownComputer(computer));
                    }
                });

        public DelegateCommand StartSingleComputerCommand =>
            _startSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer != null)
                    {
                        Task.Factory.StartNew(async () => await SendStartComputer(SelectedViewComputer));
                    }
                });

        public DelegateCommand RestartSingleComputerCommand =>
            _restartSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer != null)
                    {
                        Task.Factory.StartNew(async () => await SendRestartComputer(SelectedViewComputer));
                    }
                });

        public DelegateCommand ShutdownSingleComputerCommand =>
            _shutdownSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer != null)
                    {
                        Task.Factory.StartNew(async () => await SendShutdownComputer(SelectedViewComputer));
                    }
                });

        public DelegateCommand StartMultipleComputersCommand =>
            _startMultipleComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.IsSelected)
                        {
                            Task.Factory.StartNew(async () => await SendStartComputer(computer));
                        }
                    }
                });

        public DelegateCommand RestartMultipleComputersCommand =>
            _restartMultipleComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.IsSelected)
                        {
                            Task.Factory.StartNew(async () => await SendRestartComputer(computer));
                        }
                    }
                });

        public DelegateCommand ShutdownMultipleComputersCommand =>
            _shutdownMultipleComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.IsSelected)
                        {
                            Task.Factory.StartNew(async () => await SendShutdownComputer(computer));
                        }
                    }
                });

        public DelegateCommand SaveComputersCommand =>
            _saveComputersCommand ??= new DelegateCommand(
                () => { Task.Factory.StartNew(async () => await SaveViewModel(ComputersConfigFileName)); });

        public DelegateCommand ReadComputersCommand =>
            _readComputersCommand ??= new DelegateCommand(
                () => { ReadViewModel(ComputersConfigFileName); });

        public DelegateCommand AddComputerCommand =>
            _addComputerCommand ??= new DelegateCommand(
                () => { Computers.Add(new Computer()); });

        public DelegateCommand RemoveComputerCommand =>
            _removeComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedEditingComputer != null)
                    {
                        Computers.Remove(Computers.Single(x => x == SelectedEditingComputer));
                    }
                });

        public DelegateCommand AddProgramCommand =>
            _addProgramCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedEditingComputer != null)
                    {
                        SelectedEditingComputer.AddProgram(new Program());
                    }
                });

        public DelegateCommand RemoveProgramCommand =>
            _removeProgramCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedEditingComputer != null)
                    {
                        if (SelectedEditingProgram != null)
                        {
                            SelectedEditingComputer.Programs.Remove(
                                SelectedEditingComputer.Programs.Single(x => x == SelectedEditingProgram));
                        }
                    }
                });

        #endregion

        #region 설정 저장 및 불러오기

        public void UpdateMacAddresses()
        {
            foreach (var computer in Computers)
            {
                //TODO:
                // if (validate ip is computer)
                Task.Factory.StartNew(async () =>
                {
                    var macAddress = await WakeOnLan.GetMACAddressFromARP(computer.ComputerIp);
                    // TODO:
                    // if (validate macAddress is macAddress)
                    computer.ComputerMacAddress = macAddress;
                });
            }
        }

        public async void UpdateManagedProgramNames()
        {
            await callOnUiThread(() =>
            {
                var filteredPrograms = GetEnumerableAllProgramNames().ToList();
                for (int i = ListManagedProgramNames.Count - 1; i >= 0; i--)
                {
                    var item = ListManagedProgramNames[i];
                    if (!filteredPrograms.Contains(item))
                    {
                        ListManagedProgramNames.Remove(item);
                    }
                }

                foreach (var item in filteredPrograms)
                {
                    if (!ListManagedProgramNames.Contains(item))
                    {
                        ListManagedProgramNames.Add(item);
                    }
                }
            });
        }

        public async void UpdateManagedPrograms(string filterProgramName)
        {
            await callOnUiThread(() =>
            {
                // 모든 프로그램들 IsSelected 체크 해제
                // SendMessage에 담을 프로그램 이름을 IsSelected로 체크하기 위함
                var allProrams = GetAllPrograms().ToList();
                foreach (var program in allProrams)
                {
                    program.IsSelected = false;
                }

                // 프로그램들 전체 체크도 해제
                IsAllSelectedPrograms = false;

                // 필터링
                List<Program> filteredPrograms = new List<Program>();
                if (filterProgramName == "전체")
                {
                    filteredPrograms = GetAllPrograms().ToList();
                }
                else
                {
                    filteredPrograms = GetAllPrograms()
                        .Where((program) => program.ProgramName == filterProgramName).ToList();
                }

                for (int i = ListManagedPrograms.Count - 1; i >= 0; i--)
                {
                    var item = ListManagedPrograms[i];
                    if (!filteredPrograms.Contains(item))
                    {
                        ListManagedPrograms.Remove(item);
                    }
                }

                foreach (var item in filteredPrograms)
                {
                    if (!ListManagedPrograms.Contains(item))
                    {
                        ListManagedPrograms.Add(item);
                    }
                }
            });
        }

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

                    ListManagedPrograms.Clear();
                    ListManagedProgramNames.Clear();
                    GetAllPrograms().ToList().ForEach(ListManagedPrograms.Add);
                    GetEnumerableAllProgramNames().ToList().ForEach(ListManagedProgramNames.Add);
                    IsAllSelectedComputers = false;
                    IsAllSelectedPrograms = false;
                });
            }
        }

        private async Task SaveViewModel(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            if (Computers.Count > 0)
            {
                await JsonManager.WritJsonFile(fileName, Computers);
            }
        }

        #endregion

        #region 통신 / 컴퓨터 시작,재시작,종료, 프로그램 시작,종료

        public async Task SendRunProgram(Computer computer, Program program)
        {
            try
            {
                ProgramControl programControl = new ProgramControl()
                {
                    Control = ProgramControl.Types.ProgramControlType.Start,
                    Name = program.ProgramName,
                    FileName = program.ProgramPath,
                    ProcessName = program.ProgramProcessName
                };
                Channel channel = new Channel($"{computer.ComputerIp}:{SERVER_INSECURE_PORT}", ChannelCredentials.Insecure);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostProgramControlMessageAsync(programControl);
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SendKillProgram(Computer computer, Program program)
        {
            try
            {
                ProgramControl programControl = new ProgramControl()
                {
                    Control = ProgramControl.Types.ProgramControlType.Stop,
                    Name = program.ProgramName,
                    FileName = program.ProgramPath,
                    ProcessName = program.ProgramProcessName
                };
                Channel channel = new Channel($"{computer.ComputerIp}:{SERVER_INSECURE_PORT}", ChannelCredentials.Insecure);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostProgramControlMessageAsync(programControl);
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task SendStartComputer(Computer computer)
        {
            // TODO:
            // if (ComputerMacAddress is validate)
            await WakeOnLan.SendMagicPacketAsync(computer.ComputerMacAddress);

            // TODO:
            // Task<int>로 result값 받아서 화면에 띄워주기
        }

        private async Task SendRestartComputer(Computer computer)
        {
            try
            {
                ComputerControl computerControl = new ComputerControl()
                {
                    Control = ComputerControl.Types.ComputerControlType.Restart
                };
                Channel channel = new Channel($"{computer.ComputerIp}:{SERVER_INSECURE_PORT}", ChannelCredentials.Insecure);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostComputerControlMessageAsync(computerControl);
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task SendShutdownComputer(Computer computer)
        {
            await Task.Yield();
            try
            {
                ComputerControl computerControl = new ComputerControl()
                {
                    Control = ComputerControl.Types.ComputerControlType.Stop
                };
                Channel channel = new Channel($"{computer.ComputerIp}:{SERVER_INSECURE_PORT}", ChannelCredentials.Insecure);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostComputerControlMessageAsync(computerControl);
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region 타이머 및 통신 / 컴퓨터 상태 및 맥주소 받아오기

        public void RunMonitoringComputer(object sender, ElapsedEventArgs e)
        {
            if (Computers == null)
            {
                return;
            }

            //if (IsComputerStatusCheck)
            //{
            //    foreach (var computer in Computers)
            //    {
            //        Task.Factory.StartNew(async () =>
            //        {
            //            try
            //            {
            //                Channel channel = new Channel(computer.ComputerIp + ":5000", ChannelCredentials.Insecure);
            //                var client = new Remote.RemoteClient(channel);
            //                if (IsMacAddressCheck)
            //                {
            //                    var reply = await client.GetMacAddressAsync(new Empty());
            //                    if (!string.IsNullOrEmpty(reply.MacAddress))
            //                    {
            //                        computer.ComputerMacAddress = reply.MacAddress;
            //                    }
            //                }

            //                await channel.ShutdownAsync();
            //            }
            //            catch (Exception e)
            //            {
            //                computer.ComputerStatus = e.ToString();
            //                Console.WriteLine(e);
            //            }
            //        });
            //    }
            //}
        }

        #endregion
    }
}
