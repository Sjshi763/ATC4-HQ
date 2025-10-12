using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ATC4_HQ.Services
{
    public class BtService
    {
        private ClientEngine? _engine;
        private TorrentManager? _manager;

        public async Task<bool> InitAsync(string downloadPath)
        {
            var settings = new EngineSettingsBuilder
            {
                AllowedEncryption = EncryptionTypes.All,
                ListenPort = 55123,
                SavePath = downloadPath
            }.ToSettings();
            _engine = new ClientEngine(settings);
            return true;
        }

        public async Task<bool> StartTorrentAsync(string torrentFilePath)
        {
            if (_engine == null) return false;
            var torrent = await Torrent.LoadAsync(torrentFilePath);
            _manager = new TorrentManager(torrent, _engine.Settings.SavePath, new TorrentSettings());
            await _engine.Register(_manager);
            await _manager.StartAsync();
            return true;
        }

        public async Task StopTorrentAsync()
        {
            if (_manager != null)
            {
                await _manager.StopAsync();
            }
        }

        public double GetProgress()
        {
            return _manager?.Progress ?? 0.0;
        }
    }
}
