using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AAP
{
    public class TextTool: Tool
    {
        public TextTool()
        {
            Type = ToolType.Text;
        }

        public override void ActivateStart(System.Drawing.Point location)
        {
            if (App.CurrentArt == null)
                return;

            base.ActivateStart(location);

            App.Selected = new(Math.Clamp(location.X, 0, App.CurrentArt.Width), Math.Clamp(location.Y, 0, App.CurrentArt.Height), 1, 1);
        }

        public static void TypeKeyCharacter(char character)
        {
            Console.WriteLine("TextTool.TypeKeyCharacter(character) - Point from System.Drawing is used instead of from System.Windows!");

            if (App.CurrentArt == null)
                return;

            if (App.Selected == Rect.Empty)
                return;

            if (char.IsControl(character)) 
                return;

            if (char.IsWhiteSpace(character) && character != ' ') 
                return;

            App.CurrentArt.Draw(App.CurrentLayerID, new((int)App.Selected.X, (int)App.Selected.Y), character == ' ' ? null : character);
            
            if(App.Selected.X < App.CurrentArt.Width)
                App.Selected = new(App.Selected.X + 1, App.Selected.Y, App.Selected.Width, App.Selected.Height);

            Console.WriteLine("Type Key! Character: " + character);
        }

        public void OnPressKeyCode(Keys keys)
        {
            Console.WriteLine("TextTool.OnPressKeyCode() - Point from System.Drawing is used instead of from System.Windows!");
            if (App.CurrentArt == null)
                return;

            if (App.Selected == Rect.Empty)
                return;

            switch(keys)
            {
                case Keys.Down:
                    App.Selected = new(MainProgram.Selected.Location.X, Math.Clamp(App.Selected.Location.Y + 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Keys.Up:
                    App.Selected = new(MainProgram.Selected.Location.X, Math.Clamp(App.Selected.Location.Y - 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Keys.Right:
                    App.Selected = new(Math.Clamp(MainProgram.Selected.Location.X + 1, 0, App.CurrentArt.Width - 1), MainProgram.Selected.Location.Y, 1, 1);
                    break;
                case Keys.Left:
                    App.Selected = new(Math.Clamp(MainProgram.Selected.Location.X - 1, 0, App.CurrentArt.Width - 1), MainProgram.Selected.Location.Y, 1, 1);
                    break;
                case Keys.Return:
                    App.Selected = new(StartPoint.X, Math.Clamp(MainProgram.Selected.Location.Y + 1, 0, App.CurrentArt.Height - 1), 1, 1);
                    break;
                case Keys.Back:
                    if (App.Selected.X > 0)
                        App.Selected = new(App.Selected.X - 1, App.Selected.Y, App.Selected.Width, App.Selected.Height);

                    App.CurrentArt.Draw(App.CurrentLayerID, new((int)App.Selected.X, (int)App.Selected.Y), null);
                    break;
                default:
                    break;
            }

            if (StartPoint.X > App.Selected.X)
                StartPoint = new((int)App.Selected.X, StartPoint.Y);
        }
    }
}
