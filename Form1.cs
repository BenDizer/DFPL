using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using Reloaded.Injector;

namespace DFPl
{
    public partial class Form1 : Form
    {
        // Constants
        private const string Version = "DFPL 0.1.1";
        private const string GameExe = "Dwarf Fortress.exe";
        private const string TranslationDll = "df-steam-translate-hook.dll";
        private const string QuietStartArg = "-quiet_start";
        private const string GamePathSetting = "Gamepath";
        private const string ShortcutName = "Запуск DF.lnk";
        private const string ShortcutIcon = "ico\\DFPLQS.ico";

        // Fields
        private readonly string[] _args;
        private string _gamePath;

        public Form1(string[] args)
        {
            _args = args;
            InitializeComponent();
            textBox2.Text = DFPl.Properties.Settings.Default[GamePathSetting].ToString();
            this.Text = Version;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DFPl.Properties.Settings.Default[GamePathSetting] = textBox2.Text;
            DFPl.Properties.Settings.Default.Save();
            StartGame();
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                CreateQuietStartShortcut();
            }
            catch (Exception ex)
            {
                LogError("Failed to create quiet start shortcut", ex);
                MessageBox.Show("Failed to create quiet start shortcut!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox2.Text = folderBrowserDialog1.SelectedPath;
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            DFPl.Properties.Settings.Default[GamePathSetting] = textBox2.Text;
            DFPl.Properties.Settings.Default.Save();
        }
        
        private void StartGame()
        {
            try
            {
                _gamePath = DFPl.Properties.Settings.Default[GamePathSetting].ToString();
                if (!CheckGameFilesExist())
                {
                    MessageBox.Show("Required game files not found!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(_gamePath, GameExe),
                    CreateNoWindow = false,
                    UseShellExecute = true,
                    WorkingDirectory = _gamePath
                };
                Process df = Process.Start(startInfo);
                int procId = df.Id;
                try
                {
                    Injector injector = new Injector(df);
                    injector.Inject(Path.Combine(_gamePath, TranslationDll));
                    injector.Dispose();
                }
                catch (Exception ex)
                {
                    LogError("Failed to apply mods", ex);
                    MessageBox.Show("Failed to apply mods!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to start game", ex);
                MessageBox.Show("Failed to start game!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool CheckGameFilesExist()
        {
            return System.IO.File.Exists(Path.Combine(_gamePath, GameExe)) &&
                   System.IO.File.Exists(Path.Combine(_gamePath, TranslationDll));
        }

        private void CreateQuietStartShortcut()
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + $@"\{ShortcutName}";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = Environment.CurrentDirectory + @"\DFPL.exe";
            shortcut.Arguments = QuietStartArg;
            shortcut.IconLocation = Path.Combine(Environment.CurrentDirectory, ShortcutIcon);
            shortcut.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Array.IndexOf(_args, QuietStartArg) > -1)
            {
                StartGame();
                Application.Exit();
            }
        }

        private static void LogError(string message, Exception ex)
        {
            // TODO: Implement error logging using a logging library such as log4net
        }
    }
}
