using System.Text;
namespace Installer.Core
{
    public class IniEditor
    {
        private readonly string _path;
        public IniEditor(string path)
        {
            _path = path;
        }

        public void Set(string section, string key, string value)
        {
            var lines = new List<string>();
            if (File.Exists(_path))
                lines = File.ReadAllLines(_path).ToList();

            var sb = new StringBuilder();
            bool inSection = false;
            bool updated = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    inSection = string.Equals(line.Trim('[',']'), section, StringComparison.OrdinalIgnoreCase);
                    sb.AppendLine(line);
                    continue;
                }

                if (inSection && line.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                {
                    sb.AppendLine(key + "=" + value);
                    updated = true;
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (!updated)
            {
                if (!lines.Any(l => l.TrimStart().StartsWith("[")))
                {
                    sb.AppendLine("[" + section + "]");
                }
                sb.AppendLine(key + "=" + value);
            }

            File.WriteAllText(_path, sb.ToString());
        }
    }
}
