using System.Net.Http;
using System.Text.Json;
using Installer.Models;

namespace Installer.Core
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly string _baseManifestUrl;

        public ApiClient(string baseManifestUrl)
        {
            _http = new HttpClient();
            _baseManifestUrl = baseManifestUrl.TrimEnd('/') + "/UpdateManifest.json";
        }

        public async Task<List<ComponentInfo>> GetManifestAsync(CancellationToken ct = default)
        {
            var res = await _http.GetAsync(_baseManifestUrl, ct);
            res.EnsureSuccessStatusCode();
            using var s = await res.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);
            var root = doc.RootElement;
            var comps = root.GetProperty("components");
            var list = new List<ComponentInfo>();
            foreach (var item in comps.EnumerateArray())
            {
                var ci = JsonSerializer.Deserialize<ComponentInfo>(item.GetRawText())!;
                list.Add(ci);
            }
            return list;
        }

        public async Task<ComponentInfo?> GetComponentByIdAsync(string id, CancellationToken ct = default)
        {
            var all = await GetManifestAsync(ct);
            return all.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
