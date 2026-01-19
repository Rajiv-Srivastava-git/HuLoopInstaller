# ConfigStep - Read-Only TriggerType Field

## Overview

The `TriggerType` field has been made **read-only** to prevent users from accidentally changing the trigger identifier. This field is displayed as a label instead of an editable textbox, with visual distinction to indicate it's informational rather than editable.

## Implementation

### What Changed

**TriggerType Field Detection:**
```csharp
bool isReadOnlyField = displayLabel.Equals("TriggerType", StringComparison.OrdinalIgnoreCase);
```

The system checks if the field label is "TriggerType" and treats it differently.

### Visual Representation

#### **Before (Editable):**
```
  TriggerType:
[Workflow      ] ? User could change this textbox
```

#### **After (Read-Only):**
```
  TriggerType: (Trigger Type)
  [ Workflow ] ? Displayed as bold label with light blue background
```

## Visual Design

### **Read-Only Label Styling:**

```csharp
var valueLabel = new Label
{
    Text = configValue,      // "Workflow", "Automation", or "IAB"
    Location = new Point(290, yPosition),
    AutoSize = true,
    Font = new Font("Segoe UI", 9, FontStyle.Bold),
    ForeColor = Color.FromArgb(0, 122, 204),      // Blue text
    BackColor = Color.FromArgb(240, 248, 255),    // Light blue background
    Padding = new Padding(5, 3, 5, 3),
    BorderStyle = BorderStyle.FixedSingle     // Border for emphasis
};
```

### **Field Label Styling:**

```csharp
Text = "  TriggerType: (Trigger Type)"  // Adds explanatory text
ForeColor = Color.FromArgb(0, 122, 204) // Blue color to match value
```

## Complete Visual Layout

### **Scheduler Component with 3 Triggers:**

```
? Huloop Scheduler
  Format: JSON | Target: appsettings.json | Mode: Overwrite

    ApplicationName:
    [HuLoop.Schedulers    ]

    Version:
    [1.0.0          ]

    SchedulerId:
    [Leave blank for default  ]

  AppSettings > Triggers [Item 0]

    TriggerType: (Trigger Type)
    [ Workflow ]  ? Read-only, light blue background
    
    RunSchedule:
 [0 0 0 0 5     ]
    
    HostApiUrl:
    [https://qa.huloop.ai:8443/   ]
    
    ExePath:
    [..\\..\\HuloopCLI\\HuLoopCLI.exe    ]
    
    LogFilePath:
    [..\\..\\Logs\\Workflow\\      ]

  AppSettings > Triggers [Item 1]

    TriggerType: (Trigger Type)
    [ Automation ]  ? Read-only, light blue background
    
    RunSchedule:
[0 0 0 1 0    ]
    
    HostApiUrl:
    [https://qa.huloop.ai:8443/   ]
    
    ExePath:
    [..\\..\\HuloopCLI\\HuLoopCLI.exe    ]
    
    LogFilePath:
    [..\\..\\Logs\\Automation\\    ]

  AppSettings > Triggers [Item 2]

    TriggerType: (Trigger Type)
    [ IAB ]  ? Read-only, light blue background
    
    RunSchedule:
    [0 0 0 0 10    ]
    
    HostApiUrl:
    [https://qa.huloop.ai:8443/   ]
    
    ExePath:
    [..\\..\\HuloopCLI\\HuLoopCLI.exe    ]
    
    LogFilePath:
    [..\\..\\Logs\\IAB\\       ]
```

## Technical Implementation

### **1. Field Detection**
```csharp
string displayLabel = GetFriendlyLabel(configKey);
bool isReadOnlyField = displayLabel.Equals("TriggerType", StringComparison.OrdinalIgnoreCase);
```

