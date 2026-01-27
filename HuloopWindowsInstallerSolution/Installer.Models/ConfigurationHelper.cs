using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Installer.Models
{
    /// <summary>
    /// Helper class to flatten nested configuration dictionaries into key-value pairs
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Flattens a nested configuration dictionary into a flat list of key-value pairs
        /// Example: {"AppSettings": {"Version": "1.0"}} becomes "AppSettings.Version" = "1.0"
        /// Arrays: {"Triggers": [{"Type": "A"}]} becomes "Triggers[0].Type" = "A"
        /// </summary>
        public static Dictionary<string, string> FlattenConfiguration(Dictionary<string, object> configuration, string parentKey = "")
        {
            var result = new Dictionary<string, string>();

            foreach (var kvp in configuration)
            {
                string key = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}.{kvp.Key}";

                if (kvp.Value == null)
                {
                    result[key] = string.Empty;
                }
                else if (kvp.Value is string stringValue)
                {
                    result[key] = stringValue;
                }
                else if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    // Recursively flatten nested objects
                    var nested = FlattenConfiguration(nestedDict, key);
                    foreach (var nestedKvp in nested)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else if (kvp.Value is List<object> list)
                {
                    // Flatten arrays with index notation
                    for (int i = 0; i < list.Count; i++)
                    {
                        string arrayKey = $"{key}[{i}]";

                        if (list[i] is Dictionary<string, object> dictItem)
                        {
                            // Recursively flatten object within array
                            var nested = FlattenConfiguration(dictItem, arrayKey);
                            foreach (var nestedKvp in nested)
                            {
                                result[nestedKvp.Key] = nestedKvp.Value;
                            }
                        }
                        else if (list[i] is string stringItem)
                        {
                            result[arrayKey] = stringItem;
                        }
                        else if (list[i] != null)
                        {
                            result[arrayKey] = list[i].ToString() ?? string.Empty;
                        }
                        else
                        {
                            result[arrayKey] = string.Empty;
                        }
                    }
                }
                else if (kvp.Value is JsonElement jsonElement)
                {
                    // Handle JsonElement types from deserialization
                    result[key] = FlattenJsonElement(jsonElement, key, result);
                }
                else
                {
                    // For other types (numbers, booleans), convert to string
                    result[key] = kvp.Value.ToString() ?? string.Empty;
                }
            }

            return result;
        }

        private static string FlattenJsonElement(JsonElement element, string key, Dictionary<string, string> result)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;
                case JsonValueKind.Number:
                    return element.GetRawText();
                case JsonValueKind.True:
                    return "true";
                case JsonValueKind.False:
                    return "false";
                case JsonValueKind.Null:
                    return string.Empty;
                case JsonValueKind.Object:
                    var objDict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        objDict[prop.Name] = JsonElementToObject(prop.Value);
                    }
                    var nested = FlattenConfiguration(objDict, key);
                    foreach (var kvp in nested)
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                    return string.Empty;
                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        string arrayKey = $"{key}[{index}]";
                        if (item.ValueKind == JsonValueKind.Object)
                        {
                            var itemDict = new Dictionary<string, object>();
                            foreach (var prop in item.EnumerateObject())
                            {
                                itemDict[prop.Name] = JsonElementToObject(prop.Value);
                            }
                            var nestedArray = FlattenConfiguration(itemDict, arrayKey);
                            foreach (var kvp in nestedArray)
                            {
                                result[kvp.Key] = kvp.Value;
                            }
                        }
                        else
                        {
                            result[arrayKey] = item.GetRawText();
                        }
                        index++;
                    }
                    return string.Empty;
                default:
                    return element.GetRawText();
            }
        }

        private static object JsonElementToObject(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out long longVal))
                        return longVal;
                    return element.GetDouble();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null!;
                case JsonValueKind.Object:
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in element.EnumerateObject())
                    {
                        dict[prop.Name] = JsonElementToObject(prop.Value);
                    }
                    return dict;
                case JsonValueKind.Array:
                    var list = new List<object>();
                    foreach (var item in element.EnumerateArray())
                    {
                        list.Add(JsonElementToObject(item));
                    }
                    return list;
                default:
                    return element.GetRawText();
            }
        }

        /// <summary>
        /// Unflattens a flat dictionary back into a nested structure
        /// Example: "AppSettings.Version" = "1.0" becomes {"AppSettings": {"Version": "1.0"}}
        /// Arrays: "Triggers[0].Type" = "A" becomes {"Triggers": [{"Type": "A"}]}
        /// </summary>
        public static Dictionary<string, object> UnflattenConfiguration(Dictionary<string, string> flatConfig)
        {
            var result = new Dictionary<string, object>();

            foreach (var kvp in flatConfig)
            {
                SetNestedValue(result, kvp.Key, kvp.Value);
            }

            return result;
        }

        private static void SetNestedValue(Dictionary<string, object> dict, string path, string value)
        {
            var parts = SplitPath(path);
            Dictionary<string, object> current = dict;

            for (int i = 0; i < parts.Count - 1; i++)
            {
                var part = parts[i];

                if (part.IsArrayIndex)
                {
                    // This shouldn't happen as array indices are part of the key
                    continue;
                }

                string nextKey = part.Key;
                var nextPart = parts[i + 1];

                if (!current.ContainsKey(nextKey))
                {
                    if (nextPart.IsArrayIndex)
                    {
                        // Next part is array, create list
                        current[nextKey] = new List<object>();
                    }
                    else
                    {
                        // Next part is object, create dictionary
                        current[nextKey] = new Dictionary<string, object>();
                    }
                }

                if (nextPart.IsArrayIndex)
                {
                    // Handle array
                    var list = (List<object>)current[nextKey];

                    // Ensure list has enough items
                    while (list.Count <= nextPart.Index)
                    {
                        list.Add(new Dictionary<string, object>());
                    }

                    // If there are more parts after the array index, navigate into the array item
                    if (i + 2 < parts.Count)
                    {
                        current = (Dictionary<string, object>)list[nextPart.Index];
                        i++; // Skip the array index part
                    }
                    else
                    {
                        // This is the final array index, set the value
                        list[nextPart.Index] = ParseValue(value);
                        return;
                    }
                }
                else
                {
                    current = (Dictionary<string, object>)current[nextKey];
                }
            }

            // Set the final value
            var lastPart = parts[^1];
            if (lastPart.IsArrayIndex)
            {
                // Should not happen
                return;
            }

            current[lastPart.Key] = ParseValue(value);
        }

        private static List<PathPart> SplitPath(string path)
        {
            var parts = new List<PathPart>();
            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                if (segment.Contains('['))
                {
                    // Handle array notation: "Triggers[0]"
                    var propName = segment.Substring(0, segment.IndexOf('['));
                    var indexStr = segment.Substring(segment.IndexOf('[') + 1, segment.IndexOf(']') - segment.IndexOf('[') - 1);

                    parts.Add(new PathPart { Key = propName, IsArrayIndex = false });
                    parts.Add(new PathPart { Key = string.Empty, IsArrayIndex = true, Index = int.Parse(indexStr) });
                }
                else
                {
                    parts.Add(new PathPart { Key = segment, IsArrayIndex = false });
                }
            }

            return parts;
        }

        private class PathPart
        {
            public string Key { get; set; } = string.Empty;
            public bool IsArrayIndex { get; set; }
            public int Index { get; set; }
        }

        /// <summary>
        /// Attempts to parse a string value into the appropriate type
        /// </summary>
        private static object ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            // Try boolean
            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }

            // Try integer
            if (long.TryParse(value, out long longValue))
            {
                return longValue;
            }

            // Try double
            if (double.TryParse(value, out double doubleValue))
            {
                return doubleValue;
            }

            // Try JSON array or object
            if ((value.StartsWith("[") && value.EndsWith("]")) ||
                 (value.StartsWith("{") && value.EndsWith("}")))
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(value);
                }
                catch
                {
                    // If parsing fails, return as string
                }
            }

            return value;
        }

        /// <summary>
        /// Gets a display-friendly label for a configuration key
        /// Example: "AppSettings.Triggers[0].RunSchedule" becomes "Triggers [Item 0] Run Schedule"
        /// </summary>
        public static string GetDisplayLabel(string key)
        {
            // Remove parent prefixes for cleaner display
            var parts = key.Split('.');
            if (parts.Length > 1)
            {
                // Skip the root level (e.g., "AppSettings")
                parts = parts.Skip(1).ToArray();
            }

            return string.Join(" > ", parts);
        }

        /// <summary>
        /// Checks if a configuration key represents an array item
        /// </summary>
        public static bool IsArrayItem(string key)
        {
            return key.Contains("[") && key.Contains("]");
        }

        /// <summary>
        /// Gets the original nested path for display (e.g., "AppSettings > Triggers > Item 0 > RunSchedule")
        /// </summary>
        public static string GetHierarchicalDisplay(string flatKey)
        {
            var parts = flatKey.Split('.');
      var display = new List<string>();

            foreach (var part in parts)
    {
     // Handle array indices
    if (part.Contains("[") && part.Contains("]"))
   {
        var propName = part.Substring(0, part.IndexOf('['));
   var index = part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1);
   display.Add($"{propName} [Item {index}]");
       }
  else
      {
      display.Add(part);
   }
       }

      return string.Join(" > ", display);
        }

        /// <summary>
 /// Splits a flattened key into individual parts
        /// Example: "AppSettings.Triggers[0].ExePath" -> ["AppSettings", "Triggers", "[0]", "ExePath"]
        /// </summary>
        public static string[] SplitKey(string flatKey)
        {
            var parts = new List<string>();
   var segments = flatKey.Split('.');

    foreach (var segment in segments)
          {
                if (segment.Contains('['))
                {
                    // Handle array notation: "Triggers[0]"
  var propName = segment.Substring(0, segment.IndexOf('['));
          var indexPart = segment.Substring(segment.IndexOf('['));
    
    parts.Add(propName);
        parts.Add(indexPart);
                }
  else
              {
  parts.Add(segment);
     }
            }

    return parts.ToArray();
        }

        /// <summary>
  /// Checks if a key part is an array index and extracts the array name and index
 /// Example: "[0]" -> true, arrayName="", index=0
        /// Example: "Triggers[0]" -> true, arrayName="Triggers", index=0
        /// </summary>
        public static bool IsArrayIndex(string keyPart, out string arrayName, out int index)
        {
    arrayName = string.Empty;
     index = -1;

            if (!keyPart.Contains('[') || !keyPart.Contains(']'))
         return false;

   try
    {
         int bracketStart = keyPart.IndexOf('[');
int bracketEnd = keyPart.IndexOf(']');

  if (bracketStart > 0)
                {
           arrayName = keyPart.Substring(0, bracketStart);
       }

                string indexStr = keyPart.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
         index = int.Parse(indexStr);
return true;
       }
          catch
            {
     return false;
     }
        }
    }
}
