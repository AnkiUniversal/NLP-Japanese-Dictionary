using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLPJDict.NLPJDictCore.Interfaces
{
    public interface INLPPopup
    {
        bool IsOpen { get; }
        void Hide();
    }
}
