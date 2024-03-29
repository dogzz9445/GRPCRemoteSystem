﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using RemoteSystem.Proto;
using RemoteSystemManager.ViewModel;

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
                // ------------------
                // 초기화
                // ------------------
                // 아이콘 초기화
                TrayTaskbarIcon.Icon = new System.Drawing.Icon(@"Graphicloads.ico");

                // ------------------
                // 이벤트 등록
                // ------------------

                // 메인 메뉴 트레이 클릭시 / 윈도우 비활성화
                //ButtonTray.Click += (s, e) => this.Hide();
                // 윈도우 어디를 선택해도 이동 가능
                //MouseLeftButtonDown += (s, e) => this.DragMove();
                // 응용프로그램 종료시 / 아이콘 제거
                System.Windows.Application.Current.Exit += (s, e) => TrayTaskbarIcon.Dispose();
                // 응용프로그램 종료 버튼 클릭시 / 윈도우 비활성화
                Closing += (s, e) => { e.Cancel = true; this.Hide(); };
                // 메인 팝업 메뉴 트레이 클릭시 / 윈도우 비활성화
                //MainMenuItemTray.Click += (s, e) => this.Hide();
                // 메인 팝업 메뉴 종료 클릭시 / 응용프로그램 종료
                //MainMenuItemClose.Click += (s, e) => System.Windows.Application.Current.Shutdown();
                // 아이콘 더블클릭시 / 윈도우 활성화
                TrayTaskbarIcon.TrayMouseDoubleClick += (s, e) => this.Show();
                // 태스크바 아이콘 설정버튼 클릭시 / 윈도우 활성화
                (TrayTaskbarIcon.ContextMenu.Items.GetItemAt(0) as MenuItem).Click += (s, e) => this.Show();
                // 태스크바 아이콘 종료버튼 클릭시 / 응용프로그램 종료
                (TrayTaskbarIcon.ContextMenu.Items.GetItemAt(2) as MenuItem).Click += (s, e) => System.Windows.Application.Current.Shutdown();

                //TextBlockLog.Text = heartBeat.Timestamp + " " + heartBeat.Message;
            };
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComputerViewModel.Instance.UpdateManagedProgramNames();

            switch (((sender as TabControl)?.SelectedItem as TabItem)?.Tag.ToString())
            {
                case "HomePage":
                    ComputerViewModel.Instance.MonitoringTimer.Elapsed += new ElapsedEventHandler(ComputerViewModel.Instance.RunMonitoringComputer);
                    ComputerViewModel.Instance.IsComputerStatusCheck = true;
                    //ComputerViewModel.Instance.IsMacAddressCheck = true;
                    break;
                case "ComputerSettingPage":
                    ComputerViewModel.Instance.MonitoringTimer.Elapsed -= new ElapsedEventHandler(ComputerViewModel.Instance.RunMonitoringComputer);
                    break;
                case "DistributionPage":
                    break;
            }
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMaximized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
