using Installer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Installer.Core
{
    public static class ComponentCatalogLoader
    {
        public static List<InstallerComponent> Load(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException("Component catalog not found", jsonPath);

            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<InstallerComponent>>(json)!
                   ?? new List<InstallerComponent>();
        }
    }
}
