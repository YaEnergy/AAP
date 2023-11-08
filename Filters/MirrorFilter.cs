using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Filters
{
    public class MirrorFilter : Filter
    {
        public override string Name => App.Language.GetString("Filter_Mirror");

        public override string Description => App.Language.GetString("Filter_Mirror_Description");

        public ASCIIArt? Art { get; set; } = null;
        public ArtLayer? Layer { get; set; } = null;
        public Rect Affect { get; set; } = Rect.Empty;

        public Axis2D MirrorAxis { get; set; }

        /// <summary>
        /// Creates a MirrorFilter for mirroring the specified art canvas along an axis
        /// </summary>
        /// <param name="art">ASCIIArt to mirror</param>
        /// <param name="mirrorAxis">Axis to mirror along</param>
        public MirrorFilter(ASCIIArt art, Axis2D mirrorAxis)
        {
            Art = art;
            MirrorAxis = mirrorAxis;
        }

        /// <summary>
        /// Creates a MirrorFilter for mirroring the specified layer along an axis
        /// </summary>
        /// <param name="layer">ArtLayer to mirror</param>
        /// <param name="mirrorAxis">Axis to mirror along</param>
        public MirrorFilter(ArtLayer layer, Axis2D mirrorAxis)
        {
            Layer = layer;
            Affect = new(0, 0, layer.Width, layer.Height);
            MirrorAxis = mirrorAxis;
        }

        /// <summary>
        /// Creates a MirrorFilter for mirroring the specified rect of an artlayer along an axis
        /// </summary>
        /// <param name="layer">ArtLayer to mirror</param>
        /// <param name="affect">Part of the layer to mirror, this does not take offset into account.</param>
        /// <param name="mirrorAxis">Axis to mirror along</param>
        public MirrorFilter(ArtLayer layer, Rect affect, Axis2D mirrorAxis)
        {
            Layer = layer;
            Affect = affect;
            MirrorAxis = mirrorAxis;
        }

        public override void Apply()
        {
            if (Art == null && Layer != null && Affect != Rect.Empty)
                MirrorLayerRect(Layer, Affect, MirrorAxis);
            else if (Art == null && Layer != null && Affect == Rect.Empty)
                MirrorLayer(Layer, MirrorAxis);
            else if (Art != null)
                MirrorArt(Art, MirrorAxis);
            else
                throw new InvalidOperationException();
        }

        public static void MirrorArt(ASCIIArt art, Axis2D mirrorAxis)
        {
            foreach (ArtLayer layer in art.ArtLayers)
            {
                MirrorLayer(layer, mirrorAxis);

                switch(mirrorAxis)
                {
                    case Axis2D.X:
                        layer.OffsetX = -layer.OffsetX;
                        break;
                    case Axis2D.Y:
                        layer.OffsetY = -layer.OffsetY;
                        break;
                }
            }
        }

        public static void MirrorLayer(ArtLayer layer, Axis2D mirrorAxis)
            => MirrorLayerRect(layer, 0, 0, layer.Width, layer.Height, mirrorAxis);

        public static void MirrorLayerRect(ArtLayer layer, Rect affect, Axis2D mirrorAxis)
            => MirrorLayerRect(layer, (int)affect.X, (int)affect.Y, (int)affect.Width, (int)affect.Height, mirrorAxis);

        public static void MirrorLayerRect(ArtLayer layer, int startX, int startY, int width, int height, Axis2D mirrorAxis)
        {
            char?[][] newData = new char?[layer.Width][];

            for (int x = 0; x < layer.Width; x++)
                newData[x] = new char?[layer.Height];

            switch (mirrorAxis)
            {
                case Axis2D.X:
                    for (int x = 0; x < layer.Width; x++)
                        for (int y = 0; y < layer.Height; y++)
                            // If inside mirror region, mirror.
                            if (x >= startX && y >= startY && x < startX + width && y < startY + height)
                                newData[x][y] = layer.Data[startX * 2 + width - x - 1][y];
                            else
                                newData[x][y] = layer.Data[x][y];

                    break;
                case Axis2D.Y:
                    for (int x = 0; x < layer.Width; x++)
                        for (int y = 0; y < layer.Height; y++)
                            //If inside mirror region, mirror.
                            if (x >= startX && y >= startY && x < startX + width && y < startY + height)
                                newData[x][y] = layer.Data[x][startY * 2 + height - y - 1];
                            else
                                newData[x][y] = layer.Data[x][y];

                    break;
            }

            layer.Data = newData;
        }
    }

    public enum Axis2D
    {
        X = 1,
        Y = 2
    }
}
