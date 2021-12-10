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
    public class ClothTypeData : MainTypeData {
        public ClothTypeData(int type, RadioButton button, StackPanel panel = null) : base(type, button, panel) {
            ClothNormal subTypeNormal = new ClothNormal();
            addSubTypeData(subTypeNormal);
        }        
    }

    public class ClothNormal : SubTypeData {
        public ClothNormal(RadioButton btn = null) : base(btn) {
            this.type = ELEMINFO_TYPES.CLOTH;
        }
    }
}
