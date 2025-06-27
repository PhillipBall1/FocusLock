using FocusLock.Service;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FocusLock.Controls
{
    // Sidebar user control that supports navigation and live focus mode updates
    public partial class Sidebar : UserControl, INotifyPropertyChanged
    {
        private bool _focusActive;      // True if Focus Mode is active    
        private DispatcherTimer timer; // Timer to keep checking focus mode status
        public event PropertyChangedEventHandler PropertyChanged; // Required for property change notifications

        // Property that tracks whether Focus Mode is currently active
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

        // Constructor: initializes component and starts a timer to poll Focus Mode status
        public Sidebar()
        {
            InitializeComponent();

            // Set up timer to check focus mode status every 100ms
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += (tick, eventArgs) =>
            {
                FocusActive = FocusService.IsFocusModeActive;
            };
            timer.Start();
        }

        // Dependency property to track the currently selected page/view name
        public static readonly DependencyProperty CurrentPageNameProperty =
            DependencyProperty.Register("CurrentPageName", typeof(string), typeof(Sidebar), new PropertyMetadata("", OnCurrentPageNameChanged));

        public string CurrentPageName
        {
            get => (string)GetValue(CurrentPageNameProperty);
            set => SetValue(CurrentPageNameProperty, value);
        }

        // Notifies the view when the current page changes
        private static void OnCurrentPageNameChanged(DependencyObject bar, DependencyPropertyChangedEventArgs e)
        {
            var sidebar = (Sidebar)bar;
            sidebar.PropertyChanged?.Invoke(sidebar, new PropertyChangedEventArgs(nameof(CurrentPageName)));
        }

        // Commands to switch between different views in the app
        // Dependency properties for view switch commands (Dashboard, Focus, Tasks, etc.)

        #region Commands

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
        #endregion
    }
}
