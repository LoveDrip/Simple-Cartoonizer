using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cartoonizer
{
    public class FaceShapeImageData : ImageData {
        public string pathNeck { get; set; }
        public BitmapImage neck { get; set; }        

        public FaceShapeImageData(string imgPath, string thumbPath, string neckPath) : base(imgPath, thumbPath){            
            try {
                this.pathNeck = neckPath;
                this.neck = new BitmapImage(new Uri(neckPath));                
            } catch {
                try {
                    this.pathNeck = "pack://application:,,,/Assets/neck.png";
                    this.neck = new BitmapImage(new Uri(this.pathNeck));   
                }catch(Exception) {
                    this.pathNeck = "";
                    this.neck = null;
                }
            }
        }

        public FaceShapeImageData(string imgPath, string thumbPath) : base(imgPath, thumbPath) {
            try {
                this.pathNeck = "pack://application:,,,/Images/Assets/neck.png";
                this.neck = new BitmapImage(new Uri(this.pathNeck));
            } catch (Exception) {
                this.pathNeck = "";
                this.neck = null;
            }            
        }
        
        public FaceShapeImageData() : base() {
            this.pathNeck = "";
            this.neck = null;
            this.Disabled = true; // Face Shape shows empty icon, but user cannot interact with it! This is only for reset
        }
    }

    public class FaceShape : SubTypeData
    {
        public FaceShape(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.FACE_SHAPE;
            //bHasEmptyImage = false;    
            nDefualtImgIdx = 1;       
        }       
        public override void initControlPanel()
        {
            base.initControlPanel();
            resource.bShowAlpha = false;
            resource.bShowColor = true;
            resource.bShowMoveH = true;
            resource.bShowMoveV = true;
            resource.bShowZoom = true;
        }
        public override void initImages() {
            //base.initImages();
            addImage(new FaceShapeImageData());            
        }

        public override ImageData getImageDataFromResourceEntry(Hashtable oneResEntry) {
            ImageData id = base.getImageDataFromResourceEntry(oneResEntry);
            if (id == null) return null;

            if (oneResEntry.ContainsKey("neck")) {
                string strNeck = (string)oneResEntry["neck"];
                return new FaceShapeImageData(id.pathImg, id.pathThumb, strNeck);
            } else
                return new FaceShapeImageData(id.pathImg, id.pathThumb);            
        }

        public override void initColors() {
            addColor(Color.FromRgb(243, 212, 207));
            addColor(Color.FromRgb(255, 208, 188));
            addColor(Color.FromRgb(217, 184, 175));
            addColor(Color.FromRgb(217, 164, 148));
            addColor(Color.FromRgb(233, 185, 149));
            addColor(Color.FromRgb(245, 175, 149));
            addColor(Color.FromRgb(225, 158, 149));
            addColor(Color.FromRgb(240, 199, 177));
            addColor(Color.FromRgb(218, 164, 136));
            addColor(Color.FromRgb(242, 170, 146));
            addColor(Color.FromRgb(236, 196, 184));
            addColor(Color.FromRgb(246, 228, 226));
            addColor(Color.FromRgb(238, 170, 132));
            addColor(Color.FromRgb(205, 161, 132));
            addColor(Color.FromRgb(147, 97, 74));
            addColor(Color.FromRgb(118, 70, 48));
            addColor(Color.FromRgb(117, 57, 21));
            addColor(Color.FromRgb(88, 40, 18));
            addColor(Color.FromRgb(179, 106, 52));
            addColor(Color.FromRgb(76, 45, 24));
            base.initColors();
        }

        protected override bool onImageChanged(ImageData oldImage, ImageData newImage, bool bOnlyImage)
        {
            bool b = base.onImageChanged(oldImage, newImage, bOnlyImage);
            if (!b) return false;
            if (this.body != null) {
                Color c = Colors.Transparent;
                c = this.bodyElem.m_colorMask;

                if (this.body.m_biNeck != null) {
                    FaceShapeImageData fid = (FaceShapeImageData)newImage;
                    this.body.m_biNeck.changeImage(fid.pathNeck);
                    this.body.m_biNeck.maskColor(c);
                }
                if (this.body.m_biFaceEar != null) this.body.m_biFaceEar.maskColor(c);
            }
            return b;
        }
        protected override void maskColor(Color color) {
            base.maskColor(color);
            if (this.body != null) {
                if (this.body.m_biNeck != null) this.body.m_biNeck.maskColor(color);
                if (this.body.m_biFaceEar != null) this.body.m_biFaceEar.maskColor(color);
            }            
        }
        public override void updateView() {
            if (bodyElem != null) {
                maskColor(bodyElem.m_colorMask);
            }
        }

        protected override bool onControlZoomIn() {
            if (bodyElem != null) {
                return bodyElem.zoomInX();
            }
            return false;
        }
        protected override bool onControlZoomOut()
        {
            if (bodyElem != null) {
                return bodyElem.zoomOutX();
            }
            return false;
        }
    }
    public class FaceMouth : SubTypeData {
        public FaceMouth(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.FACE_MOUTH;
            nDefualtImgIdx = 1;
        }
        public override void initControlPanel() {
            base.initControlPanel(); 
            resource.bShowColor = true;
            resource.bShowAlpha = true;
            resource.bShowMoveV = true;
            resource.bShowMoveH = true;
            resource.bShowZoom = true;
        }        
        public override void initColors() {
            addColor(Colors.Brown);
            addColor(Colors.Crimson);
            addColor(Colors.DarkRed);
            addColor(Colors.Red);
            addColor(Colors.OrangeRed);
            addColor(Colors.DeepPink);
            addColor(Colors.Red);
            addColor(Colors.Tomato);
            addColor(Colors.Fuchsia);
            addColor(Colors.HotPink);
            addColor(Colors.MediumVioletRed);
            addColor(Colors.DarkRed);
            addColor(Colors.Red);
            addColor(Colors.OrangeRed);
            addColor(Colors.DeepPink);
            addColor(Colors.Red);
            addColor(Colors.Tomato);
            addColor(Colors.Fuchsia);
            addColor(Colors.HotPink);
            addColor(Colors.MediumVioletRed);
            base.initColors();
        }
    }

    public class FaceNose : SubTypeData {
        public FaceNose(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.FACE_NOSE;
            nDefualtImgIdx = 1;
        }
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowColor = false;
            resource.bShowAlpha = true;
            resource.bShowMoveV = true;
            resource.bShowMoveH = true;
            resource.bShowZoom = true;
        }
    }
    public class FaceEar : SubTypeData {        
        public FaceEar(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.FACE_EAR;
            nDefualtImgIdx = 1;
            bHasPairElem = true;
        }
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowMoveV = true;
        }
    }

    public class FaceCheer : SubTypeData {
        public FaceCheer(RadioButton btn) : base(btn) {
            this.type = ELEMINFO_TYPES.FACE_CHEER;            
        }
        public override void initControlPanel() {
            base.initControlPanel();
            resource.bShowMoveH = true;
            resource.bShowMoveV = true;
        }
    }
}
