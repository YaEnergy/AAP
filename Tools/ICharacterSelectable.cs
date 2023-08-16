using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP
{
    internal interface ICharacterSelectable
    {
        public char? Character { get; set; }

        public void SelectCharacter(char? character)
            => Character = character;

    }
}
