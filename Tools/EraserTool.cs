﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class EraserTool: Tool, ISizeSelectable, IStayInsideSelectionProperty, INotifyPropertyChanged
    {
        public override ToolType Type { get; } = ToolType.Eraser;

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

        public EraserTool(int size)
        {
            Size = size;
        }

        protected override void UseStart(Point startArtPos)
        {
            DrawCircle(startArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseUpdate(Point startArtPos, Point currentArtPos)
        {
            DrawCircle(currentArtPos);

            App.CurrentArtFile?.Art.Update();
        }

        protected override void UseEnd(Point startArtPos, Point endArtPos)
        {
            App.CurrentArtFile?.ArtTimeline.NewTimePoint();
        }

        public void DrawCircle(Point artPos)
            => App.CurrentArtFile?.ArtDraw.DrawFilledCircle(App.CurrentLayerID, null, artPos, Size - 1, StayInsideSelection);
    }
}
