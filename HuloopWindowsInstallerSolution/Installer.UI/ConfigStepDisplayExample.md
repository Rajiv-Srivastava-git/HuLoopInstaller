# ConfigStep Display Example - All Components

## What You'll See in ConfigStep

### **Component 1: Huloop CLI**

```
? Huloop CLI
  Format: INI | Target: Config/server.cnf | Mode: Patch

    ai.huloop.server.url: *
    [https://qa.huloop.ai   ]
```

**Fields to configure:**
- `ai.huloop.server.url` - Pre-filled with `https://qa.huloop.ai`
- User can change to their own server URL

---

### **Component 2: Huloop Scheduler**

```
? Huloop Scheduler
  Format: JSON | Target: appsettings.json | Mode: Overwrite

    ApplicationName: *
    [HuLoop.Schedulers   ]

    Version: *
    [1.0.0           ]

    SchedulerId: *
    [    ]

  AppSettings > Triggers [Item 0]

    TriggerType: *
    [Workflow        ]

    RunSchedule: *
    [0 0 0 0 5       ]

    HostApiUrl: *
    [https://qa.huloop.ai:8443/             ]

    ExePath: *
    [..\\..\\HuloopCLI\\HuLoopCLI.exe                  ]

    LogFilePath: *
    [..\\..\\Logs\\Workflow\\            ]

  AppSettings > Triggers [Item 1]

    TriggerType: *
    [Automation       ]

    RunSchedule: *
    [0 0 0 1 0      ]

    HostApiUrl: *
  [https://qa.huloop.ai:8443/     ]

  ExePath: *
    [..\\..\\HuloopCLI\\HuLoopCLI.exe   ]

    LogFilePath: *
    [..\\..\\Logs\\Automation\\]

  AppSettings > Triggers [Item 2]

    TriggerType: *
    [IAB    ]

RunSchedule: *
    [0 0 0 0 10    ]

    HostApiUrl: *
    [https://qa.huloop.ai:8443/       ]

    ExePath: *
    [..\\..\\HuloopCLI\\HuLoopCLI.exe            ]

    LogFilePath: *
    [..\\..\\Logs\\IAB\\        ]
```

**Fields to configure (total: 18 fields):**

1. `ApplicationName` - Pre-filled with `HuLoop.Schedulers`
2. `Version` - Pre-filled with `1.0.0`
3. `SchedulerId` - Empty (user must fill)

**Trigger 0 (Workflow):**
4. `TriggerType` - Pre-filled with `Workflow`
5. `RunSchedule` - Pre-filled with `0 0 0 0 5`
6. `HostApiUrl` - Pre-filled with `https://qa.huloop.ai:8443/`
7. `ExePath` - Pre-filled with `..\\..\\HuloopCLI\\HuLoopCLI.exe`
8. `LogFilePath` - Pre-filled with `..\\..\\Logs\\Workflow\\`

**Trigger 1 (Automation):**
9. `TriggerType` - Pre-filled with `Automation`
10. `RunSchedule` - Pre-filled with `0 0 0 1 0`
11. `HostApiUrl` - Pre-filled with `https://qa.huloop.ai:8443/`
12. `ExePath` - Pre-filled with `..\\..\\HuloopCLI\\HuLoopCLI.exe`
13. `LogFilePath` - Pre-filled with `..\\..\\Logs\\Automation\\`

**Trigger 2 (IAB):**
14. `TriggerType` - Pre-filled with `IAB`
15. `RunSchedule` - Pre-filled with `0 0 0 0 10`
16. `HostApiUrl` - Pre-filled with `https://qa.huloop.ai:8443/`
17. `ExePath` - Pre-filled with `..\\..\\HuloopCLI\\HuLoopCLI.exe`
18. `LogFilePath` - Pre-filled with `..\\..\\Logs\\IAB\\`

---

## How the Data is Flattened

### Original JSON Structure (Scheduler):
```json
{
  "AppSettings": {
    "ApplicationName": "HuLoop.Schedulers",
    "Version": "1.0.0",
    "SchedulerId": "",
    "Triggers": [
      {
        "TriggerType": "Workflow",
        "RunSchedule": "0 0 0 0 5",
        "HostApiUrl": "https://qa.huloop.ai:8443/",
 "ExePath": "..\\..\\HuloopCLI\\HuLoopCLI.exe",
        "LogFilePath": "..\\..\\Logs\\Workflow\\"
      },
      {
     "TriggerType": "Automation",
        "RunSchedule": "0 0 0 1 0",
        ...
      },
  {
        "TriggerType": "IAB",
        "RunSchedule": "0 0 0 0 10",
   ...
      }
]
  }
}
```

