using System;
using System.Windows.Forms;
using Installer.Models;

namespace Installer.UI
{
    public partial class ComponentItemControl : UserControl
    {
        public Installer.Models.InstallerComponent Component { get; private set; } = null!;

        public event EventHandler<bool>? SelectionChanged;
        
        private bool _suppressEvent = false;

        public ComponentItemControl()
        {
            InitializeComponent();
        }

        public void Bind(InstallerComponent component)
        {
            Component = component;

            chkSelect.Text = component.ComponentName;
            lblSize.Text = $"{component.ComponentSizeMB} MB";

            chkSelect.Checked = component.IsMandatory;
            chkSelect.Enabled = !component.IsMandatory;

            component.IsSelected = chkSelect.Checked;
        }

        /// <summary>
        /// Programmatically set the selection state without triggering the event
        /// </summary>
        public void SetSelected(bool selected)
        {
            _suppressEvent = true;
            try
            {
                chkSelect.Checked = selected;
                Component.IsSelected = selected;
            }
            finally
            {
                _suppressEvent = false;
            }
        }

        private void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (Component == null) return;

            Component.IsSelected = chkSelect.Checked;
            
            // Only raise event if not suppressed (prevents recursive calls)
            if (!_suppressEvent)
            {
                SelectionChanged?.Invoke(this, chkSelect.Checked);
            }
        }
    }
}