using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AAP.FileObjects;

namespace AAP.UI.ViewModels
{
    public class ToolOptionsViewModel : INotifyPropertyChanged
    {
        private Tool? tool;
        public Tool? Tool
        {
            get => tool;
            set
            {
                if (tool == value)
                    return;

                tool = value;

                OnToolSelected(tool);

                PropertyChanged?.Invoke(this, new(nameof(Tool)));
                PropertyChanged?.Invoke(this, new(nameof(HasToolSelected)));

                if (tool != null)
                    ToolName = GetToolTypeName(tool.Type);
            }
        }

        public bool HasToolSelected
        {
            get => tool != null && tool.Type != ToolType.None;
        }

        private string toolName = "";
        public string ToolName
        {
            get => toolName;
            set
            {
                if (toolName == value)
                    return;

                toolName = value;

                PropertyChanged?.Invoke(this, new(nameof(ToolName)));
            }
        }

        private CharacterPaletteSelectionViewModel? characterPaletteSelectionViewModel;
        public CharacterPaletteSelectionViewModel? CharacterPaletteSelectionViewModel 
        { 
            get => characterPaletteSelectionViewModel; 
            set
            {
                if (characterPaletteSelectionViewModel == value)
                    return;

                characterPaletteSelectionViewModel = value;

                if (value == null)
                    return;

                if (Tool is ICharacterSelectable characterSelectableTool)
                {
                    value.SelectedCharacter = characterSelectableTool.Character;
                }
            }
        }

