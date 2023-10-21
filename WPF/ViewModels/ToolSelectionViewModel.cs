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

            OnToolChanged(App.CurrentTool);

            App.OnCurrentToolChanged += OnToolChanged;
            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content
        private string pencilToolTip = App.Language.GetString("Tool_PencilToolTip");
        public string PencilToolTip => pencilToolTip;

        private string eraserToolTip = App.Language.GetString("Tool_EraserToolTip");
        public string EraserToolTip => eraserToolTip;

        private string selectToolTip = App.Language.GetString("Tool_SelectToolTip");
        public string SelectToolTip => selectToolTip;

        private string moveToolTip = App.Language.GetString("Tool_MoveToolTip");
        public string MoveToolTip => moveToolTip;

        private string lineToolTip = App.Language.GetString("Tool_LineToolTip");
        public string LineToolTip => lineToolTip;

        private string bucketToolTip = App.Language.GetString("Tool_BucketToolTip");
        public string BucketToolTip => bucketToolTip;

        private string textToolTip = App.Language.GetString("Tool_TextToolTip");
        public string TextToolTip => textToolTip;

        private void OnLanguageChanged(Language language)
        {
            pencilToolTip = App.Language.GetString("Tool_PencilToolTip");
            eraserToolTip = App.Language.GetString("Tool_EraserToolTip");
            selectToolTip = App.Language.GetString("Tool_SelectToolTip");
            moveToolTip = App.Language.GetString("Tool_MoveToolTip");
            lineToolTip = App.Language.GetString("Tool_LineToolTip");
            bucketToolTip = App.Language.GetString("Tool_BucketToolTip");
            textToolTip = App.Language.GetString("Tool_TextToolTip");

            PropertyChanged?.Invoke(this, new(nameof(PencilToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(EraserToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(SelectToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(MoveToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(LineToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(BucketToolTip)));
            PropertyChanged?.Invoke(this, new(nameof(TextToolTip)));
        }
        #endregion

        private void OnToolChanged(Tool? tool)
        {
            foreach (KeyValuePair<ToolType, StateBox> kvp in ToolStateBoxes)
                kvp.Value.State = tool != null && kvp.Key == tool.Type;
        }
    }
}
