using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AAP.UI
{
    public class Theme
    {
        public readonly System.Windows.Media.Brush Background = System.Windows.Media.Brushes.White;
        public readonly System.Windows.Media.Brush Art = System.Windows.Media.Brushes.Black;
        public readonly System.Windows.Media.Brush Border = System.Windows.Media.Brushes.Black;

        public readonly System.Windows.Media.Brush Primary = System.Windows.Media.Brushes.White;
        public readonly System.Windows.Media.Brush Secondary = System.Windows.Media.Brushes.Gray;

        public readonly System.Windows.Media.Brush PrimaryActivated = System.Windows.Media.Brushes.Green;
        public readonly System.Windows.Media.Brush SecondaryActivated = System.Windows.Media.Brushes.DarkGreen;

        public Theme()
        {
            Background.Freeze();
            Art.Freeze();
            Border.Freeze();
            Primary.Freeze();
            Secondary.Freeze();
            PrimaryActivated.Freeze();
            SecondaryActivated.Freeze();
        }

        public Theme(System.Windows.Media.Brush background, System.Windows.Media.Brush art, System.Windows.Media.Brush border, System.Windows.Media.Brush primary, System.Windows.Media.Brush secondary, System.Windows.Media.Brush primaryActivated, System.Windows.Media.Brush secondaryActivated)
        {
            Background = background;
            Art = art;
            Border = border;
            Primary = primary; 
            Secondary = secondary;
            PrimaryActivated = primaryActivated;
            SecondaryActivated = secondaryActivated;

            Background.Freeze();
            Art.Freeze();
            Border.Freeze();
            Primary.Freeze();
            Secondary.Freeze();
            PrimaryActivated.Freeze();
            SecondaryActivated.Freeze();
        }
    }
}
