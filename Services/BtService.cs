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
    private string? _downloadPath;

        public async Task<bool> InitAsync(string downloadPath)
        {
            _downloadPath = downloadPath;
            var settings = new EngineSettings();
            _engine = new ClientEngine(settings);
            return true;
        }

        public async Task<bool> StartTorrentAsync(string torrentFilePath)
        {
            if (_engine == null || _downloadPath == null) return false;
            var torrent = await Torrent.LoadAsync(torrentFilePath);
            _manager = await _engine.AddAsync(torrent, _downloadPath);
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
