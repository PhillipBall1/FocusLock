using FocusLock.Service;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FocusLock.Controls
{
    public partial class Sidebar : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private bool _focusActive;

        public bool FocusActive
        {
            get => _focusActive;
            set
            {
                if (_focusActive != value)
                {
                    _focusActive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FocusActive)));
                }
            }
        }

        public Sidebar()
        {
            InitializeComponent();

            FocusActive = FocusService.IsFocusModeActive;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (tick, eventArgs) =>
            {
                FocusActive = FocusService.IsFocusModeActive;
            };
            _timer.Start();
        }

        public static readonly DependencyProperty CurrentPageNameProperty =
            DependencyProperty.Register("CurrentPageName", typeof(string), typeof(Sidebar), new PropertyMetadata("", OnCurrentPageNameChanged));

        public string CurrentPageName
        {
            get => (string)GetValue(CurrentPageNameProperty);
            set => SetValue(CurrentPageNameProperty, value);
        }

        private static void OnCurrentPageNameChanged(DependencyObject bar, DependencyPropertyChangedEventArgs e)
        {
            var sidebar = (Sidebar)bar;
            sidebar.PropertyChanged?.Invoke(sidebar, new PropertyChangedEventArgs(nameof(CurrentPageName)));
        }

        public static readonly DependencyProperty SwitchToDashboardCommandProperty =
            DependencyProperty.Register("SwitchToDashboardCommand", typeof(ICommand), typeof(Sidebar));

        public ICommand SwitchToDashboardCommand
        {
            get => (ICommand)GetValue(SwitchToDashboardCommandProperty);
            set => SetValue(SwitchToDashboardCommandProperty, value);
        }

        public static readonly DependencyProperty SwitchToFocusCommandProperty =
            DependencyProperty.Register("SwitchToFocusCommand", typeof(ICommand), typeof(Sidebar));

        public ICommand SwitchToFocusCommand
        {
            get => (ICommand)GetValue(SwitchToFocusCommandProperty);
            set => SetValue(SwitchToFocusCommandProperty, value);
        }

        public static readonly DependencyProperty SwitchToTasksCommandProperty =
           DependencyProperty.Register("SwitchToTasksCommand", typeof(ICommand), typeof(Sidebar));

        public ICommand SwitchToTasksCommand
        {
            get => (ICommand)GetValue(SwitchToTasksCommandProperty);
            set => SetValue(SwitchToTasksCommandProperty, value);
        }

        public static readonly DependencyProperty SwitchToSettingsCommandProperty =
            DependencyProperty.Register("SwitchToSettingsCommand", typeof(ICommand), typeof(Sidebar));

        public ICommand SwitchToSettingsCommand
        {
            get => (ICommand)GetValue(SwitchToSettingsCommandProperty);
            set => SetValue(SwitchToSettingsCommandProperty, value);
        }

        public static readonly DependencyProperty SwitchToDistractionsCommandProperty =
            DependencyProperty.Register("SwitchToDistractionsCommand", typeof(ICommand), typeof(Sidebar));

        public ICommand SwitchToDistractionsCommand
        {
            get => (ICommand)GetValue(SwitchToDistractionsCommandProperty);
            set => SetValue(SwitchToDistractionsCommandProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
