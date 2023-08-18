using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                CharacterOptionVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                PropertyChanged?.Invoke(this, new(nameof(IsCharacterOptionVisible)));
            }
        }

        private Visibility characterOptionVisibility = Visibility.Collapsed;
        public Visibility CharacterOptionVisibility
        {
            get => characterOptionVisibility;
            private set
            {
                if (characterOptionVisibility == value)
                    return;

                characterOptionVisibility = value;

                PropertyChanged?.Invoke(this, new(nameof(CharacterOptionVisibility)));
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
                SizeOptionVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                PropertyChanged?.Invoke(this, new(nameof(IsSizeOptionVisible)));
            }
        }

        private Visibility sizeOptionVisibility = Visibility.Collapsed;
        public Visibility SizeOptionVisibility
        {
            get => sizeOptionVisibility;
            private set
            {
                if (sizeOptionVisibility == value)
                    return;

                sizeOptionVisibility = value;
                PropertyChanged?.Invoke(this, new(nameof(SizeOptionVisibility)));
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
                EightDirectionalOptionVisibility = value ? Visibility.Visible : Visibility.Collapsed;

                PropertyChanged?.Invoke(this, new(nameof(IsEightDirectionalOptionVisible)));
            }
        }

        private Visibility eightDirectionalOptionVisibility = Visibility.Collapsed;
        public Visibility EightDirectionalOptionVisibility
        {
            get => eightDirectionalOptionVisibility;
            private set
            {
                if (eightDirectionalOptionVisibility == value)
                    return;

                eightDirectionalOptionVisibility = value;
                PropertyChanged?.Invoke(this, new(nameof(EightDirectionalOptionVisibility)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnToolSelected(Tool? tool)
        {
            IsCharacterOptionVisible = tool is ICharacterSelectable;
            IsSizeOptionVisible = tool is ISizeSelectable;
            IsEightDirectionalOptionVisible = tool is IEightDirectionalProperty;

            if (tool is ICharacterSelectable characterSelectableTool && CharacterPaletteSelectionViewModel != null)
                CharacterPaletteSelectionViewModel.SelectedCharacter = characterSelectableTool.Character;

            if (tool is ISizeSelectable sizeSelectableTool)
                Size = sizeSelectableTool.Size;

            if (tool is IEightDirectionalProperty eightDirectionalPropertyTool)
                EightDirectional = eightDirectionalPropertyTool.EightDirectional;

        }
    }
}
