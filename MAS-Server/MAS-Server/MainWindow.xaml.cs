using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MAS_Server
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MasListener listener;
        private Thread listenerThread;

        public MainWindow()
        {
            InitializeComponent();
            listener = new MasListener(ref this.CommandLog);

            /*
            listenerThread = new Thread(listener.StartListening);
            listenerThread.IsBackground = true; // background threads will be close on app exit
            listenerThread.Start();
             */
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ExecuteCommand();
            }
        }

        private void ExecuteCommand()
        {
            if (CommandInput.Text.Length == 0) return; // prevent empty commands

            if (CommandInput.Text.ToUpper().Equals("EXIT")) Application.Current.Shutdown();
            else listener.SendCommand(CommandInput.Text);

            CommandLog.AppendText("> " + CommandInput.Text + "\r\n");
            CommandInput.Clear();
        }
    }
}
