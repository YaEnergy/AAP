using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AAP.UI
{
    public class Theme
    {
        public Brush Background { get; set; } = Brushes.White;
        public Brush LightAccent { get; set; } = new SolidColorBrush(Color.FromRgb(242, 242, 242));
        public Brush DarkAccent { get; set; } = new SolidColorBrush(Color.FromRgb(222, 222, 222));

        public Brush Art { get; set; } = Brushes.Black;
        public Brush Border { get; set; } = Brushes.Black;

        public Brush Primary { get; set; } = Brushes.White;
        public Brush Secondary { get; set; } = Brushes.Gray;

        public Brush PrimaryActive { get; set; } = Brushes.Green;
        public Brush SecondaryActive { get; set; } = Brushes.DarkGreen;

        public Theme()
        {

        }

        public Theme(Brush background, Brush art, Brush border, Brush primary, Brush secondary, Brush primaryActive, Brush secondaryActive)
        {
            Background = background;
            Art = art;
            Border = border;
            Primary = primary; 
            Secondary = secondary;
            PrimaryActive = primaryActive;
            SecondaryActive = secondaryActive;
        }
    }
}
