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
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using RemoteSystem.Remote;

namespace RemoteSystemManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new Remote.RemoteClient(channel);
                var heartBeat = client.GetHeartBeat(new Empty());

                TextBlockLog.Text = heartBeat.Timestamp + " " + heartBeat.Message;
            };
        }
    }
}
