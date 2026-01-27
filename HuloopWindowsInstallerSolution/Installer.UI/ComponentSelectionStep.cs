using Installer.Core;
using Installer.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.IO;

namespace Installer.UI
{
    public partial class ComponentSelectionStep : StepBase
    {
  private List<InstallerComponent> _components = new();
        private bool _isProcessingSelection = false;
    private Panel leftPanel = null!;
        private Panel rightPanel = null!;
     private Label lblTotalSize = null!;
        private Label lblDescription = null!;
        private Label lblDescriptionTitle = null!;
     private Dictionary<string, CheckBox> componentCheckBoxes = new();
        private InstallerComponent? _selectedComponent;

   public ComponentSelectionStep()
        {
      InitializeComponent();
          Title.Text = "Select Components";
    Description.Text = "Choose which components you want to install.";
    BuildLayout();
   LoadComponents();
        }

        private void BuildLayout()
        {
            var host = ContentPanel;
   host.Controls.Clear();

            // Left panel for component list
          leftPanel = new Panel
            {
     Location = new Point(20, 20),
     Size = new Size(380, 380),
     Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
 BackColor = WizardTheme.Colors.Surface,
    AutoScroll = true
      };

     leftPanel.Paint += (s, e) =>
       {
using var pen = new Pen(WizardTheme.Colors.BorderLight);
          e.Graphics.DrawRectangle(pen, 0, 0, leftPanel.Width - 1, leftPanel.Height - 1);
   };

    // Right panel for description
       rightPanel = new Panel
     {
          Location = new Point(420, 20),
       Size = new Size(440, 340),
      Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
     BackColor = WizardTheme.Colors.SurfaceSecondary
      };

     rightPanel.Paint += (s, e) =>
{
        using var pen = new Pen(WizardTheme.Colors.BorderLight);
      e.Graphics.DrawRectangle(pen, 0, 0, rightPanel.Width - 1, rightPanel.Height - 1);
  };

  // Description title
            lblDescriptionTitle = new Label
            {
    Text = "Component Details",
            Location = new Point(15, 15),
     AutoSize = true,
          Font = new Font("Segoe UI", 12, FontStyle.Bold),
     ForeColor = WizardTheme.Colors.TextPrimary
    };
     rightPanel.Controls.Add(lblDescriptionTitle);

   // Description text
            lblDescription = new Label
       {
      Text = "Select a component to view its description.",
     Location = new Point(15, 50),
      Size = new Size(410, 275),
    Font = new Font("Segoe UI", 10),
          ForeColor = WizardTheme.Colors.TextSecondary,
     BackColor = Color.Transparent,
      AutoSize = false
            };
        rightPanel.Controls.Add(lblDescription);

     // Total size label at bottom
   lblTotalSize = new Label
     {
     Location = new Point(420, 370),
Size = new Size(440, 30),
    Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
           TextAlign = ContentAlignment.MiddleRight,
       Font = new Font("Segoe UI", 11, FontStyle.Bold),
     ForeColor = WizardTheme.Colors.Primary,
           Text = "Total Size: 0 MB"
  };

   host.Controls.Add(leftPanel);
     host.Controls.Add(rightPanel);
  host.Controls.Add(lblTotalSize);
        }

     private void LoadComponents()
        {
      var jsonPath = Path.Combine(AppContext.BaseDirectory, "components.json");

      if (!File.Exists(jsonPath))
     throw new FileNotFoundException("components.json not found", jsonPath);
  
  string json = File.ReadAllText(jsonPath);
    var options = new JsonSerializerOptions
      {
   PropertyNameCaseInsensitive = true
  };
            _components = JsonSerializer.Deserialize<List<InstallerComponent>>(json, options)
 ?? new List<InstallerComponent>();

        leftPanel.Controls.Clear();
        componentCheckBoxes.Clear();

          int y = 15;

        foreach (var component in _components)
     {
 // Create checkbox for component
     var checkBox = new CheckBox
       {
            Text = $"{component.ComponentName}",
        Location = new Point(15, y),
  AutoSize = false,
    Size = new Size(280, 24),
    Font = new Font("Segoe UI", 10, FontStyle.Bold),
       ForeColor = WizardTheme.Colors.TextPrimary,
        Tag = component
  };

  checkBox.CheckedChanged += OnComponentCheckedChanged;
      checkBox.MouseEnter += (s, e) => OnComponentHover(component);
      checkBox.Click += (s, e) => OnComponentClick(component);

  // Size label - Use platform-specific size
    var lblSize = new Label
  {
     Text = FormatSize(component.GetSizeForCurrentPlatform()),
      Location = new Point(300, y + 2),
        AutoSize = true,
    Font = new Font("Segoe UI", 9),
ForeColor = WizardTheme.Colors.TextSecondary
        };

    leftPanel.Controls.Add(checkBox);
       leftPanel.Controls.Add(lblSize);
     componentCheckBoxes[component.ComponentId] = checkBox;

     y += 32;
        }

    UpdateTotalSize();
      }

