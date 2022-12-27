using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using DFPl.Models;
using IWshRuntimeLibrary;
using NLog;
using Reloaded.Injector;

namespace DFPl
{
    public partial class Form1 : Form
    {
        // Constants
        private const string Version = "DFPL 0.1.3";
        private const string GamePathSetting = "Gamepath";
        private const string ShortcutName = "Запуск DF.lnk";
        private const string ShortcutIcon = "ico\\DFPLQS.ico";
        private const string QuietStartArg = "-quiet_start";
        private const string GameVersionStr = "@Версия игры ";
        private const string GameVersionStrN = "неизвестна";

        // Fields
        private readonly string[] _args;
        private readonly Logger _logger;



        public Form1(string args = "")
        {
            _args = args.Split('|');
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("App started with args: " + String.Join(", ", args));

            InitializeComponent();
            textBox2.Text = DFPl.Properties.Settings.Default[GamePathSetting].ToString();
            this.Text = Version;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DFPl.Properties.Settings.Default[GamePathSetting] = textBox2.Text;
            DFPl.Properties.Settings.Default.Save();

            try
            {
                var game = new Game(DFPl.Properties.Settings.Default[GamePathSetting].ToString(), _logger);
                game.Start();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start game", ex);
                MessageBox.Show("Failed to start game!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                CreateQuietStartShortcut();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to create quiet start shortcut", ex);
                MessageBox.Show("Failed to create quiet start shortcut!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox2.Text = folderBrowserDialog1.SelectedPath;
            DFPl.Properties.Settings.Default[GamePathSetting] = folderBrowserDialog1.SelectedPath;
            DFPl.Properties.Settings.Default.Save();
            _logger.Info("New path set and saved. Path: " + DFPl.Properties.Settings.Default[GamePathSetting].ToString());
            var game = new Game(DFPl.Properties.Settings.Default[GamePathSetting].ToString(), _logger);
            string GV = game.DeterminingGameVersion();
            if (GV != "")
                label1.Text = GameVersionStr + GV;
            else
                label1.Text = GameVersionStr + GameVersionStrN;
        }


        // TODO
        // 1. Download localization from the server
        // 2. Request avalible versions from the server
        private void button4_Click(object sender, EventArgs e)
        {
            string gamePath = textBox2.Text;
            DFPl.Properties.Settings.Default["Gamepath"] = gamePath;
            DFPl.Properties.Settings.Default.Save();

            if (!System.IO.File.Exists(gamePath + "\\Dwarf Fortress.exe"))
            {
                _logger.Error("Game file not found at path: " + gamePath);
                MessageBox.Show("Game file not found at path!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string localizationFile = "";

            switch (listBox1.SelectedItem?.ToString())
            {
                case ("Dwarf Fortress v50.02 - Локализация DFRUS v0.1"):
                    localizationFile = "Dwarf Fortress v50.02__v0.1_DFRUS.zip";
                    break;
                case ("Dwarf Fortress v50.03 - Локализация DFRUS v0.1"):
                    localizationFile = "Dwarf Fortress v50.03__v0.1_DFRUS.zip";
                    break;
                case ("Dwarf Fortress v50.03 - Локализация DFRUS v0.2"):
                    localizationFile = "Dwarf Fortress v50.03__v0.2_DFRUS.zip";
                    break;
                case ("Переводы из мастерской - UNIT"):
                    localizationFile = "WorkshopMod.zip";
                    break;
                case ("Dwarf Fortress v50.03 - Локализация DFRUS v0.3"):
                    localizationFile = "Dwarf Fortress v50.03__v0.3_DFRUS.zip";
                    break;
            }

            if (localizationFile == "")
            {
                _logger.Error("No localization file selected");
                MessageBox.Show("No localization file selected!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string localizationArchivePath = Environment.CurrentDirectory + @"\loc\" + localizationFile;
            if (!System.IO.File.Exists(localizationArchivePath))
            {
                _logger.Error("Localization archive not found at path: " + localizationArchivePath);
                MessageBox.Show("Localization archive not found!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(localizationArchivePath))
                {
                    foreach (var archiveEntry in archive.Entries)
                    {
                        string fullPath = Path.Combine(gamePath, archiveEntry.FullName);
                        if (archiveEntry.Name == "")
                        {
                            Directory.CreateDirectory(fullPath);
                        }
                        else
                        {
                            archiveEntry.ExtractToFile(fullPath, true);
                        }
                    }
                }

                _logger.Info("Localization installed at path: " + gamePath);
                MessageBox.Show("Localization installed!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error("Error installing localization", ex);
                MessageBox.Show("Error installing localization!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CreateQuietStartShortcut()
        {
            string appPath = Application.StartupPath;
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ShortcutName);
            string exePath = Path.Combine(appPath, "DFPl.exe");

            if (System.IO.File.Exists(shortcutPath))
            {
                System.IO.File.Delete(shortcutPath);
            }

            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.Description = "DFPl Quiet Start";
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = appPath;
            shortcut.Arguments = QuietStartArg;
            shortcut.IconLocation = Path.Combine(appPath, ShortcutIcon);
            shortcut.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //globalKeyboardHook gkh = new globalKeyboardHook();
            ////Keys[] keys = {Keys.LShiftKey, Keys.F5};
            //gkh.HookedKeys.Add(Keys.PageDown);
            //gkh.KeyUp += new KeyEventHandler(gkh_KeyUp);
            if (_args.Length > 0 && _args[0] == QuietStartArg)
            {
                DFPl.Properties.Settings.Default[GamePathSetting] = textBox2.Text;
                DFPl.Properties.Settings.Default.Save();

                try
                {
                    var game = new Game(DFPl.Properties.Settings.Default[GamePathSetting].ToString(), _logger);
                    game.Start();
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to start game", ex);
                    MessageBox.Show("Failed to start game!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //Close();
            }
        }

        private void gkh_KeyUp(object sender, KeyEventArgs e)
        {
            var game = new Game(DFPl.Properties.Settings.Default[GamePathSetting].ToString(), _logger);
            game.INEJ();
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            var game = new Game(DFPl.Properties.Settings.Default[GamePathSetting].ToString(), _logger);
            game.INEJ();
        }
    }
}
