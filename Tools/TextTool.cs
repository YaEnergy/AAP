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

            App.Selected = new(Math.Clamp(location.X, 0, App.CurrentArt.Width), Math.Clamp(location.Y, 0, App.CurrentArt.Height), 1, 1);
        }

        public static void TypeKeyCharacter(char character)
        {
            if (App.CurrentArt == null)
                return;

            if (App.CurrentArtDraw == null)
                return;

            if (App.Selected == Rect.Empty)
                return;

            if (char.IsControl(character)) 
                return;

            if (char.IsWhiteSpace(character) && character != ' ') 
                return;

            App.CurrentArtDraw.DrawCharacter(App.CurrentLayerID, character == ' ' ? null : character, App.Selected.Location);
            
            if(App.Selected.X < App.CurrentArt.Width)
                App.Selected = new(App.Selected.X + 1, App.Selected.Y, App.Selected.Width, App.Selected.Height);

            Console.WriteLine("Type Key! Character: " + character);
        }

        public void OnPressKeyCode(Key key)
        {
            Console.WriteLine("TextTool.OnPressKeyCode() - Point from System.Drawing is used instead of from System.Windows!");
            if (App.CurrentArt == null)
                return;

            if (App.Selected == Rect.Empty)
                return;

            switch(key)
            {
                case Key.Down:
                    App.Selected = new(App.Selected.Location.X, Math.Clamp(App.Selected.Location.Y + 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Key.Up:
                    App.Selected = new(App.Selected.Location.X, Math.Clamp(App.Selected.Location.Y - 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Key.Right:
                    App.Selected = new(Math.Clamp(App.Selected.Location.X + 1, 0, App.CurrentArt.Width - 1), App.Selected.Location.Y, 1, 1);
                    break;
                case Key.Left:
                    App.Selected = new(Math.Clamp(App.Selected.Location.X - 1, 0, App.CurrentArt.Width - 1), App.Selected.Location.Y, 1, 1);
                    break;
                case Key.Return:
                    App.Selected = new(StartPoint.X, Math.Clamp(App.Selected.Location.Y + 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Key.Back:
                    if (App.Selected.X > 0)
                        App.Selected = new(App.Selected.X - 1, App.Selected.Y, App.Selected.Width, App.Selected.Height);

                    App.CurrentArtDraw?.DrawCharacter(App.CurrentLayerID, null, App.Selected.Location);
                    break;
                default:
                    break;
            }

            if (StartPoint.X > App.Selected.X)
                StartPoint = new((int)App.Selected.X, StartPoint.Y);
        }
    }
}
