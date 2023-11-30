using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AAP.UI.Controls;
using AAP.FileObjects;
using System.ComponentModel;

namespace AAP.UI.ViewModels
{
    public class ToolSelectionViewModel : INotifyPropertyChanged
    {
        public Dictionary<ToolType, StateBox> ToolStateBoxes { get; set; } = new();

        public ICommand SetDrawToolCommand { get; set; }
        public ICommand SetEraserToolCommand { get; set; }
        public ICommand SetSelectToolCommand { get; set; }
        public ICommand SetMoveToolCommand { get; set; }
        public ICommand SetBucketToolCommand { get; set; }
        public ICommand SetTextToolCommand { get; set; }
        public ICommand SetLineToolCommand { get; set; }
        public ICommand SetRectangleToolCommand { get; set; }
        public ICommand SetEllipseToolCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ToolSelectionViewModel()
        {
            SetDrawToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Draw); } );
            SetEraserToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Eraser); });
            SetSelectToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Select); });
            SetMoveToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Move); });
            SetTextToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Text); });
            SetBucketToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Bucket); });
            SetLineToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Line); });
            SetRectangleToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Rectangle); });
            SetEllipseToolCommand = new EventArgsCommand<bool>((sender, state) => { if (state) App.SelectToolType(ToolType.Ellipse); });

            OnToolChanged(App.CurrentTool);

            App.OnCurrentToolChanged += OnToolChanged;
            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content
        private string pencilToolTip = App.Language.GetString("Tool_PencilTool") + " (Ctrl + Shift + D)";
        public string PencilToolTip => pencilToolTip;

        private string eraserToolTip = App.Language.GetString("Tool_EraserTool") + " (Ctrl + Shift + E)";
        public string EraserToolTip => eraserToolTip;

        private string selectToolTip = App.Language.GetString("Tool_SelectTool") + " (Ctrl + Shift + W)";
        public string SelectToolTip => selectToolTip;

        private string moveToolTip = App.Language.GetString("Tool_MoveTool") + " (Ctrl + Shift + M)";
        public string MoveToolTip => moveToolTip;

        private string lineToolTip = App.Language.GetString("Tool_LineTool") + " (Ctrl + Shift + L)";
        public string LineToolTip => lineToolTip;

        private string bucketToolTip = App.Language.GetString("Tool_BucketTool") + " (Ctrl + Shift + B)";
        public string BucketToolTip => bucketToolTip;

        private string textToolTip = App.Language.GetString("Tool_TextTool") + " (Ctrl + Shift + T)";
        public string TextToolTip => textToolTip;

        private string rectangleToolTip = App.Language.GetString("Tool_RectangleTool") + " (Ctrl + Shift + R)";
        public string RectangleToolTip => rectangleToolTip;

        private string ellipseToolTip = App.Language.GetString("Tool_EllipseTool") + " (Ctrl + Shift + G)";
        public string EllipseToolTip => ellipseToolTip;

        private void OnLanguageChanged(Language language)
        {
            pencilToolTip = language.GetString("Tool_PencilTool") + " (Ctrl + Shift + D)";
            eraserToolTip = language.GetString("Tool_EraserTool") + " (Ctrl + Shift + E)";
            selectToolTip = language.GetString("Tool_SelectTool") + " (Ctrl + Shift + W)";
            moveToolTip = language.GetString("Tool_MoveTool") + " (Ctrl + Shift + M)";
            lineToolTip = language.GetString("Tool_LineTool") + " (Ctrl + Shift + L)";
            bucketToolTip = language.GetString("Tool_BucketTool") + " (Ctrl + Shift + B)";
            textToolTip = language.GetString("Tool_TextTool") + " (Ctrl + Shift + T)";
            rectangleToolTip = language.GetString("Tool_RectangleTool") + " (Ctrl + Shift + R)";
            ellipseToolTip = language.GetString("Tool_EllipseTool") + " (Ctrl + Shift + G)";

            PropertyChanged?.Invoke(this, new(nameof(PencilToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(EraserToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(SelectToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(MoveToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(LineToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(BucketToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(TextToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(RectangleToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(EllipseToolTip)));
        }
        #endregion

        private void OnToolChanged(Tool? tool)
        {
            foreach (KeyValuePair<ToolType, StateBox> kvp in ToolStateBoxes)
                kvp.Value.State = tool != null && kvp.Key == tool.Type;
        }
    }
}
