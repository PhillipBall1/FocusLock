using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FocusLock.Helper
{
    public static class NotificationHelper
    {
        private static NotifyIcon _notifyIcon;

        public static void ShowNotification(string title, string message)
        {
            if (_notifyIcon == null)
            {
                _notifyIcon = new System.Windows.Forms.NotifyIcon
                {
                    Visible = true,
                    Icon = System.Drawing.SystemIcons.Information,
                    BalloonTipIcon = ToolTipIcon.Info
                };
            }

            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _notifyIcon.ShowBalloonTip(5000);
            });
        }

    }

}
