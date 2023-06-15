using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class TextTool: Tool
    {
        public override void ActivateStart(Point location)
        {
            if (MainProgram.CurrentArt == null)
                return;

            base.ActivateStart(location);

            MainProgram.Selected = new(Math.Clamp(location.X, 0, MainProgram.CurrentArt.Width), Math.Clamp(location.Y, 0, MainProgram.CurrentArt.Height), 1, 1);
        }

        public static void TypeKeyCharacter(char character)
        {
            if (MainProgram.CurrentArt == null)
                return;

            if (MainProgram.Selected == Rectangle.Empty)
                return;

            if (char.IsControl(character)) 
                return;

            if (char.IsWhiteSpace(character) && character != ' ') 
                return;

            MainProgram.CurrentArt.Draw(MainProgram.CurrentLayerID, MainProgram.Selected.Location, character == ' ' ? null : character);
            
            if(MainProgram.Selected.X < MainProgram.CurrentArt.Width)
                MainProgram.Selected = new(MainProgram.Selected.X + 1, MainProgram.Selected.Y, MainProgram.Selected.Width, MainProgram.Selected.Height);

            Console.WriteLine("Type Key! Character: " + character);
        }

        public void OnPressKeyCode(Keys keys)
        {
            if (MainProgram.CurrentArt == null)
                return;

            if (MainProgram.Selected == Rectangle.Empty)
                return;

            switch(keys)
            {
                case Keys.Down:
                    MainProgram.Selected = new(MainProgram.Selected.Location.X, Math.Clamp(MainProgram.Selected.Location.Y + 1, 0, MainProgram.CurrentArt.Height - 1), 1, 1);
                    break;
                case Keys.Up:
                    MainProgram.Selected = new(MainProgram.Selected.Location.X, Math.Clamp(MainProgram.Selected.Location.Y - 1, 0, MainProgram.CurrentArt.Height - 1), 1, 1);
                    break;
                case Keys.Right:
                    MainProgram.Selected = new(Math.Clamp(MainProgram.Selected.Location.X + 1, 0, MainProgram.CurrentArt.Width - 1), MainProgram.Selected.Location.Y, 1, 1);
                    break;
                case Keys.Left:
                    MainProgram.Selected = new(Math.Clamp(MainProgram.Selected.Location.X - 1, 0, MainProgram.CurrentArt.Width - 1), MainProgram.Selected.Location.Y, 1, 1);
                    break;
                case Keys.Return:
                    MainProgram.Selected = new(StartPoint.X, Math.Clamp(MainProgram.Selected.Location.Y + 1, 0, MainProgram.CurrentArt.Height - 1), 1, 1);
                    break;
                case Keys.Back:
                    if (MainProgram.Selected.X > 0)
                        MainProgram.Selected = new(MainProgram.Selected.X - 1, MainProgram.Selected.Y, MainProgram.Selected.Width, MainProgram.Selected.Height);

                    MainProgram.CurrentArt.Draw(MainProgram.CurrentLayerID, MainProgram.Selected.Location, null);
                    break;
                default:
                    break;
            }

            if (StartPoint.X > MainProgram.Selected.X)
                StartPoint = new(MainProgram.Selected.X, StartPoint.Y);
        }
    }
}