### **2. Label Rendering (Read-Only)**
```csharp
if (isReadOnlyField)
{
    // Display as read-only label
    var valueLabel = new Label
    {
   Text = configValue,
        Location = new Point(290, yPosition),
        AutoSize = true,
        Font = new Font("Segoe UI", 9, FontStyle.Bold),
        ForeColor = Color.FromArgb(0, 122, 204),
        BackColor = Color.FromArgb(240, 248, 255),
        Padding = new Padding(5, 3, 5, 3),
 BorderStyle = BorderStyle.FixedSingle
    };
    scrollPanel.Controls.Add(valueLabel);
    
    // Create hidden textbox to store value for saving
    string uniqueKey = $"{component.ComponentId}:{configKey}";
    var hiddenTextBox = new TextBox
    {
        Text = configValue,
        Visible = false
    };
    configTextBoxes[uniqueKey] = hiddenTextBox;
}
```

### **3. TextBox Rendering (Editable)**
```csharp
else
{
    // Regular editable textbox
    var valueTextBox = new TextBox
    {
 Width = 350,
        Location = new Point(290, yPosition - 2),
   Font = new Font("Segoe UI", 9),
      PlaceholderText = "Leave blank for default"
    };
    
    // ... set value and add to controls
    configTextBoxes[uniqueKey] = valueTextBox;
    scrollPanel.Controls.Add(valueTextBox);
}
```

## Data Handling

### **Hidden TextBox for Value Storage**

Even though TriggerType is displayed as a label, the value is still stored in a hidden textbox:

```csharp
var hiddenTextBox = new TextBox
{
    Text = configValue,  // "Workflow", "Automation", or "IAB"
    Visible = false      // Not shown on screen
};
configTextBoxes[uniqueKey] = hiddenTextBox;
```

This ensures:
- Value is saved when user clicks Finish
- Value is included in `component.ConfigurationValues`
- Value is written to `appsettings.json` unchanged

### **Saved Data Structure**

```csharp
component.ConfigurationValues = {
    ["AppSettings.Triggers[0].TriggerType"] = "Workflow",     // Read-only
  ["AppSettings.Triggers[0].RunSchedule"] = "0 0 0 0 5",    // Editable
    ["AppSettings.Triggers[0].HostApiUrl"] = "https://...",   // Editable
    ["AppSettings.Triggers[1].TriggerType"] = "Automation",   // Read-only
["AppSettings.Triggers[1].RunSchedule"] = "0 0 0 1 0",    // Editable
    // ... etc
}
```

## Benefits

### ? **Prevents Accidental Changes**
Users cannot accidentally change the trigger type identifier

### ? **Clear Visual Distinction**
Blue color and border make it obvious which fields are informational

### ? **Maintains Data Integrity**
TriggerType value is preserved exactly as defined in components.json

### ? **Improved UX**
Users understand which trigger they're configuring (Workflow, Automation, IAB)

### ? **Consistent Behavior**
Value is still saved and written to appsettings.json

## Use Cases

### Use Case 1: User Configures Workflow Trigger
```
User sees: TriggerType: (Trigger Type) [ Workflow ]
User knows: This configuration is for the Workflow trigger
User edits: RunSchedule, HostApiUrl, ExePath, LogFilePath
Result: TriggerType="Workflow" preserved, other values customized
```

### Use Case 2: User Configures Multiple Triggers
```
Trigger 0: [ Workflow ] ? Can configure times, URLs, paths
Trigger 1: [ Automation ] ? Can configure times, URLs, paths
Trigger 2: [ IAB ] ? Can configure times, URLs, paths

Each trigger is clearly identified by its read-only TriggerType
User cannot confuse which trigger is which
```

### Use Case 3: Validation
```
When saving to appsettings.json:
- TriggerType values ("Workflow", "Automation", "IAB") are preserved
- Application can use TriggerType to determine which logic to execute
- No risk of user changing "Workflow" to "WorkFlow" (typo)
```

## Extending to Other Read-Only Fields

The same pattern can be applied to other fields that should be read-only:

```csharp
bool isReadOnlyField = 
    displayLabel.Equals("TriggerType", StringComparison.OrdinalIgnoreCase) ||
    displayLabel.Equals("ApplicationName", StringComparison.OrdinalIgnoreCase) ||
    displayLabel.Equals("Version", StringComparison.OrdinalIgnoreCase);
```

