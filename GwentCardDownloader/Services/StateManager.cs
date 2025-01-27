using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GwentCardDownloader.Models;

namespace GwentCardDownloader.Services
{
    public class StateManager
    {
        private readonly string _statePath;
        private readonly ConcurrentDictionary<string, CardDownloadState> _state;

        public StateManager(string statePath)
        {
            _statePath = statePath;
            _state = new ConcurrentDictionary<string, CardDownloadState>();
        }

        public async Task SaveStateAsync()
        {
            var json = JsonSerializer.Serialize(_state, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_statePath, json);
        }

        public async Task<Dictionary<string, CardDownloadState>> LoadStateAsync()
        {
            if (!File.Exists(_statePath))
                return new Dictionary<string, CardDownloadState>();

            var json = await File.ReadAllTextAsync(_statePath);
            return JsonSerializer.Deserialize<Dictionary<string, CardDownloadState>>(json);
        }
    }
}
