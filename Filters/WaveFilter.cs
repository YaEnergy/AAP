using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Filters
{
    public class WaveFilter : Filter
    {
        public override string Name => App.Language.GetString("Filter_Wave");

        public override string Description => App.Language.GetString("Filter_Wave_Description");

        public ArtLayer Layer { get; set; }

        public Axis2D Axis { get; set; } = Axis2D.X;

        public int Offset { get; set; } = 0;

        /// <summary>
        /// Creates a WaveFilter
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="axis">The axis to execute the pattern on</param>
        /// <param name="offset">Offset of the pattern</param>
        public WaveFilter(ArtLayer layer, Axis2D axis, int offset)
        {
            Layer = layer;
            Axis = axis;
            Offset = offset;
        }

        /// <summary>
        /// Creates a WaveFilter with no offset
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="axis">The axis to execute the pattern on</param>
        /// <param name="speed">How fast the pattern goes up and down</param>
        public WaveFilter(ArtLayer layer, Axis2D axis) : this(layer, axis, 0)
        {

        }

        public override void Apply()
        {
            WaveLayer(Layer, Axis, Offset);
        }

        public static void WaveLayer(ArtLayer layer, Axis2D axis, int offset)
        {
            char?[,] newData;

            switch (axis)
            {
                case Axis2D.X:
                    newData = new char?[layer.Width + 2, layer.Height];

                    for (int y = 0; y < layer.Height; y++)
                    {
                        int waveOffset = (int)Math.Round(Math.Sin(y + offset));

                        for (int x = 0; x < layer.Width; x++)
                            newData[x + 1 + waveOffset, y] = layer.GetCharacter(x, y);
                    }

                    layer.OffsetX -= 1;
                    break;
                case Axis2D.Y:
                    newData = new char?[layer.Width, layer.Height + 2];

                    for (int x = 0; x < layer.Width; x++)
                    {
                        int waveOffset = (int)Math.Round(Math.Sin(x + offset));

                        for (int y = 0; y < layer.Height; y++)
                            newData[x, y + 1 + waveOffset] = layer.GetCharacter(x, y);
                    }

                    layer.OffsetY -= 1;
                    break;
                default:
                    throw new ArgumentException(null, nameof(axis));
            }

            layer.Data = newData;
        }
    }
}
