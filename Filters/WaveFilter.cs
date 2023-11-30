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

        public int Strength { get; set; } = 1;
        public int Speed { get; set; } = 1;
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Creates a WaveFilter
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="axis">The axis to execute the pattern on</param>
        /// <param name="strength">The max & min height offset of the pattern</param>
        /// <param name="speed">How fast the pattern goes up and down</param>
        /// <param name="offset">Offset of the pattern</param>
        public WaveFilter(ArtLayer layer, Axis2D axis, int strength, int speed, int offset)
        {
            Layer = layer;
            Axis = axis;
            Strength = strength;
            Speed = speed;
            Offset = offset;
        }

        /// <summary>
        /// Creates a WaveFilter with no offset
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="axis">The axis to execute the pattern on</param>
        /// <param name="strength">The max & min height offset of the pattern</param>
        /// <param name="speed">How fast the pattern goes up and down</param>
        public WaveFilter(ArtLayer layer, Axis2D axis, int strength, int speed) : this(layer, axis, strength, speed, 0)
        {

        }

        /// <summary>
        /// Creates a WaveFilter with speed set to 1 and no offset
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="axis">The axis to execute the pattern on</param>
        /// <param name="strength">The max & min height offset of the pattern</param>
        public WaveFilter(ArtLayer layer, Axis2D axis, int strength) : this(layer, axis, strength, 1, 0)
        {

        }

        /// <summary>
        /// Creates a WaveFilter with strength and speed set to 1 and no offset
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="axis">The axis to execute the pattern on</param>
        public WaveFilter(ArtLayer layer, Axis2D axis): this(layer, axis, 1, 1, 0)
        {

        }

        public override void Apply()
        {
            WaveLayer(Layer, Axis, Strength, Speed, Offset);
        }

        public static void WaveLayer(ArtLayer layer, Axis2D axis, int strength, int speed, int offset)
        {
            char?[,] newData;

            switch (axis)
            {
                case Axis2D.X:
                    newData = new char?[layer.Width + strength * 2, layer.Height];

                    for (int y = 0; y < layer.Height; y++)
                    {
                        int waveOffset = (int)Math.Round(Math.Sin(speed * y + offset) * strength);

                        for (int x = 0; x < layer.Width; x++)
                            newData[x + waveOffset, y] = layer.GetCharacter(x, y);
                    }

                    layer.OffsetX -= strength;
                    break;
                case Axis2D.Y:
                    newData = new char?[layer.Width, layer.Height + strength * 2];

                    for (int x = 0; x < layer.Width; x++)
                    {
                        int waveOffset = (int)Math.Round(Math.Sin(speed * x + offset) * strength);

                        for (int y = 0; y < layer.Height; y++)
                            newData[x, y + waveOffset] = layer.GetCharacter(x, y);
                    }

                    layer.OffsetY -= strength;
                    break;
                default:
                    throw new ArgumentException(null, nameof(axis));
            }

            layer.Data = newData;
        }
    }
}
