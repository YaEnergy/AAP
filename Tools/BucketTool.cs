using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    public class BucketTool: Tool
    {
        public override ToolType Type { get; protected set; } = ToolType.Bucket;

        public char? Character = '/';

        public BucketTool(char? character)
        {
            Character = character;
        }

        public override void ActivateStart(Point artMatrixPosition)
        {
            base.ActivateStart(artMatrixPosition);

            FillArea(artMatrixPosition);
        }

        public void FillArea(Point artMatrixPosition)
            => throw new NotImplementedException();
    }
}
