using NLog;
using NLog.Config;
using Prism.Models;
using Prism.Services;
using ILogger = NLog.ILogger;

namespace Prism
{
    public partial class Main : Form
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private CancellationTokenSource _cancellationToken;
        public Main()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? throw new Exception("Icon not found");
            Load += (_, _) =>
            {
                Hide();
                ShowInTaskbar = false;
            };
            FormClosing += (_, e) =>
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
            };
            Resize += (_, e) =>
            {
                if (WindowState != FormWindowState.Minimized) return;
                Hide();
                ShowInTaskbar = false;
            };
            Deactivate += (_, _) =>
            {
                Hide();
                ShowInTaskbar = false;
            };

            NotifyIcon notifyIcon = new()
            {
                Icon = Icon,
                Visible = true,
                Text = @"Prism",
                ContextMenuStrip = new ContextMenuStrip()
            };
            notifyIcon.ContextMenuStrip.Items.Add("Show", null, (_, _) =>
            {
                ShowInTaskbar = true;
                Show();
            }); 
            notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (_, _) => Environment.Exit(0));

            LoggerTarget.LogListBox = LoggingSink;

            var config = new LoggingConfiguration();
            var listBoxTarget = new LoggerTarget { Layout = "${level} | ${message}" };
            config.AddTarget("listbox", listBoxTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, listBoxTarget));
            LogManager.Configuration = config;

            _cancellationToken = new CancellationTokenSource();

            var prismService = new PrismService(logger);
            Task.Run(() => prismService.StartAsync(_cancellationToken.Token));

            ControlButton.Text = "\uE769";
        }

        private void ChangeState(object sender, EventArgs e)
        {
            if (ControlButton.Text == "\uE769")
            {
                ControlButton.Text = "\uE768";
                _cancellationToken.Cancel();
            }
            else
            {
                ControlButton.Text = "\uE769";
                _cancellationToken = new CancellationTokenSource();
                var prismService = new PrismService(logger);
                Task.Run(() => prismService.StartAsync(_cancellationToken.Token));
            }
        }
    }
}
