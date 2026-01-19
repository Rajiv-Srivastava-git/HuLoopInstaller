using Installer.Core;
using Installer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer.UI
{
    public partial class ComponentSelectionStep : StepBase
    {
        private List<InstallerComponent> _components = new();
        private bool _isProcessingSelection = false; // Prevent recursive selection

        public ComponentSelectionStep()
        {
            InitializeComponent();
            LoadComponents();
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

            flowPanel.Controls.Clear();

            foreach (var comp in _components)
            {
                var ctrl = new ComponentItemControl();
                ctrl.Bind(comp);
                ctrl.SelectionChanged += OnComponentSelectionChanged;

                flowPanel.Controls.Add(ctrl);
            }

            UpdateTotalSize();
        }

        private void OnComponentSelectionChanged(object? sender, bool selected)
        {
          // Prevent recursive calls
            if (_isProcessingSelection)
 return;

            _isProcessingSelection = true;

         try
     {
             if (sender is ComponentItemControl sourceControl)
                {
         var component = sourceControl.Component;

       if (selected)
            {
    // Component was selected - select all connected components
     SelectConnectedComponents(component);
      }
             else
 {
 // Component was deselected - check if we need to deselect dependents
        DeselectDependentComponents(component);
       }
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
            // Find the control and update its checkbox
    var control = FindComponentControl(connectedComponent.ComponentId);
                    if (control != null)
     {
          control.SetSelected(true);
   connectedComponent.IsSelected = true;

              // Recursively select components connected to this one
  SelectConnectedComponents(connectedComponent);
 }
      }
   }
     }

 private void DeselectDependentComponents(InstallerComponent deselectedComponent)
        {
   // Find all components that depend on this deselected component
            foreach (var component in _components)
     {
       if (component.IsConnectedWith != null && 
      component.IsConnectedWith.Contains(deselectedComponent.ComponentId) &&
          component.IsSelected)
        {
          // This component depends on the deselected one - deselect it
             var control = FindComponentControl(component.ComponentId);
 if (control != null)
          {
  control.SetSelected(false);
        component.IsSelected = false;

         // Recursively deselect components that depend on this one
DeselectDependentComponents(component);
      }
        }
   }
     }

        private ComponentItemControl? FindComponentControl(string componentId)
     {
            foreach (Control ctrl in flowPanel.Controls)
          {
  if (ctrl is ComponentItemControl itemControl && 
    itemControl.Component.ComponentId == componentId)
              {
                 return itemControl;
                }
  }
         return null;
        }

      private void UpdateTotalSize()
        {
            var total = _components
        .Where(c => c.IsSelected)
     .Sum(c => c.ComponentSizeMB);

            lblTotalSize.Text = $"Total Size: {total} MB";
        }

        public List<InstallerComponent> GetSelectedComponents()
      {
            return _components.Where(c => c.IsSelected).ToList();
        }
    }
}
