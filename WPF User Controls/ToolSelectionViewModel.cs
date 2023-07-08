using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AAP
{
    public class ToolSelectionViewModel
    {
        public List<StateBox> ToolStateBoxes { get; set; } = new();

        public ICommand SetDrawToolCommand { get; set; }
        public ICommand SetEraserToolCommand { get; set; }
        public ICommand SetSelectToolCommand { get; set; }
        public ICommand SetMoveToolCommand { get; set; }
        public ICommand SetTextToolCommand { get; set; }

        public ToolSelectionViewModel()
        {
            SetDrawToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) SetToolType(sender, ToolType.Draw); });
            SetEraserToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) SetToolType(sender, ToolType.Eraser); });
            SetSelectToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) SetToolType(sender, ToolType.Select); });
            SetMoveToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) SetToolType(sender, ToolType.Move); });
            SetTextToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) SetToolType(sender, ToolType.Text); });
        }

        private void SetToolType(object? sender, ToolType toolType)
        {
            if (sender is not StateBox stateBox)
                return;

            foreach (StateBox toolStateBox in ToolStateBoxes)
                if (toolStateBox != stateBox)
                    toolStateBox.State = false;

            App.CurrentToolType = toolType;
        }
    }
}
