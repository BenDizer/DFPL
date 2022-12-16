using System;
using System.Diagnostics;
using System.IO;
using IWshRuntimeLibrary;
using NLog;
using Reloaded.Injector;

namespace DFPl.Models
{
    public class Game
    {
        // Constants
        private const string GameExe = "Dwarf Fortress.exe";
        private const string TranslationDll = "df-steam-translate-hook.dll";

        // Fields
        private readonly string _gamePath;
        private readonly Logger _logger;

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

                    Injector injector = new Injector(df);
                    injector.Inject(Path.Combine(_gamePath, TranslationDll));
                    injector.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to apply mods", ex);
                    throw new Exception("Failed to apply mods!", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start game", ex);
                throw new Exception("Failed to start game!", ex);
            }
        }

        private bool CheckGameFilesExist()
        {
            return System.IO.File.Exists(Path.Combine(_gamePath, GameExe)) &&
                   System.IO.File.Exists(Path.Combine(_gamePath, TranslationDll));
        }
    }
}
