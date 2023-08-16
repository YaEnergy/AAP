using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class BucketTool: Tool, ICharacterSelectable
    {
        public override ToolType Type { get; } = ToolType.Bucket;

        public char? Character { get; set; }

        public BucketTool(char? character)
        {
            Character = character;
        }

        protected override void UseStart(Point startArtPos)
        {
            FillArea(startArtPos);
        }

        public void FillArea(Point artMatrixPosition)
            => throw new NotImplementedException();
    }
}
