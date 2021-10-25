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
using Grpc.Core;
using Grpc.Net.Client;
using RemoteSystem.Protobuf;
using RemoteSystem.Remote;
using RemoteSystemClient.Services;

namespace RemoteSystemClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var channelOptions = new GrpcChannelOptions();
            var channel = GrpcChannel.ForAddress("127.0.0.1:5001");
            var remoteService = new RemoteService();
            
            remoteService.
        }
    }
}
