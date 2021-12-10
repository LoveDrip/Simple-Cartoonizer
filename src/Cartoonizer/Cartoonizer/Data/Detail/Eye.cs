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
    public class EyeBrow : SubTypeData {
        public EyeBrow(RadioButton btn): base(btn) {
            this.type = ELEMINFO_TYPES.EYE_BROW;
            nDefualtImgIdx = 1;
            bHasPairElem = true;
        }
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowMoveV = true;
            resource.bShowMoveH = true;
            resource.bShowEnlarge = true;
            resource.bShowAlpha = true;
            resource.bShowColor = true;
        }
        public override void initColors()
        {
            addColor(Color.FromRgb(0, 0, 0));
            addColor(Color.FromRgb(25, 29, 41));
            addColor(Color.FromRgb(15, 26, 12));
            addColor(Color.FromRgb(9, 21, 47));
            addColor(Color.FromRgb(48, 33, 64));
            addColor(Color.FromRgb(27, 42, 65));
            addColor(Color.FromRgb(45, 22, 48));
            addColor(Color.FromRgb(42, 22, 15));
            addColor(Color.FromRgb(19, 17, 18));
            addColor(Color.FromRgb(28, 25, 42));
            addColor(Color.FromRgb(10, 17, 46));
            addColor(Color.FromRgb(9, 46, 12));
            addColor(Color.FromRgb(46, 8, 19));
            addColor(Color.FromRgb(88, 35, 17));
            addColor(Color.FromRgb(34, 13, 52));
            addColor(Color.FromRgb(21, 58, 77));
            addColor(Color.FromRgb(215, 247, 244));
            addColor(Color.FromRgb(94, 162, 165));
            addColor(Color.FromRgb(120, 44, 118));
            addColor(Color.FromRgb(88, 125, 144));
            base.initColors();
        }
    }

    public class EyeGlass : SubTypeData
    {
        public EyeGlass(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.EYE_GLASS;            
        }
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowMoveV = true;
            resource.bShowMoveH = true;
            resource.bShowAlpha = true;
        }
    }

    public class EyeShape : SubTypeData {
        public EyeShape(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.EYE_SHAPE;
            nDefualtImgIdx = 1;
            bHasPairElem = true;
        }        
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowMoveV = true;
            resource.bShowMoveH = true;
            resource.bShowZoom = true;
            resource.bShowEnlarge = true;
            resource.bShowAlpha = true;
        }
    }

    public class EyeIris : SubTypeData {
        public EyeIris(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.EYE_IRIS;
            nDefualtImgIdx = 1;
        } 
        public override void initControlPanel()
        {
            base.initControlPanel();
            resource.bShowMoveV = true;
            resource.bShowMoveH = true;
            resource.bShowAlpha = true;
        }
    }

    public class EyeMakeup : SubTypeData {
        public EyeMakeup(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.EYE_MAKEUP;
            nDefualtImgIdx = 1;
        } 
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowAlpha = true;
        }
        public override void initImages() {
            base.initImages();
        }
    }
}