   private void OnComponentHover(InstallerComponent component)
   {
    // Show description on hover
  ShowComponentDescription(component);
        }

        private void OnComponentClick(InstallerComponent component)
        {
    // Show description on click and select
      _selectedComponent = component;
     ShowComponentDescription(component);
        }

  private void ShowComponentDescription(InstallerComponent component)
    {
         lblDescriptionTitle.Text = component.ComponentName;
  
      // Build description text
    var description = new System.Text.StringBuilder();
  description.AppendLine(component.Description);
          description.AppendLine();
      description.AppendLine($"Size: {FormatSize(component.GetSizeForCurrentPlatform())}");
     description.AppendLine($"Platform: {InstallerComponent.GetCurrentPlatformDescription()}");
            
   if (component.IsMandatory)
    {
   description.AppendLine("Status: Required");
 }

  if (component.IsConnectedWith != null && component.IsConnectedWith.Count > 0)
            {
        description.AppendLine();
       description.AppendLine("Dependencies:");
    foreach (var depId in component.IsConnectedWith)
   {
      var depComponent = _components.FirstOrDefault(c => c.ComponentId == depId);
        if (depComponent != null)
          {
       description.AppendLine($"  • {depComponent.ComponentName}");
      }
         }
   description.AppendLine("(Will be automatically selected)");
   }

        if (component.ManualConfiguration)
     {
     description.AppendLine();
         description.AppendLine("⚙️ Requires configuration after installation");
            }

  lblDescription.Text = description.ToString();
 }

   private void OnComponentCheckedChanged(object? sender, EventArgs e)
 {
            if (_isProcessingSelection || sender is not CheckBox checkBox)
         return;

            _isProcessingSelection = true;

            try
{
         var component = checkBox.Tag as InstallerComponent;
     if (component == null)
      return;

 component.IsSelected = checkBox.Checked;

 if (checkBox.Checked)
         {
         SelectConnectedComponents(component);
                }
    else
       {
  DeselectDependentComponents(component);
         }

          UpdateTotalSize();
            }
  finally
       {
    _isProcessingSelection = false;
   }
        }

        private void SelectConnectedComponents(InstallerComponent selectedComponent)
        {
            if (selectedComponent.IsConnectedWith == null || selectedComponent.IsConnectedWith.Count == 0)
  return;

    foreach (var connectedId in selectedComponent.IsConnectedWith)
       {
     var connectedComponent = _components.FirstOrDefault(c => c.ComponentId == connectedId);
      if (connectedComponent != null && !connectedComponent.IsSelected)
  {
        if (componentCheckBoxes.TryGetValue(connectedId, out var checkBox))
     {
            checkBox.Checked = true;
        connectedComponent.IsSelected = true;
     SelectConnectedComponents(connectedComponent);
    }
      }
     }
 }

      private void DeselectDependentComponents(InstallerComponent deselectedComponent)
 {
  foreach (var component in _components)
   {
       if (component.IsConnectedWith != null && 
   component.IsConnectedWith.Contains(deselectedComponent.ComponentId) &&
        component.IsSelected)
     {
    if (componentCheckBoxes.TryGetValue(component.ComponentId, out var checkBox))
    {
      checkBox.Checked = false;
       component.IsSelected = false;
   DeselectDependentComponents(component);
     }
       }
  }
        }

  private void UpdateTotalSize()
        {
    var total = _components
          .Where(c => c.IsSelected)
          .Sum(c => c.GetSizeForCurrentPlatform());

      lblTotalSize.Text = $"Total Size: {FormatSize(total)}";
        }

        private string FormatSize(long sizeMB)
{
   if (sizeMB >= 1000)
  {
     double sizeGB = sizeMB / 1024.0;
       return $"{sizeGB:F2} GB";
            }
        else
     {
     return $"{sizeMB:N0} MB";
   }
        }

        public List<InstallerComponent> GetSelectedComponents()
        {
   return _components.Where(c => c.IsSelected).ToList();
        }
    }
}