### Flattened Keys:
```
AppSettings.ApplicationName = "HuLoop.Schedulers"
AppSettings.Version = "1.0.0"
AppSettings.SchedulerId = ""
AppSettings.Triggers[0].TriggerType = "Workflow"
AppSettings.Triggers[0].RunSchedule = "0 0 0 0 5"
AppSettings.Triggers[0].HostApiUrl = "https://qa.huloop.ai:8443/"
AppSettings.Triggers[0].ExePath = "..\\..\\HuloopCLI\\HuLoopCLI.exe"
AppSettings.Triggers[0].LogFilePath = "..\\..\\Logs\\Workflow\\"
AppSettings.Triggers[1].TriggerType = "Automation"
AppSettings.Triggers[1].RunSchedule = "0 0 0 1 0"
AppSettings.Triggers[1].HostApiUrl = "https://qa.huloop.ai:8443/"
AppSettings.Triggers[1].ExePath = "..\\..\\HuloopCLI\\HuLoopCLI.exe"
AppSettings.Triggers[1].LogFilePath = "..\\..\\Logs\\Automation\\"
AppSettings.Triggers[2].TriggerType = "IAB"
AppSettings.Triggers[2].RunSchedule = "0 0 0 0 10"
AppSettings.Triggers[2].HostApiUrl = "https://qa.huloop.ai:8443/"
AppSettings.Triggers[2].ExePath = "..\\..\\HuloopCLI\\HuLoopCLI.exe"
AppSettings.Triggers[2].LogFilePath = "..\\..\\Logs\\IAB\\"
```

---

## User Interactions

### 1. **Pre-filled Values**
- All fields show default values from `components.json`
- User can see what the default configuration is

### 2. **Editable Fields**
- User can change any value
- All fields marked with `*` (required)
- Empty fields must be filled before clicking Finish

### 3. **Field Validation**
- Finish button is **gray/disabled** if any field is empty
- Finish button becomes **blue/enabled** when all fields have values
- Real-time validation as user types

### 4. **Saved Values**
After user fills in values and clicks Finish, data is stored in:

```csharp
component.ConfigurationValues = {
    ["AppSettings.ApplicationName"] = "HuLoop.Schedulers",
 ["AppSettings.Version"] = "1.0.0",
    ["AppSettings.SchedulerId"] = "SCHED-001", // User entered
    ["AppSettings.Triggers[0].TriggerType"] = "Workflow",
  ["AppSettings.Triggers[0].RunSchedule"] = "0 0 0 0 5",
    ["AppSettings.Triggers[0].HostApiUrl"] = "https://qa.huloop.ai:8443/",
    ["AppSettings.Triggers[0].ExePath"] = "..\\..\\HuloopCLI\\HuLoopCLI.exe",
    ["AppSettings.Triggers[0].LogFilePath"] = "..\\..\\Logs\\Workflow\\",
    // ... all other fields
}
```

---

## Example: User Changes Values

### Scenario: User wants to change server URL for CLI

**Original:**
```
ai.huloop.server.url: *
[https://qa.huloop.ai    ]
```

**User changes to:**
```
ai.huloop.server.url: *
[https://production.mycompany.com:8080/api    ]
```

**Saved:**
```csharp
cliComponent.ConfigurationValues["ai.huloop.server.url"] = "https://production.mycompany.com:8080/api"
```

### Scenario: User fills in Scheduler ID

**Original:**
```
SchedulerId: *
[]  ? Empty
```

**User fills in:**
```
SchedulerId: *
[PROD-SCHED-001          ]
```

**Saved:**
```csharp
schedulerComponent.ConfigurationValues["AppSettings.SchedulerId"] = "PROD-SCHED-001"
```

---

## Benefits

### ? **All Fields Visible**
- User sees every configuration option
- No hidden or missing fields

### ? **Pre-filled with Defaults**
- User knows what the default values are
- Can accept defaults or customize

### ? **Grouped Display**
- Array items grouped under headers
- Easy to see which trigger is being configured

### ? **Friendly Labels**
- "ApplicationName" instead of "AppSettings.ApplicationName"
- "TriggerType" instead of "AppSettings.Triggers[0].TriggerType"

### ? **Validation**
- Cannot proceed with empty fields
- Clear visual feedback (button state)

### ? **Flexibility**
- User can customize any value
- All fields are editable
- Changes are immediately reflected

---

## Technical Details

### Flattening Process:
```
Original JSON
  ? (DictionaryStringObjectJsonConverter)
Dictionary<string, object> with nested structure
  ? (ConfigurationHelper.FlattenConfiguration)
Dictionary<string, string> with dot notation keys
  ? (ConfigStep UI)
Individual TextBox controls
```

### Array Handling:
```
"Triggers": [
  {"Type": "A", "Value": "1"},
  {"Type": "B", "Value": "2"}
]
  ? Becomes ?
Triggers[0].Type = "A"
Triggers[0].Value = "1"
Triggers[1].Type = "B"
Triggers[1].Value = "2"
```

### Unflattening Process (for saving to file):
```
Dictionary<string, string> with dot notation
  ? (ConfigurationHelper.UnflattenConfiguration)
Dictionary<string, object> with nested structure
  ? (JsonSerializer.Serialize)
appsettings.json file
```

---

## Total Configuration Fields

### CLI Component: **1 field**
- Server URL

### Scheduler Component: **18 fields**
- 3 general settings (ApplicationName, Version, SchedulerId)
- 5 fields per trigger × 3 triggers = 15 fields

### **Total: 19 editable configuration fields**

All fields are:
- ? Displayed to user
- ? Pre-filled with defaults
- ? Editable
- ? Required (must not be empty)
- ? Saved to `component.ConfigurationValues`

---

**Status:** ? Fully Implemented and Working
**Version:** 2.0
**Last Updated:** 2024
