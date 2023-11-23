using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class PencilTool: Tool, ICharacterSelectable, ISizeSelectable, IStayInsideSelectionProperty, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Draw;

        private int size = 1;
        public int Size
        {
            get => size;
            set
            {
                if (size == value)
                    return;

                size = value;

                PropertyChanged?.Invoke(this, new(nameof(Size)));
            }
        }

        private char? character = '/';
        public char? Character
        {
            get => character;
            set
            {
                if (character == value)
                    return;

                character = value;

                PropertyChanged?.Invoke(this, new(nameof(Character)));
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

                PropertyChanged?.Invoke(this, new(nameof(StayInsideSelection)));
            }
        }

        private ArtLayerDraw? layerDraw = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PencilTool(char? character, int size)
        {
            Character = character;
            Size = size;

            App.OnCurrentLayerIDChanged += OpenLayerDraw;
        }

        private void OpenLayerDraw(int id)
        {
            if (App.CurrentArtFile == null)
                return;

            if (id == -1)
                layerDraw = null;

            layerDraw = new(App.CurrentArtFile.Art.ArtLayers[id]);
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawBrush(startArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            DrawBrush(currentArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();
        }

        public void DrawBrush(Point artPos)
        {
            if (App.CurrentArtFile == null || layerDraw == null)
                return;

            layerDraw.StayInsideSelection = StayInsideSelection;

            layerDraw.DrawBrush(Character, artPos, Size);
        }
    }
}
