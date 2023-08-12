using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AAP
{
    public class TextTool: Tool
    {
        public TextTool()
        {
            Type = ToolType.Text;
            //System.Windows.Input.Keyboard.PrimaryDevice.FocusedElement.PreviewTextInput returns strings!!
        }

        public override void ActivateStart(Point location)
        {
            if (App.CurrentArt == null)
                return;

            base.ActivateStart(location);

            App.SelectedArt = new(Math.Clamp(location.X, 0, App.CurrentArt.Width), Math.Clamp(location.Y, 0, App.CurrentArt.Height), 1, 1);
        }

        public static void TypeKeyCharacter(char character)
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

            App.CurrentArtDraw.DrawCharacter(App.CurrentLayerID, character == ' ' ? null : character, App.SelectedArt.Location);
            
            if(App.SelectedArt.X < App.CurrentArt.Width)
                App.SelectedArt = new(App.SelectedArt.X + 1, App.SelectedArt.Y, App.SelectedArt.Width, App.SelectedArt.Height);

            Console.WriteLine("Type Key! Character: " + character);
        }

        public void OnPressKeyCode(Key key)
        {
            if (App.CurrentArt == null)
                return;

            if (App.SelectedArt == Rect.Empty)
                return;

            switch(key)
            {
                case Key.Down:
                    App.SelectedArt = new(App.SelectedArt.Location.X, Math.Clamp(App.SelectedArt.Location.Y + 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Key.Up:
                    App.SelectedArt = new(App.SelectedArt.Location.X, Math.Clamp(App.SelectedArt.Location.Y - 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Key.Right:
                    App.SelectedArt = new(Math.Clamp(App.SelectedArt.Location.X + 1, 0, App.CurrentArt.Width - 1), App.SelectedArt.Location.Y, 1, 1);
                    break;
                case Key.Left:
                    App.SelectedArt = new(Math.Clamp(App.SelectedArt.Location.X - 1, 0, App.CurrentArt.Width - 1), App.SelectedArt.Location.Y, 1, 1);
                    break;
                case Key.Return:
                    App.SelectedArt = new(StartPoint.X, Math.Clamp(App.SelectedArt.Location.Y + 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Key.Back:
                    if (App.SelectedArt.X > 0)
                        App.SelectedArt = new(App.SelectedArt.X - 1, App.SelectedArt.Y, App.SelectedArt.Width, App.SelectedArt.Height);

                    App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, null, App.SelectedArt.Location);
                    break;
                default:
                    break;
            }

            if (StartPoint.X > App.SelectedArt.X)
                StartPoint = new((int)App.SelectedArt.X, StartPoint.Y);
        }
    }
}
