using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.Filters
{
    public class OutlineFilter : Filter
    {
        public override string Name => App.Language.GetString("Filter_Outline");

        public override string Description => App.Language.GetString("Filter_Outline_Description");

        public ASCIIArt? Art { get; set; } = null;
        public ArtLayer? Layer { get; set; } = null;
        public Rect Affect { get; set; } = Rect.Empty;

        public char Character { get; set; }

        /// <summary>
        /// Creates a OutlineFilter for outlining art with a specified character.
        /// </summary>
        /// <param name="art">ASCIIArt to outline</param>
        /// <param name="character">Outline character</param>
        public OutlineFilter(ASCIIArt art, char character)
        {
            Art = art;
            Character = character;
        }

        /// <summary>
        /// Creates a OutlineFilter for outlining a layer with a specified character.
        /// </summary>
        /// <param name="layer">Layer to outline</param>
        /// <param name="character">Outline character</param>
        public OutlineFilter(ArtLayer layer, char character)
        {
            Layer = layer;
            Affect = new(0, 0, layer.Width, layer.Height);
            Character = character;
        }

        /// <summary>
        /// Creates a OutlineFilter for outlining a specific region of a layer with a specified character.
        /// </summary>
        /// <param name="layer">ArtLayer to mirror</param>
        /// <param name="affect">Part of the layer to outline, this does not take offset into account.</param>
        /// <param name="character">Axis to mirror along</param>
        public OutlineFilter(ArtLayer layer, Rect affect, char character)
        {
            Layer = layer;
            Affect = affect;
            Character = character;
        }

        public override void Apply()
        {
            if (Art == null && Layer != null && Affect != Rect.Empty)
                OutlineLayerRect(Layer, Affect, Character);
            else if (Art == null && Layer != null && Affect == Rect.Empty)
                OutlineLayer(Layer, Character);
            else if (Art != null)
                OutlineArt(Art, Character);
            else
                throw new InvalidOperationException();
        }

        public static void OutlineArt(ASCIIArt art, char character)
        {
            foreach (ArtLayer layer in art.ArtLayers)
            {
                OutlineLayer(layer, character);
            }
        }

        public static void OutlineLayer(ArtLayer layer, char character)
            => OutlineLayerRect(layer, 0, 0, layer.Width, layer.Height, character);

        public static void OutlineLayerRect(ArtLayer layer, Rect affect, char character)
            => OutlineLayerRect(layer, (int)affect.X, (int)affect.Y, (int)affect.Width, (int)affect.Height, character);

        public static void OutlineLayerRect(ArtLayer layer, int startX, int startY, int width, int height, char character)
        {
            //add error clauses if outline region is invalid

            char?[][] newData = new char?[layer.Width][];

            for (int x = 0; x < layer.Width; x++)
            {
                newData[x] = new char?[layer.Height];

                for (int y = 0; y < layer.Height; y++)
                {
                    //If this is inside outline region and current character can be an outline
                    if (layer.Data[x][y] == null && x < startX + width && x >= 0 && y < startY + height && y >= 0 && 
                        (x + 1 < layer.Width || layer.Data[x + 1][y] != null || x - 1 >= 0 || layer.Data[x - 1][y] != null || (y + 1 < layer.Height || layer.Data[x][y + 1] != null) || y + 1 < layer.Height || layer.Data[x][y + 1] != null || y + 1 < layer.Height || layer.Data[x][y + 1] != null || y - 1 >= 0 || layer.Data[x][y - 1] != null))
                    {
                        newData[x][y] = character;
                    }
                    else
                        newData[x][y] = layer.Data[x][y];
                    
                }
            }

            layer.Data = newData;
        }
    }
}
