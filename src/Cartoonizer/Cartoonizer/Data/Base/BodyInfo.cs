using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cartoonizer
{
    public class HisInfo {
        public int idx;
    }
    public class HistoryElemInfo : HisInfo {        
        public double tX, tY;
        public double s_centerX, sX, s_centerY, sY;
        public double angle, r_centerX, r_centerY;
        public int mX, mY;
        public double centerX, centerY;
        public float scale, rotate, alpha;
        public Color color;
    }
    public class HistoryPairElemInfo : HisInfo {
        public HistoryElemInfo left;
        public HistoryElemInfo right;
    }

    public class HistoryInfo
    {
        public HistoryElemInfo back;
        public HistoryElemInfo neck;
        public HistoryElemInfo faceShape;
        public HistoryElemInfo faceMouth;
        public HistoryElemInfo faceNose;
        public HistoryElemInfo faceCheer;
        public HistoryPairElemInfo faceEar;

        public HistoryPairElemInfo eyeShape;
        public HistoryPairElemInfo eyeBrow;
        public HistoryElemInfo eyeGlass;

        public HistoryElemInfo hairHair;
        public HistoryElemInfo hairAccessory;
        public HistoryElemInfo hairBeard;
        public HistoryElemInfo hairMustache;

        public HistoryElemInfo cloth;
    }

    public class IElemInfo
    {
        public int m_nIdx = -1;
        public Color m_colorMask = Colors.Transparent;
        public ELEMINFO_TYPE m_eType = ELEMINFO_TYPES.BACK;
        //public abstract virtual bool changeIndex(int idx);
        public virtual bool changeImage(string uri, bool bOnlyImage = false) {return false;}
        public virtual bool updateView() { return false; }
        public virtual bool moveLeft() { return false; }
        public virtual bool moveRight() { return false; }
        public virtual bool moveUp() { return false; }
        public virtual bool moveDown() { return false; }
        public virtual bool rotateLeft() { return false; }
        public virtual bool rotateRight() { return false; }
        public virtual bool zoomIn() { return false; }
        public virtual bool zoomOut() { return false; }
        public virtual bool zoomInX() { return false; }
        public virtual bool zoomOutX() { return false; }
        public virtual bool enlarge() { return false; }
        public virtual bool narrow() { return false; }
        public virtual bool maskColor(Color color) { return false; }
        public virtual bool changeAlpha(float alpha) { return false; }
    }
    public class Range {
        public bool bApplyMinMax = false;
        public float min = 0;
        public float max = 0;
        public float offset = 0;

        public Range(float offset) {
            this.offset = offset;
        }
        public Range(float min, float max) {
            this.min = min;
            this.max = max;
            this.bApplyMinMax = true;
        }
        public Range(float offset, float min, float max) : this(min,max) {
            this.offset = offset;            
        }
    }
    public class ElemInfo : IElemInfo {          
        public Point m_ptCenter = new Point(512, 512);
        public Point m_ptDefaultCenter = new Point(512, 512);
        public int m_nDefaultMoveX = 0;
        public int m_nDefaultMoveY = 0;
        public float m_fDefaultRotate = 0;
        public float m_fDefaultScale = 1;        
        
        public int m_nMoveX = 0;
        public int m_nMoveY = 0;
        public float m_fRotate = 0;
        public float m_fScale = 1;        
        public float m_fMaskOpacity = 1.0f;

        public Range m_rangeMoveX = new Range(2);
        public Range m_rangeMoveY = new Range(2);
        public Range m_rangeScale = new Range(0.05f, 0, 100);
        public Range m_rangeRotate = new Range(1, -20, 20); 
                
        public string m_strUri = "";
        public BitmapImage m_bitmap = null;
        
        public TranslateTransform m_translate = null;
        public ScaleTransform m_scale = null;
        public RotateTransform m_rotate = null;

        public Rectangle m_rectMask = null;
        public SolidColorBrush m_brushMask = null;
        public ImageBrush m_imgBrush = null;
        public Image m_image = null;

        public ElemInfo() { }
        public ElemInfo(ELEMINFO_TYPE type) { m_eType = type; }

        public void setTransforms(TranslateTransform translate = null, ScaleTransform scale = null, RotateTransform rotate = null)
        {
            m_translate = translate;
            m_scale = scale;
            m_rotate = rotate;
        }

        public void setImage(Image image) { m_image = image; }

        public void setMask(Rectangle rect = null, SolidColorBrush brush = null, ImageBrush imgBrush = null)
        {
            m_rectMask = rect;
            m_imgBrush = imgBrush;
            m_brushMask = brush;
        }

        public void setDefaultCenterPt(Point pt) { m_ptCenter = m_ptDefaultCenter = pt; }

        public void initTransforms() {
            m_ptCenter = m_ptDefaultCenter;
            m_nMoveX = m_nDefaultMoveX;
            m_nMoveY = m_nDefaultMoveY;
            m_fRotate = m_fDefaultRotate;
            m_fScale = m_fDefaultScale;

            if (m_translate != null) {
                m_translate.Y = m_nMoveY;
                m_translate.X = m_nMoveX;
            } 
            if (m_rotate != null) {
                m_rotate.Angle = m_fRotate;
                m_rotate.CenterX = m_ptCenter.X;
                m_rotate.CenterY = m_ptCenter.Y;
            } 
            if (m_scale != null) {
                m_scale.ScaleX = m_fScale;
                m_scale.ScaleY = m_fScale;
            }
            //changeImage(m_strUri);
        }
        public void moveLeft(object s, EventArgs e) { moveLeft(); }
        public void moveRight(object s, EventArgs e) { moveRight(); }
        public void moveUp(object s, EventArgs e) { moveUp(); }
        public void moveDown(object s, EventArgs e) { moveDown(); }
        public void rotateLeft(object s, EventArgs e) { rotateLeft(); }
        public void rotateRight(object s, EventArgs e) { rotateRight(); }
        public void zoomIn(object s, EventArgs e) { zoomIn(); }
        public void zoomOut(object s, EventArgs e) { zoomOut(); }

        public bool moveVertical(int nSign = 1)
        {
            if (m_translate == null) return false;
            m_nMoveY = (int)m_translate.Y;

            int newY = m_nMoveY + (int) m_rangeMoveY.offset * nSign;
            if (m_rangeMoveY.bApplyMinMax && (newY < m_rangeMoveY.min || newY > m_rangeMoveY.max)) {
                return false;
            }            
            m_translate.Y = m_nMoveY = newY;
            return true;
        }
        public bool moveHorizontal(int nSign = 1)
        {
            if (m_translate == null) return false;
            m_nMoveX = (int)m_translate.X;
            int newX = m_nMoveX + (int)m_rangeMoveX.offset * nSign;
            if (m_rangeMoveX.bApplyMinMax && (newX < m_rangeMoveX.min || newX > m_rangeMoveX.max)) {
                return false;
            }
            m_translate.X = m_nMoveX = newX;
            return true;
        }
        public bool rotate(float nSign = 1)
        {
            if (m_rotate == null) return false;
            m_fRotate = (float)m_rotate.Angle;

            float newR = m_fRotate + m_rangeRotate.offset * nSign;
            if (m_rangeRotate.bApplyMinMax && (newR < m_rangeRotate.min || newR > m_rangeRotate.max)) {
                return false;
            }
            m_rotate.Angle = m_fRotate = newR;
            m_rotate.CenterX = m_ptCenter.X;
            m_rotate.CenterY = m_ptCenter.Y;
            return true;
        }

        public bool rotateByVal(int nCenterX, int nCenterY, float angle)
        {
            if (m_rotate == null) return false;
            m_rotate.Angle = angle;
            m_rotate.CenterX = nCenterX;
            m_rotate.CenterY = nCenterY;
            return true;
        }
        public bool scale(float nSign = 1, bool bApplyX = true, bool bApplyY = true) {
            if (m_scale == null) return false;
            m_fScale = (float)m_scale.ScaleX;

            float newS = m_fScale + m_rangeScale.offset * nSign;
            if (m_rangeScale.bApplyMinMax && (newS < m_rangeScale.min || newS > m_rangeScale.max)) {
                return false;
            }
            m_fScale = newS;
            m_scale.CenterX = m_ptCenter.X;
            m_scale.CenterY = m_ptCenter.Y;
            m_scale.ScaleX = m_fScale;
            if (bApplyY)  m_scale.ScaleY = m_fScale;
            return true;
        }
        public bool scaleX(float nSign = 1)
        {
            return scale(nSign, true, false);            
        }

        ///////////////////////////////////////////////////////
        // Override Methods
        ///         
        public override bool changeImage(string uri, bool bOnlyImage = false) {            
            if (m_strUri == uri && uri != "") return false;
            m_strUri = uri;
            //initTransforms();
            if (m_image == null) return false;
            try {                
                m_image.Source = new BitmapImage(new Uri(m_strUri));
                if (m_imgBrush != null)
                    m_imgBrush.ImageSource = new BitmapImage(new Uri(m_strUri));
                if (!bOnlyImage) {
                    maskColor(Colors.Transparent);
                    changeAlpha(1.0f);
                }                
            } catch (Exception) {
                m_image.Source = null;
                if (m_imgBrush != null)
                    m_imgBrush.ImageSource = null;
                Console.WriteLine("bitmapImage exception");
            }
            return true;
        }       

        public override bool updateView() {
            if (m_translate != null) {
                m_translate.Y = m_nMoveY;
                m_translate.X = m_nMoveX;
            }
            if (m_rotate != null) {
                m_rotate.Angle = m_fRotate;
                m_rotate.CenterX = m_ptCenter.X;
                m_rotate.CenterY = m_ptCenter.Y;
            } 
            if (m_scale != null)
            {
                m_scale.ScaleX = m_fScale;
                m_scale.ScaleY = m_fScale;
            }
            maskColor(m_colorMask);
            return true;
        }
        public override bool zoomInX() { return scaleX(-1); }
        public override bool zoomOutX() { return scaleX(1); }
        public override bool moveLeft() { return moveHorizontal(-1); }
        public override bool moveRight() { return moveHorizontal(); }
        public override bool moveUp() { return moveVertical(-1); }
        public override bool moveDown() { return moveVertical(); }
        public override bool rotateLeft() { return rotate(-1); }
        public override bool rotateRight() { return rotate(); }
        public override bool zoomIn() { return scale(-1); }
        public override bool zoomOut() { return scale(1); }
        public override bool maskColor(Color color) {
            m_colorMask = color;
            if (m_rectMask == null || m_brushMask == null || m_imgBrush == null) return false;
            try {
                m_rectMask.Opacity = 0.7f;
                m_brushMask.Color = m_colorMask;
            }
            catch (Exception) {
                m_brushMask.Color = Colors.Transparent;
            }
            return true;
        }

        public override bool changeAlpha(float alpha) {
            m_fMaskOpacity = alpha;
            if (m_image == null) {
                return false;
            }
            m_image.Opacity = alpha;
            if (m_rectMask != null) {
                m_rectMask.Opacity = 0.7f * alpha;
            }
            return true;
        }
    }
    
    public class FaceShapeInfo : ElemInfo {
        public PairElemInfo earInfo;
        public ElemInfo neckInfo;
        public FaceShapeInfo(ELEMINFO_TYPE type) : base(type) {
            m_rangeRotate = new Range(5, -7, 7);
        }
    }

    public class BackgroundInfo : ElemInfo {        
        public BackgroundInfo(ELEMINFO_TYPE type) : base(type){             
            m_rangeMoveX.offset = 10;
            m_rangeMoveY.offset = 10;
            m_rangeScale.offset = 0.02f;
            m_rangeRotate.offset = 5;
            m_nMoveX = m_nDefaultMoveX = -337;
            m_nMoveY = m_nDefaultMoveY = -337;
            m_fRotate = m_fDefaultRotate = 0;
            m_fScale = m_fDefaultScale = 0.34f;  
        }
    }
    public class SingleEyeShapeInfo : ElemInfo
    {
        public ImageBrush m_ibIrisBackMask = null;
        public ImageBrush m_ibIrisTransMask = null;
        public bool m_bLeft = false;

        public SingleEyeShapeInfo(ELEMINFO_TYPE type, bool isLeft = false) : base(type) {
            m_bLeft = isLeft;
        }
        public void setMask(ImageBrush ibIrisBackMask, ImageBrush ibIrisTransMask)
        {
            m_ibIrisBackMask = ibIrisBackMask;
            m_ibIrisTransMask = ibIrisTransMask;
        }
        /*public override bool changeImage(string uri) {
            // 
            //bool b = base.changeImage(uri);

            string strMask = "pack://application:,,,/Images/Faces/eye_mask_1_" + (m_bLeft ? "l" : "r") + ".png";
            if (m_ibIrisBackMask != null) {
                try {
                    m_ibIrisBackMask.ImageSource = new BitmapImage(new Uri(strMask));
                } catch (Exception)
                {
                    m_ibIrisBackMask.ImageSource = null;
                }                
            }
            if (m_ibIrisTransMask != null) {
                try {
                    m_ibIrisTransMask.ImageSource = new BitmapImage(new Uri(strMask));
                } catch (Exception) {
                    m_ibIrisTransMask.ImageSource = null;
                }
            }
            return false;
        }*/
    }

    public class SingleIrisInfo : ElemInfo
    {
        public Grid m_gridBack = null;
        public SingleIrisInfo(ELEMINFO_TYPE type)
            : base(type)
        {
            m_rangeMoveX = new Range(1, -15, 15);
            m_rangeMoveY = new Range(1, -10, 5);
        }
        /*public override bool changeImage(string uri)
        {
            //bool b = base.changeImage(uri);
            if (m_gridBack != null) {
                m_gridBack.Background = m_image.Source == null ? Brushes.Transparent : Brushes.White;
            }
            return false;
        }*/
    }  

    public class PairElemInfo : IElemInfo {
        public ElemInfo right = null;
        public ElemInfo left = null;
        public PairElemInfo(ELEMINFO_TYPE type) { m_eType = type; }
        public override bool changeImage(string uri, bool bOnlyImage = false) {            
            if (right == null || left == null) return false;
            bool b = left.changeImage(uri + "_l.png", bOnlyImage) && right.changeImage(uri + "_r.png", bOnlyImage);
            if (b) {
                maskColor(left.m_colorMask);
            }
            return b;           
        }
        public override bool updateView() {
            if (right == null || left == null) return false;
            return left.updateView() && right.updateView();
        }
        public override bool moveLeft() {
            if (right == null || left == null) return false;
            bool bl = left.moveLeft();
            bool br = right.moveLeft();
            return bl || br;
        }
        public override bool moveRight() {
            if (right == null || left == null) return false;
            bool bl = left.moveRight();
            bool br = right.moveRight();
            return bl || br;            
        }
        public override bool moveUp() {
            if (right == null || left == null) return false;
            bool bl = left.moveUp();
            bool br = right.moveUp();
            return bl || br;      
        }
        public override bool moveDown() {
            if (right == null || left == null) return false;
            bool bl = left.moveDown();
            bool br = right.moveDown();
            return bl || br;   
        }
        public override bool rotateLeft() {
            if (right == null || left == null) return false;
            bool bl = left.rotateLeft();
            bool br = right.rotateLeft();
            return bl || br; 
        }
        public override bool rotateRight() {
            if (right == null || left == null) return false;
            bool bl = left.rotateRight();
            bool br = right.rotateRight();
            return bl || br;
        }
        public override bool zoomIn() {
            if (right == null || left == null) return false;
            bool bl = left.zoomIn();
            bool br = right.zoomIn();
            return bl || br;
        }
        public override bool zoomOut() {
            if (right == null || left == null) return false;
            bool bl = left.zoomOut();
            bool br = right.zoomOut();
            return bl || br;
        }
        public override bool enlarge() {
            if (right == null || left == null) return false;            
            bool bl = left.moveLeft();
            bool br = right.moveRight();
            return bl || br;
        }
        public override bool narrow() {
            if (right == null || left == null) return false;
            bool bl = left.moveRight();
            bool br = right.moveLeft();
            return bl || br;
        }
        public override bool maskColor(Color color) {
            if (right == null || left == null) return false;
            bool bl = left.maskColor(color);
            bool br = right.maskColor(color);
            return bl || br;            
        }
        public override bool changeAlpha(float alpha) {
            if (right == null || left == null) return false;
            bool bl = left.changeAlpha(alpha);
            bool br = right.changeAlpha(alpha);
            return bl || br;    
        }
    }

    /*public class EarInfo : PairElemInfo {
        public EarInfo(ELEMINFO_TYPE type) : base(type){ }
        public override bool changeImage(string uri) {
            if (right == null || left == null) return false;
            bool b = left.changeImage(uri + "_l.png") && right.changeImage(uri + "_r.png");
            if (b) {
                maskColor(left.m_colorMask);                
            }
            return b;
        }
    }      */

    public class Info
    {
        public BackgroundInfo m_biBack = new BackgroundInfo(ELEMINFO_TYPES.BACK);
        public ElemInfo m_biNeck = new ElemInfo(ELEMINFO_TYPES.NECK);

        public FaceShapeInfo m_biFaceShape = new FaceShapeInfo(ELEMINFO_TYPES.FACE_SHAPE);
        public ElemInfo m_biFaceNose = new ElemInfo(ELEMINFO_TYPES.FACE_NOSE);
        public ElemInfo m_biFaceMouth = new ElemInfo(ELEMINFO_TYPES.FACE_MOUTH);
        public ElemInfo m_biFaceEarLeft = new ElemInfo(ELEMINFO_TYPES.FACE_EAR_LEFT);
        public ElemInfo m_biFaceEarRight = new ElemInfo(ELEMINFO_TYPES.FACE_EAR_RIGHT);
        public PairElemInfo m_biFaceEar = new PairElemInfo(ELEMINFO_TYPES.FACE_EAR);
        public ElemInfo m_biFaceCheer = new ElemInfo(ELEMINFO_TYPES.FACE_CHEER);

        public ElemInfo m_biEyeShapeLeft = new ElemInfo(ELEMINFO_TYPES.EYE_SHAPE_LEFT);
        public ElemInfo m_biEyeShapeRight = new ElemInfo(ELEMINFO_TYPES.EYE_SHAPE_RIGHT);
        //public SingleIrisInfo m_biEyeIrisLeft = new SingleIrisInfo(ELEMINFO_TYPES.EYE_IRIS_LEFT);
        //public SingleIrisInfo m_biEyeIrisRight = new SingleIrisInfo(ELEMINFO_TYPES.EYE_IRIS_RIGHT);
        //public ElemInfo m_biEyeMakeupLeft = new ElemInfo(ELEMINFO_TYPES.EYE_MAKEUP_LEFT);
        //public ElemInfo m_biEyeMakeupRight = new ElemInfo(ELEMINFO_TYPES.EYE_MAKEUP_RIGHT);
        public ElemInfo m_biEyeBrowLeft = new ElemInfo(ELEMINFO_TYPES.EYE_BROW_LEFT);
        public ElemInfo m_biEyeBrowRight = new ElemInfo(ELEMINFO_TYPES.EYE_BROW_RIGHT);
        public ElemInfo m_biEyeGlass = new ElemInfo(ELEMINFO_TYPES.EYE_GLASS);
        public PairElemInfo m_biEyeShape = new PairElemInfo(ELEMINFO_TYPES.EYE_SHAPE);
        //public PairElemInfo m_biEyeIris = new PairElemInfo(ELEMINFO_TYPES.EYE_IRIS);
        //public PairElemInfo m_biEyeMakeup = new PairElemInfo(ELEMINFO_TYPES.EYE_MAKEUP);
        public PairElemInfo m_biEyeBrow = new PairElemInfo(ELEMINFO_TYPES.EYE_BROW);

        public ElemInfo m_biHairHair = new ElemInfo(ELEMINFO_TYPES.HAIR_HAIR);
        public ElemInfo m_biHairAccessory = new ElemInfo(ELEMINFO_TYPES.HAIR_ACCESSORY);
        public ElemInfo m_biHairBeard = new ElemInfo(ELEMINFO_TYPES.HAIR_BEARD);
        public ElemInfo m_biHairMustache = new ElemInfo(ELEMINFO_TYPES.HAIR_MUSTACHE);

        public ElemInfo m_biCloth = new ElemInfo(ELEMINFO_TYPES.CLOTH);

        public Hashtable m_tableInfos = new Hashtable();
        public Info() {
            m_tableInfos.Add(ELEMINFO_TYPES.BACK.toString(), m_biBack);
            m_tableInfos.Add(ELEMINFO_TYPES.NECK.toString(), m_biNeck);

            m_tableInfos.Add(ELEMINFO_TYPES.FACE_SHAPE.toString(), m_biFaceShape);
            m_tableInfos.Add(ELEMINFO_TYPES.FACE_NOSE.toString(), m_biFaceNose);
            m_tableInfos.Add(ELEMINFO_TYPES.FACE_MOUTH.toString(), m_biFaceMouth);
            m_tableInfos.Add(ELEMINFO_TYPES.FACE_EAR_LEFT.toString(), m_biFaceEarLeft);
            m_tableInfos.Add(ELEMINFO_TYPES.FACE_EAR_RIGHT.toString(), m_biFaceEarRight);
            m_tableInfos.Add(ELEMINFO_TYPES.FACE_EAR.toString(), m_biFaceEar);
            m_tableInfos.Add(ELEMINFO_TYPES.FACE_CHEER.toString(), m_biFaceCheer);

            m_tableInfos.Add(ELEMINFO_TYPES.EYE_SHAPE_LEFT.toString(), m_biEyeShapeLeft);
            m_tableInfos.Add(ELEMINFO_TYPES.EYE_SHAPE_RIGHT.toString(), m_biEyeShapeRight);
            //m_tableInfos.Add(ELEMINFO_TYPES.EYE_IRIS_LEFT.toString(), m_biEyeIrisLeft);
            //m_tableInfos.Add(ELEMINFO_TYPES.EYE_IRIS_RIGHT.toString(), m_biEyeIrisRight);
            //m_tableInfos.Add(ELEMINFO_TYPES.EYE_MAKEUP_LEFT.toString(), m_biEyeMakeupLeft);
            //m_tableInfos.Add(ELEMINFO_TYPES.EYE_MAKEUP_RIGHT.toString(), m_biEyeMakeupRight);
            m_tableInfos.Add(ELEMINFO_TYPES.EYE_BROW_LEFT.toString(), m_biEyeBrowLeft);
            m_tableInfos.Add(ELEMINFO_TYPES.EYE_BROW_RIGHT.toString(), m_biEyeBrowRight);
            m_tableInfos.Add(ELEMINFO_TYPES.EYE_GLASS.toString(), m_biEyeGlass);
            m_tableInfos.Add(ELEMINFO_TYPES.EYE_SHAPE.toString(), m_biEyeShape);
            //m_tableInfos.Add(ELEMINFO_TYPES.EYE_IRIS.toString(), m_biEyeIris);
            //m_tableInfos.Add(ELEMINFO_TYPES.EYE_MAKEUP.toString(), m_biEyeMakeup);
            m_tableInfos.Add(ELEMINFO_TYPES.EYE_BROW.toString(), m_biEyeBrow);

            m_tableInfos.Add(ELEMINFO_TYPES.HAIR_HAIR.toString(), m_biHairHair);
            m_tableInfos.Add(ELEMINFO_TYPES.HAIR_ACCESSORY.toString(), m_biHairAccessory);
            m_tableInfos.Add(ELEMINFO_TYPES.HAIR_BEARD.toString(), m_biHairBeard);
            m_tableInfos.Add(ELEMINFO_TYPES.HAIR_MUSTACHE.toString(), m_biHairMustache);
            
            m_tableInfos.Add(ELEMINFO_TYPES.CLOTH.toString(), m_biCloth);
        }
    }
}
