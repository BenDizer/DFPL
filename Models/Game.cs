using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using NLog;
using Reloaded.Injector;
using Reloaded.Injector.Interop.Structures;
using Reloaded.Injector.Interop;

namespace DFPl.Models
{
    public class Game
    {
        // Constants
        private const string GameExe = "Dwarf Fortress.exe";
        private const string TranslationDll = "df-steam-translate-hook.dll";
        private const string GameInfoPath = @"\data\vanilla\vanilla_creatures_graphics\info.txt";

        // Fields
        private readonly string _gamePath;
        private readonly Logger _logger;

        static private bool LocT = false;
        static public Injector injector;
        static public string injectorP;
        static public string _injectionDllPath;
        public Game(string gamePath, Logger logger)
        {
            _gamePath = gamePath;
            _logger = logger;
        }

        public void Start()
        {
            try
            {
                _logger.Info("Starting Game. Path: " + _gamePath);

                if (!CheckGameFilesExist())
                {

                    LocT = false;
                    _logger.Error("Required game files not found!");
                    throw new Exception("Required game files not found!");
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
                    _logger.Info("Injecting Game");
                    injector = new Injector(df);
                    injectorP = Path.Combine(_gamePath, TranslationDll);
                    injector.Inject(injectorP);
                    //injector.CallFunction(injectorP, "Spam", default);
                    LocT = true;
                }
                catch (Exception ex)
                {

                    LocT = false;
                    _logger.Error("Failed to apply mods", ex);
                    throw new Exception("Failed to apply mods!", ex);
                }
            }
            catch (Exception ex)
            {

                LocT = false;
                _logger.Error("Failed to start game", ex);
                throw new Exception("Failed to start game!", ex);
            }
        }

        private bool CheckGameFilesExist()
        {
            return System.IO.File.Exists(Path.Combine(_gamePath, GameExe)) &&
                   System.IO.File.Exists(Path.Combine(_gamePath, TranslationDll));
        }

        public string DeterminingGameVersion()
        {
            string GV = "";
            if (!CheckGameFilesExist())
            {
                _logger.Error("Required game files not found!");
                throw new Exception("Required game files not found!");
            }
            var secondLine = System.IO.File.ReadLines(_gamePath + GameInfoPath);
            foreach (string lines in secondLine)
            {
                string[] plines = lines.Split(':');
                if (plines[0] == "[DISPLAYED_VERSION")
                {
                    GV = plines[1].Split(']')[0];
                }
            }
            return GV;
        }

        public void INEJ()
        {
            if (LocT)
            {
                Eject();
            }
            else
            {
                Inject();
            }
        }
        public void Eject()
        {
            try
            {
                injector.Eject(_injectionDllPath);
                injector.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to off mods", ex);
                throw new Exception("Failed to off mods!", ex);
            }
        }

        public void Inject()
        {
            try
            {
                injector.Inject(injectorP);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to on mods", ex);
                throw new Exception("Failed to on mods!", ex);
            }
        }

        //public static List<Module> TryGetModules(Process targetProcess, int timeout = 1000)
        //{
        //    List<Module> modules = new List<Module>();
        //    Stopwatch watch = new Stopwatch();
        //    watch.Start();

        //    while (watch.ElapsedMilliseconds < timeout)
        //    {
        //        try
        //        {
        //            modules = ModuleCollector.CollectModules(targetProcess);
        //            break;
        //        }
        //        catch { /* ignored */ }
        //    }

        //    if (modules.Count == 0)
        //        throw new Exception($"Failed to find information on any of the modules inside the process " +
        //                            $"using EnumProcessModulesEx within the { timeout } millisecond timeout. " +
        //                            "The process has likely not yet initialized.");

        //    return modules;
        //}
    }
}
