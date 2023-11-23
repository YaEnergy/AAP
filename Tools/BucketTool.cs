using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class BucketTool: Tool, ICharacterSelectable, IEightDirectionalProperty, IStayInsideSelectionProperty, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Bucket;

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

        private bool eightDirectional = false;
        public bool EightDirectional
        {
            get => eightDirectional;
            set
            {
                if (eightDirectional == value)
                    return;

                eightDirectional = value;

                PropertyChanged?.Invoke(this, new(nameof(EightDirectional)));
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public BucketTool(char? character)
        {
            Character = character;
        }

        protected override void UseStart(Point startArtPos)
        {
            FloodFillArea(startArtPos);
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();

            App.CurrentArtFile?.Art.Update();
        }

        public void FloodFillArea(Point artPos)
        {
            if (App.CurrentArtFile == null)
                return;

            ASCIIArtFile artFile = App.CurrentArtFile;
            Stack<Point> positionStack = new();

            ArtLayer artLayer = artFile.Art.ArtLayers[App.CurrentLayerID];
            ArtLayerDraw layerDraw = new(artLayer);
            
            Point layerPos = artLayer.GetLayerPoint(artPos);

            if (!layerDraw.CanDrawOn(layerPos))
                return;

            char? findCharacter = artLayer.GetCharacter(layerPos);
            if (findCharacter == Character)
                return; //No changes will be made

            //Flood Fill Algorithm
            positionStack.Push(layerPos);

            layerDraw.StayInsideSelection = StayInsideSelection;

            while (positionStack.Count > 0)
            {
                Point pos = positionStack.Pop(); 

                if (!layerDraw.CanDrawOn(pos))
                    continue;

                if (artLayer.GetCharacter(pos) != findCharacter)
                    continue;

                ConsoleLogger.Log(pos.ToString());

                layerDraw.DrawCharacter(Character, pos);

                int x = (int)pos.X;
                int y = (int)pos.Y;

                if (x + 1 < artLayer.Width)
                    positionStack.Push(new(x + 1, y));

                if (x - 1 >= 0)
                    positionStack.Push(new(x - 1, y));

                if (y + 1 < artLayer.Height)
                    positionStack.Push(new(x, y + 1));

                if (y - 1 >= 0)
                    positionStack.Push(new(x, y - 1));

                if (EightDirectional)
                {
                    if (x + 1 < artLayer.Width)
                    {
                        if (y + 1 < artLayer.Height)
                            positionStack.Push(new(x + 1, y + 1));

                        if (y - 1 >= 0)
                            positionStack.Push(new(x + 1, y - 1));
                    }

                    if (x - 1 >= 0)
                    {
                        if (y + 1 < artLayer.Height)
                            positionStack.Push(new(x - 1, y + 1));

                        if (y - 1 >= 0)
                            positionStack.Push(new(x - 1, y - 1));
                    }
                }
            }
        }
    }
}