        private bool isCharacterOptionVisible = false;
        public bool IsCharacterOptionVisible
        {
            get => isCharacterOptionVisible;
            set
            {
                if (isCharacterOptionVisible == value)
                    return;

                isCharacterOptionVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsCharacterOptionVisible)));
            }
        }

        private int size = 1;
        public int Size 
        { 
            get => size; 
            set
            {
                if (size == value)
                    return;

                size = value;

                if (tool is ISizeSelectable sizeSelectableTool)
                    sizeSelectableTool.Size = value;

                PropertyChanged?.Invoke(this, new(nameof(Size)));
            }
        }

        private bool isSizeOptionVisible = false;
        public bool IsSizeOptionVisible
        {
            get => isSizeOptionVisible;
            set
            {
                if (isSizeOptionVisible == value)
                    return;

                isSizeOptionVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsSizeOptionVisible)));
            }
        }

        private bool eightDirectional = false;
        public bool EightDirectional
        {
            get => eightDirectional;
            set
            {
                if (eightDirectional == value)
                    return;

                eightDirectional = value;

                if (tool is IEightDirectionalProperty eightDirectionalPropertyTool)
                    eightDirectionalPropertyTool.EightDirectional = value;

                PropertyChanged?.Invoke(this, new(nameof(EightDirectional)));
            }
        }

        private bool isEightDirectionalOptionVisible = false;
        public bool IsEightDirectionalOptionVisible
        {
            get => isEightDirectionalOptionVisible;
            set
            {
                if (isEightDirectionalOptionVisible == value)
                    return;

                isEightDirectionalOptionVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsEightDirectionalOptionVisible)));
            }
        }

        private bool stayInsideSelection = false;
        public bool StayInsideSelection
        {
            get => stayInsideSelection;
            set
            {
                if (stayInsideSelection == value)
                    return;

                stayInsideSelection = value;

                if (tool is IStayInsideSelectionProperty propertyTool)
                    propertyTool.StayInsideSelection = value;

                PropertyChanged?.Invoke(this, new(nameof(StayInsideSelection)));
            }
        }

        private bool isStayInsideSelectionOptionVisible = false;
        public bool IsStayInsideSelectionOptionVisible
        {
            get => isStayInsideSelectionOptionVisible;
            set
            {
                if (isStayInsideSelectionOptionVisible == value)
                    return;

                isStayInsideSelectionOptionVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsStayInsideSelectionOptionVisible)));
            }
        }

        private bool fill = false;
        public bool Fill
        {
            get => fill;
            set
            {
                if (fill == value)
                    return;

                fill = value;

                if (tool is IFillProperty propertyTool)
                    propertyTool.Fill = value;

                PropertyChanged?.Invoke(this, new(nameof(Fill)));
            }
        }

        private bool isFillOptionVisible = false;
        public bool IsFillOptionVisible
        {
            get => isFillOptionVisible;
            set
            {
                if (isFillOptionVisible == value)
                    return;

                isFillOptionVisible = value;

                PropertyChanged?.Invoke(this, new(nameof(IsFillOptionVisible)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnToolSelected(Tool? tool)
        {
            IsCharacterOptionVisible = tool is ICharacterSelectable;
            IsSizeOptionVisible = tool is ISizeSelectable;
            IsEightDirectionalOptionVisible = tool is IEightDirectionalProperty;
            IsStayInsideSelectionOptionVisible = tool is IStayInsideSelectionProperty;
            IsFillOptionVisible = tool is IFillProperty;

            if (tool is ICharacterSelectable characterSelectableTool && CharacterPaletteSelectionViewModel != null)
                CharacterPaletteSelectionViewModel.SelectedCharacter = characterSelectableTool.Character;

            if (tool is ISizeSelectable sizeSelectableTool)
                Size = sizeSelectableTool.Size;

            if (tool is IEightDirectionalProperty eightDirectionalPropertyTool)
                EightDirectional = eightDirectionalPropertyTool.EightDirectional;

            if (tool is IStayInsideSelectionProperty stayInsideSelectionPropertyTool)
                StayInsideSelection = stayInsideSelectionPropertyTool.StayInsideSelection;

            if (tool is IFillProperty fillPropertyTool)
                Fill = fillPropertyTool.Fill;
        }

        public static string GetToolTypeName(ToolType toolType)
        {
            return toolType switch
            {
                ToolType.Draw => App.Language.GetString("Tool_PencilTool"),
                ToolType.Eraser => App.Language.GetString("Tool_EraserTool"),
                ToolType.Select => App.Language.GetString("Tool_SelectTool"),
                ToolType.Move => App.Language.GetString("Tool_MoveTool"),
                ToolType.Line => App.Language.GetString("Tool_LineTool"),
                ToolType.Bucket => App.Language.GetString("Tool_BucketTool"),
                ToolType.Text => App.Language.GetString("Tool_TextTool"),
                ToolType.Rectangle => App.Language.GetString("Tool_RectangleTool"),
                ToolType.Ellipse => App.Language.GetString("Tool_EllipseTool"),
                _ => throw new ArgumentException(nameof(toolType) + " has no name!"),
            };
        }

        public ToolOptionsViewModel()
        {
            App.OnLanguageChanged += OnLanguageChanged;
        }

        #region Language Content

        private string toolOptionsContent = App.Language.GetString("Tool_Options");
        public string ToolOptionsContent => toolOptionsContent;

        private string sizeContent = App.Language.GetString("Tool_Size");
        public string SizeContent => sizeContent;

        private string eightDirectionalContent = App.Language.GetString("Tool_EightDirectional");
        public string EightDirectionalContent => eightDirectionalContent;

        private string stayInsideSelectionContent = App.Language.GetString("Tool_StayInsideSelection");
        public string StayInsideSelectionContent => stayInsideSelectionContent;

        private string fillContent = App.Language.GetString("Tool_Fill");
        public string FillContent => fillContent;

        private void OnLanguageChanged(Language language)
        {
            toolOptionsContent = language.GetString("Tool_Options");
            sizeContent = language.GetString("Tool_Size");
            eightDirectionalContent = language.GetString("Tool_EightDirectional");
            stayInsideSelectionContent = language.GetString("Tool_StayInsideSelection");
            fillContent = language.GetString("Tool_Fill");

            PropertyChanged?.Invoke(this, new(nameof(ToolOptionsContent)));
            PropertyChanged?.Invoke(this, new(nameof(SizeContent)));
            PropertyChanged?.Invoke(this, new(nameof(EightDirectionalContent)));
            PropertyChanged?.Invoke(this, new(nameof(StayInsideSelectionContent)));
            PropertyChanged?.Invoke(this, new(nameof(FillContent)));

            if (tool != null && HasToolSelected)
                ToolName = GetToolTypeName(tool.Type);
        }

        #endregion
    }
}
