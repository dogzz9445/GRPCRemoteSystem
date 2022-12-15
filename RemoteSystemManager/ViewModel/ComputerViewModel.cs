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
using RemoteSystem.Proto;
using Application = System.Windows.Application;
using Timer = System.Timers.Timer;
using Grpc.Net.Client;
using System.Net.Http;

namespace RemoteSystemManager.ViewModel
{
    public class ComputerViewModel : BindableBase
    {
        #region 싱글톤 구현

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
            Computers = new ItemObservableCollection<RemoteComputer>();
            ListManagedPrograms = new ItemObservableCollection<RemoteProgram>();
            ListManagedProgramNames = new ObservableCollection<string>();

            Computers.ItemPropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RemoteComputer.ComputerIp))
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
        private const int GRPC_SERVER_PORT = 9621;

        private ItemObservableCollection<RemoteComputer> _computers;
        private ItemObservableCollection<RemoteProgram> _listManagedPrograms;
        private ObservableCollection<string> _listManagedProgramNames;
        private RemoteComputer _selectedViewComputer;
        private string _selectedViewProgramName;
        private RemoteProgram _selectedViewProgram;
        private RemoteProgram _selectedControlProgram;
        private RemoteComputer _selectedEditingComputer;
        private RemoteProgram _selectedEditingProgram;

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

        public ItemObservableCollection<RemoteComputer> Computers
        {
            get => _computers;
            set => SetCollectionProperty(ref _computers, value);
        }

        public ItemObservableCollection<RemoteProgram> ListManagedPrograms
        {
            get => _listManagedPrograms;
            set => SetCollectionProperty(ref _listManagedPrograms, value);
        }

        public ObservableCollection<string> ListManagedProgramNames
        {
            get => _listManagedProgramNames;
            set => SetProperty(ref _listManagedProgramNames, value);
        }

        public RemoteComputer SelectedViewComputer
        {
            get => _selectedViewComputer;
            set => SetProperty(ref _selectedViewComputer, value);
        }

        public RemoteProgram SelectedViewProgram
        {
            get => _selectedViewProgram;
            set => SetProperty(ref _selectedViewProgram, value);
        }

        public string SelectedViewProgramName
        {
            get => _selectedViewProgramName;
            set => SetProperty(ref _selectedViewProgramName, value);
        }

        public RemoteProgram SelectedControlProgram
        {
            get => _selectedControlProgram;
            set => SetProperty(ref _selectedControlProgram, value);
        }

        public RemoteComputer SelectedEditingComputer
        {
            get => _selectedEditingComputer;
            set => SetProperty(ref _selectedEditingComputer, value);
        }

        public RemoteProgram SelectedEditingProgram
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

        public void SelectAll(bool select, IEnumerable<RemoteComputer> computers)
        {
            foreach (var model in computers)
            {
                model.IsSelected = select;
            }
        }

        public void SelectAll(bool select, IEnumerable<RemoteProgram> programs)
        {
            foreach (var model in programs)
            {
                model.IsSelected = select;
            }
        }

        public IEnumerable<RemoteProgram> GetAllPrograms()
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
        private DelegateCommand<RemoteProgram> _runSingleProgramCommand;
        private DelegateCommand<RemoteProgram> _killSingleProgramCommand;
        private DelegateCommand _runMultipleProgramsCommand;
        private DelegateCommand _killMultipleProgramsCommand;

        private DelegateCommand _startAllComputersCommand;
        private DelegateCommand _restartAllComputersCommand;
        private DelegateCommand _shutdownAllComputersCommand;

        private DelegateCommand _startSingleComputerCommand;
        private DelegateCommand _restartSingleComputerCommand;
        private DelegateCommand _shutdownSingleComputerCommand;
        private DelegateCommand _startAVLRSingleComputerCommand;
        private DelegateCommand _stopAVLRSingleComputerCommand;
        private DelegateCommand _startMobileHotSpotSingleComputerCommand;
        private DelegateCommand _stopMobileHotSpotSingleComputerCommand;

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
                            SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Start);
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
                            SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Stop);
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
                                SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Start);
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
                                SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Stop);
                            }
                        }
                    }
                });

        public DelegateCommand<RemoteProgram> RunSingleProgramCommand =>
            _runSingleProgramCommand ??= new DelegateCommand<RemoteProgram>(
                (program) =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.ComputerName == program.ComputerName)
                        {
                            SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Start);
                        }
                    }
                });

        public DelegateCommand<RemoteProgram> KillSingleProgramCommand =>
            _killSingleProgramCommand ??= new DelegateCommand<RemoteProgram>(
                (program) =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.ComputerName == program.ComputerName)
                        {
                            SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Stop);
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
                                SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Start);
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
                                SendProgramControl(computer, program, ProgramControl.Types.ProgramControlType.Stop);
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
                        SendComputerControl(computer, ComputerControl.Types.ComputerControlType.ComputerStart);
                    }
                });

        public DelegateCommand RestartAllComputersCommand =>
            _restartAllComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        SendComputerControl(computer, ComputerControl.Types.ComputerControlType.ComputerRestart);
                    }
                });

        public DelegateCommand ShutdownAllComputersCommand =>
            _shutdownAllComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        SendComputerControl(computer, ComputerControl.Types.ComputerControlType.ComputerStop);
                    }
                });

        public DelegateCommand StartSingleComputerCommand =>
            _startSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer != null)
                    {
                        SendComputerControl(SelectedViewComputer, ComputerControl.Types.ComputerControlType.ComputerStart);
                    }
                });

        public DelegateCommand RestartSingleComputerCommand =>
            _restartSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer != null)
                    {
                        SendComputerControl(SelectedViewComputer, ComputerControl.Types.ComputerControlType.ComputerRestart);
                    }
                });

        public DelegateCommand ShutdownSingleComputerCommand =>
            _shutdownSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer != null)
                    {
                        SendComputerControl(SelectedViewComputer, ComputerControl.Types.ComputerControlType.ComputerStop);
                    }
                });


        public DelegateCommand StartALVRSelectedComputerCommand =>
            _startAVLRSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer == null)
                    {
                        return;
                    }
                    SendVRControl(SelectedViewComputer, VRControl.Types.VRControlType.AlvrClientStart);
                });

        public DelegateCommand StopALVRSelectedComputerCommand =>
            _stopAVLRSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer == null)
                    {
                        return;
                    }
                    SendVRControl(SelectedViewComputer, VRControl.Types.VRControlType.AlvrClientStop);
                });

        public DelegateCommand StartMobileHotSpotSelectedComputerCommand =>
             _startMobileHotSpotSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer == null)
                    {
                        return;
                    }
                    SendComputerControl(SelectedViewComputer, ComputerControl.Types.ComputerControlType.MobileHotspotStart);
                });

        public DelegateCommand StopMobileHotSpotSelectedComputerCommand =>
             _stopMobileHotSpotSingleComputerCommand ??= new DelegateCommand(
                () =>
                {
                    if (SelectedViewComputer == null)
                    {
                        return;
                    }
                    SendComputerControl(SelectedViewComputer, ComputerControl.Types.ComputerControlType.MobileHotspotStop);
                });

        public DelegateCommand StartMultipleComputersCommand =>
            _startMultipleComputersCommand ??= new DelegateCommand(
                () =>
                {
                    foreach (var computer in Computers)
                    {
                        if (computer.IsSelected)
                        {
                            SendComputerControl(computer, ComputerControl.Types.ComputerControlType.ComputerStart);
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
                            SendComputerControl(computer, ComputerControl.Types.ComputerControlType.ComputerRestart);
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
                            SendComputerControl(computer, ComputerControl.Types.ComputerControlType.ComputerStop);
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
                () => { Computers.Add(new RemoteComputer()); });

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
                        SelectedEditingComputer.AddProgram(new RemoteProgram());
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
                List<RemoteProgram> filteredPrograms = new List<RemoteProgram>();
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
                var computers = await JsonManager.ReadJsonFile<List<RemoteComputer>>(fileName);
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

        #region GRPC Settings
        public GrpcChannel GetChannel(string ip)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return GrpcChannel.ForAddress($"https://{ip}:{GRPC_SERVER_PORT}", new GrpcChannelOptions { HttpHandler = httpHandler });
        }
        #endregion

        private async void SendProgramControl(RemoteComputer computer, RemoteProgram program, ProgramControl.Types.ProgramControlType type)
        {
            try
            {
                ProgramControl programControl = new ProgramControl()
                {
                    Control = type,
                    Name = program.ProgramName,
                    FileName = program.ProgramPath,
                    ProcessName = program.ProgramProcessName
                };
                var channel = GetChannel(computer.ComputerIp);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostProgramControlMessageAsync(programControl);
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void SendComputerControl(RemoteComputer computer, ComputerControl.Types.ComputerControlType type)
        {
            try
            {
                ComputerControl computerControl = new ComputerControl()
                {
                    Control = type
                };
                var channel = GetChannel(computer.ComputerIp);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostComputerControlMessageAsync(computerControl);
                await channel.ShutdownAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void SendVRControl(RemoteComputer computer, VRControl.Types.VRControlType type)
        {
            try
            {
                VRControl vrControl = new VRControl()
                {
                    Control = type
                };
                var channel = GetChannel(computer.ComputerIp);
                var client = new Remote.RemoteClient(channel);
                var reply = await client.PostVRControlMessageAsync(vrControl);
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
