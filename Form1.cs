using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using Reloaded.Injector;
using IWshRuntimeLibrary;
using System.IO.Compression;

namespace DFPl
{
    public partial class Form1 : Form
    {
        string Version = "DFPL 0.1.0";
        public Form1(string[] Ags)
        {
            bool SL = false;
            foreach (string A in Ags)
            {
                if (A == "-quiet_start")
                SL = true;
            }
            if (SL)
            {
                DFSTART();
                Application.Exit();
            }
            else
            {
                InitializeComponent();
            }
            textBox2.Text = DFPl.Properties.Settings.Default["Gamepath"].ToString();
            this.Text = Version;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            textBox2.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DFPl.Properties.Settings.Default["Gamepath"] = textBox2.Text;
            DFPl.Properties.Settings.Default.Save();
            DFSTART();
        }
        private void DFSTART()
        {
            string GP = DFPl.Properties.Settings.Default["Gamepath"].ToString();
            if (System.IO.File.Exists(GP + "\\Dwarf Fortress.exe"))
            {
                String strDLLName = GP + "\\df-steam-translate-hook.dll";
                if (System.IO.File.Exists(GP + "\\df-steam-translate-hook.dll"))
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = GP + "\\Dwarf Fortress.exe";
                        startInfo.CreateNoWindow = false;
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = GP;
                        Process DF = Process.Start(startInfo);
                        int ProcID = DF.Id;
                        try
                        {
                            Injector injector = new Injector(DF);
                            injector.Inject(strDLLName);
                            injector.Dispose();
                        }
                        catch
                        {
                            MessageBox.Show("Неудалось применить моды!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось запустить игру!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
                else
                {
                    MessageBox.Show("Перевод не найден!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Файл игры не найден!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Запуск DF.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = Environment.CurrentDirectory + @"\DFPL.exe";
            shortcut.Arguments = "-quiet_start";
            shortcut.IconLocation = Environment.CurrentDirectory + @"\ico\DFPLQS.ico";
            shortcut.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DFPl.Properties.Settings.Default["Gamepath"] = textBox2.Text;
            DFPl.Properties.Settings.Default.Save();
            string GP = DFPl.Properties.Settings.Default["Gamepath"].ToString();
            if (System.IO.File.Exists(GP + "\\Dwarf Fortress.exe"))
            {
                string NMLOC = "";
                switch (listBox1.SelectedItem.ToString())
                {
                    case ("Dwarf Fortress v50.02 - Локализация DFRUS"):
                        NMLOC = "Dwarf Fortress v50.02__v1_DFRUS.zip";
                        break;
                    case ("Dwarf Fortress v50.03 - Локализация DFRUS"):
                        NMLOC = "Dwarf Fortress v50.03__v1_DFRUS.zip";
                        break;
                }
                if (NMLOC != "")
                {
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(Environment.CurrentDirectory + @"\loc\" + NMLOC))
                        {
                            foreach (var archiveEntry in archive.Entries)
                            {
                                string fullPath = Path.Combine(GP, archiveEntry.FullName);
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
                        MessageBox.Show("Локализация установлена!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Неудалось установить локализацию!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Необходимо выбрать версию!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Файл игры не найден!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
