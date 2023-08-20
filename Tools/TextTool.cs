using AAP.Timelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AAP
{
    public class TextTool: Tool, IKeyInput, ITextInput
    {
        public override ToolType Type { get; } = ToolType.Text;

        public event ReceivedTextInputEvent? ReceivedTextInput;

        private bool isTyping = false;
        protected bool IsTyping
        {
            get => isTyping;
            set
            {
                if (isTyping == value)
                    return;

                isTyping = value;
            }
        }

        public TextTool()
        {
            App.OnCurrentArtChanged += OnArtFileChanged;
            App.OnCurrentToolChanged += OnToolChanged;
        }

        protected override void UseStart(Point startArtPos)
        {
            if (App.CurrentArt == null)
                return;

            if (IsTyping)
                App.CurrentArtTimeline?.NewTimePoint();

            isTyping = false;

            ArtLayer layer = App.CurrentArt.ArtLayers[App.CurrentLayerID];

            App.SelectedArt = new(Math.Clamp(startArtPos.X, layer.OffsetX, layer.OffsetX + layer.Width - 1), Math.Clamp(startArtPos.Y, layer.OffsetY, layer.OffsetY + layer.Height - 1), 1, 1);
        }

        public void TypeKeyCharacter(char character)
        {
            if (App.CurrentArt == null)
                return;

            if (App.CurrentArtDraw == null)
                return;

            if (App.SelectedArt == Rect.Empty)
                return;

            if (char.IsControl(character)) 
                return;

            if (char.IsWhiteSpace(character) && character != ' ') 
                return;

            ArtLayer layer = App.CurrentArt.ArtLayers[App.CurrentLayerID];

            Point drawPoint = new(Math.Clamp(App.SelectedArt.Location.X, layer.OffsetX, layer.OffsetX + layer.Width - 1), Math.Clamp(App.SelectedArt.Location.Y, layer.OffsetY, layer.OffsetY + layer.Height - 1));
            App.CurrentArtDraw.DrawCharacter(App.CurrentLayerID, character == ' ' ? null : character, drawPoint);

            if (drawPoint.X < layer.OffsetX + layer.Width - 1)
                App.SelectedArt = new(drawPoint.X + 1, drawPoint.Y, 1, 1);
            else if (drawPoint.Y < layer.OffsetY + layer.Height - 1)
                App.SelectedArt = new(Math.Clamp(StartArtPos.X, layer.OffsetX, layer.OffsetX + layer.Width - 1), drawPoint.Y + 1, 1, 1);

            IsTyping = true;

            ConsoleLogger.Log("Text Tool: Typed character " + character);
        }

        public void OnTextInput(string text)
        {
            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
                TypeKeyCharacter(chars[i]);

            ReceivedTextInput?.Invoke(this, text);
        }

        public void OnPressedKey(Key key, ModifierKeys modifierKeys)
        {
            if (App.CurrentArt == null)
                return;

            if (App.SelectedArt == Rect.Empty)
                return;

            if (App.CurrentLayerID == -1)
                return;

            ArtLayer layer = App.CurrentArt.ArtLayers[App.CurrentLayerID];

            Point drawPoint = new(Math.Clamp(App.SelectedArt.X, layer.OffsetX, layer.OffsetX + layer.Width - 1), Math.Clamp(App.SelectedArt.Y, layer.OffsetY, layer.OffsetY + layer.Height - 1));
            
            switch (key)
            {
                case Key.Down:
                    App.SelectedArt = new(drawPoint.X, Math.Clamp(drawPoint.Y + 1, layer.OffsetY, layer.OffsetY + layer.Height - 1), 1, 1);
                    break;
                case Key.Up:
                    App.SelectedArt = new(drawPoint.X, Math.Clamp(drawPoint.Y - 1, layer.OffsetY, layer.OffsetY + layer.Height - 1), 1, 1);
                    break;
                case Key.Right:
                    App.SelectedArt = new(Math.Clamp(drawPoint.X + 1, layer.OffsetX, layer.OffsetX + layer.Width - 1), drawPoint.Y, 1, 1);
                    break;
                case Key.Left:
                    App.SelectedArt = new(Math.Clamp(drawPoint.X - 1, layer.OffsetX, layer.OffsetX + layer.Width - 1), drawPoint.Y, 1, 1);
                    break;
                case Key.Return:
                    App.SelectedArt = new(Math.Clamp(StartArtPos.X, layer.OffsetX, layer.OffsetX + layer.Width - 1), Math.Clamp(drawPoint.Y + 1, layer.OffsetY, layer.OffsetY + layer.Height - 1), 1, 1);

                    if (IsTyping)
                        App.CurrentArtTimeline?.NewTimePoint();

                    isTyping = false;

                    break;
                case Key.Back:
                    if (drawPoint.X != layer.OffsetX + layer.Width - 1 || drawPoint.Y != layer.OffsetY + layer.Height - 1 || layer.Data[(int)drawPoint.X - layer.OffsetX][(int)drawPoint.Y - layer.OffsetY] == null)
                        if (drawPoint.X > layer.OffsetX)
                            App.SelectedArt = new(drawPoint.X - 1, drawPoint.Y, 1, 1);
                        else if (drawPoint.Y > layer.OffsetY)
                            App.SelectedArt = new(layer.OffsetX + layer.Width - 1, drawPoint.Y - 1, 1, 1);
                    
                    App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, null, App.SelectedArt.Location);
                    isTyping = true;
                    break;
                default:
                    break;
            }

            if (StartArtPos.X > App.SelectedArt.X)
                StartArtPos = new((int)App.SelectedArt.X, StartArtPos.Y);
        }

        private void OnArtFileChanged(ASCIIArt? art, ASCIIArtDraw? artDraw, ObjectTimeline? artTimeline)
        {
            if (IsTyping)
                IsTyping = false;
        }

        private void OnToolChanged(Tool? tool)
        {
            if (IsTyping)
            {
                IsTyping = false;
                App.CurrentArtTimeline?.NewTimePoint();
            }
        }
    }
}
