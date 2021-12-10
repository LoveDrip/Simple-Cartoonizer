using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cartoonizer
{
    public class BackgroundTypeData : MainTypeData {
        public BackgroundTypeData(int type, RadioButton button, StackPanel panel = null)
            : base(type, button, panel)
        {
            BackgroundNormal subTypeNormal = new BackgroundNormal();
            addSubTypeData(subTypeNormal);
        }
    }

    public class BackgroundNormal : SubTypeData {        
        public BackgroundNormal(RadioButton btn = null) : base(btn) {
            this.type = ELEMINFO_TYPES.BACK;
        }
    }
}