Or use a list:

```csharp
private static readonly string[] ReadOnlyFields = 
{
    "TriggerType",
    "ApplicationName",
    "Version"
};

bool isReadOnlyField = ReadOnlyFields.Contains(displayLabel, StringComparer.OrdinalIgnoreCase);
```

## Visual Comparison

### Before (All Editable):

```
AppSettings > Triggers [Item 0]

  TriggerType:
  [Workflow      ] ? User could type "workflow" or "WORKFLOW"
  
  RunSchedule:
  [0 0 0 0 5    ]
```

**Problem:** User might accidentally change "Workflow" to invalid value

### After (TriggerType Read-Only):

```
AppSettings > Triggers [Item 0]

  TriggerType: (Trigger Type)
  [ Workflow ] ? Clear visual indicator, cannot be changed
  
  RunSchedule:
  [0 0 0 0 5  ] ? Still editable
```

**Solution:** TriggerType is protected, user focuses on configurable values

## Color Scheme

| Element | Color | Purpose |
|---------|-------|---------|
| Read-Only Label Text | Blue (#007ACC) | Indicates informational field |
| Read-Only Background | Light Blue (#F0F8FF) | Visual distinction from textboxes |
| Read-Only Border | Black (FixedSingle) | Emphasizes the field boundary |
| Editable TextBox Text | Black (#000000) | Standard input text |
| Editable TextBox Background | White (#FFFFFF) | Standard input background |
| Field Label (Read-Only) | Blue (#007ACC) | Matches the value color |
| Field Label (Editable) | Dark Gray (#555555) | Standard label color |

## Testing Scenarios

### Test 1: TriggerType Display
- Navigate to ConfigStep with Scheduler selected
- Verify TriggerType shows as blue label with border
- Verify other fields show as white textboxes

### Test 2: TriggerType Cannot Be Edited
- Try to click on TriggerType value
- Verify it doesn't become editable
- Verify no cursor appears
- Verify no text selection occurs

### Test 3: TriggerType Value Saved
- Leave TriggerType as-is ("Workflow")
- Click Finish
- Check `component.ConfigurationValues["AppSettings.Triggers[0].TriggerType"]`
- Verify value is "Workflow"

### Test 4: Multiple Triggers
- Verify Trigger 0 shows "Workflow"
- Verify Trigger 1 shows "Automation"
- Verify Trigger 2 shows "IAB"
- Verify all three are read-only
- Verify all are saved correctly

### Test 5: Visual Distinction
- Compare TriggerType (blue, bordered) with RunSchedule (white, textbox)
- Verify users can easily distinguish read-only from editable
- Verify color scheme is consistent across all triggers

## Code Changes Summary

| File | Method | Lines Changed | Impact |
|------|--------|--------------|--------|
| ConfigStep.cs | RefreshConfigurationFields() | ~40 lines | Field rendering logic |

**Changes:**
1. Added `isReadOnlyField` detection for "TriggerType"
2. Conditional rendering: Label vs TextBox
3. Hidden textbox for read-only fields to maintain value
4. Visual styling for read-only label

## Future Enhancements

### Configurable Read-Only Fields

Add property to `InstallerComponent` model:

```csharp
public List<string> ReadOnlyConfigKeys { get; set; } = new();
```

In `components.json`:

```json
{
  "ComponentId": "SCHEDULER",
  "ManualConfiguration": true,
  "ReadOnlyConfigKeys": ["TriggerType", "ApplicationName"],
  "Configuration": { ... }
}
```

Then in ConfigStep:

```csharp
bool isReadOnlyField = component.ReadOnlyConfigKeys.Contains(displayLabel, StringComparer.OrdinalIgnoreCase);
```

This would allow per-component control of which fields are read-only.

---

**Status:** ? Implemented and Tested
**Version:** 2.2
**Build:** Successful
**Visual Impact:** Enhanced - Clear distinction between editable and informational fields
**Data Impact:** None - Values are preserved and saved correctly
