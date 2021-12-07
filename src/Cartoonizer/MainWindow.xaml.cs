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
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Layout.Core;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DevExpress.Xpf.LayoutControl;
using Microsoft.Win32;
using System.Windows.Forms;

namespace Cartoonizer
{
    public partial class MainWindow : Window {

        public bool isFemale = true;
        public bool bAskConfirm = true;

        
        public const string UriPrefix = "/Cartoonizer;component";
        public string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;

        public List<HistoryInfo> history = new List<HistoryInfo>();
        public int m_nHisPos = -1;

        public Info data = new Info();
        public ResourceData defaultResourceData = new ResourceData();
        public ResourceData curRes = new ResourceData();

        public ObservableCollection<ImageData> images = new ObservableCollection<ImageData>();
        public ObservableCollection<ColorData> colors = new ObservableCollection<ColorData>();
        public ObservableCollection<ImageData> Images { get { return images; } set { images = value; } }
        public ObservableCollection<ColorData> ColorItems { get { return colors; } set { colors = value; } } 
        public GenderTypeData gender = null;

        public MainWindow(bool isFemale) {
            this.isFemale = isFemale;
            InitializeComponent();
            Loaded += init;
        }
        public void init(object sender, RoutedEventArgs e) {
            gender = new GenderTypeData(!this.isFemale);

            MainTypeData face = new MainTypeData((int)MainMode.FACE, btnFace, subFace);
            MainTypeData eye = new MainTypeData((int)MainMode.EYES, btnEye, subEye);
            MainTypeData hair = new MainTypeData((int)MainMode.HAIR, btnHair, subHair);
            ClothTypeData cloth = new ClothTypeData((int)MainMode.CLOTHES, btnCloth);
            BackgroundTypeData back = new BackgroundTypeData((int)MainMode.BACK, btnBack);

            face.addSubTypeData(new FaceShape(btnFace_Shape));
            face.addSubTypeData(new FaceMouth(btnFace_Mouth));
            face.addSubTypeData(new FaceNose(btnFace_Nose));
            face.addSubTypeData(new FaceEar(btnFace_Ear));
            face.addSubTypeData(new FaceCheer(btnFace_Cheer));

            eye.addSubTypeData(new EyeShape(btnEye_Shape));
            eye.addSubTypeData(new EyeBrow(btnEye_Brow));            
            eye.addSubTypeData(new EyeGlass(btnEye_Glass));

            hair.addSubTypeData(new HairHair(btnHair_Hair));
            if (isFemale) {
                btnHair_Mustache.SetVisible(false);
                btnHair_Beard.SetVisible(false);
                btnHair_Hair.Width = btnHair_Accessory.Width = 250;
                hair.addSubTypeData(new HairAccessory(btnHair_Accessory));
            } else {
                btnHair_Accessory.SetVisible(false);
                btnHair_Hair.Width = btnHair_Mustache.Width = btnHair_Beard.Width = 166;
                hair.addSubTypeData(new HairMustache(btnHair_Mustache));
                hair.addSubTypeData(new HairBeard(btnHair_Beard));
            }            

            gender.addSubTypeData(face);
            gender.addSubTypeData(eye);
            gender.addSubTypeData(hair);
            gender.addSubTypeData(cloth);
            gender.addSubTypeData(back);
                        
            initBodies();
            
            gender.setBody(data);
            gender.init();
            updateControlPanel();

            initHistory();
        }
        private void subUpdateControlPanel() {            
            bool bShowControlPanel = false;
            bool bShowColorPanel = false;

            SubTypeData sd = gender.getSubTypeData();

            if (sd != null) {
                bShowControlPanel = sd.isValidControlPanel();
                bShowColorPanel = bShowControlPanel & sd.resource.bShowColor;
            }
            colorPanel.SetVisible(bShowColorPanel);
            panelControls.SetVisible(bShowControlPanel);
        }

        private void updateControlPanel() {
            SubTypeData sd = gender.getSubTypeData();
            if (sd == null) { 
                curRes = defaultResourceData;            
            } else { curRes = sd.resource; }
            
            images.Clear();
            foreach (ImageData id in curRes.images)
                images.Add(id);

            colorPanel.SetVisible(curRes.bShowColor);
            colors.Clear();
            foreach (ColorData id in curRes.colors)
                colors.Add(id);            

            bool bShowPanelControl = (curRes.bShowAlpha || curRes.bShowEnlarge || curRes.bShowMoveH || curRes.bShowMoveV || curRes.bShowZoom || curRes.bShowRotate);
            panelControls.SetVisible(bShowPanelControl);
            slideAlpha.SetVisible(curRes.bShowAlpha);
            btnSubMoveUp.SetVisible(curRes.bShowMoveV); btnSubMoveDown.SetVisible(curRes.bShowMoveV);
            btnSubMoveLeft.SetVisible(curRes.bShowMoveH); btnSubMoveRight.SetVisible(curRes.bShowMoveH);
            btnSubZoomIn.SetVisible(curRes.bShowZoom); btnSubZoomOut.SetVisible(curRes.bShowZoom);
            btnSubEnlarge.SetVisible(curRes.bShowEnlarge); btnSubNarrow.SetVisible(curRes.bShowEnlarge);

        }
        private void onChangeMainType(object sender, RoutedEventArgs e) {
            if (gender == null) return;
            bool bProcessed = gender.onChangeMainType((System.Windows.Controls.RadioButton)sender);            
            updateControlPanel();
            subUpdateControlPanel();
        }
        private void onChangeSubType(object sender, RoutedEventArgs e) {
            if (gender == null) return;
            bool bProcessed = gender.onChangeSubType((System.Windows.Controls.RadioButton)sender);
            updateControlPanel();
            subUpdateControlPanel();
        }
        private void onChangeImage(object sender, RoutedEventArgs e) {
            if (gender == null) return;
            var imageInfo = ((Border)sender).DataContext as ImageData;
            if (imageInfo == null || imageInfo.Disabled) return;

            bool bAffected = gender.onChangeImage(imageInfo);
            subUpdateControlPanel();
            if (bAffected) addHistoryItem();
        }
        private void onChangeColor(object sender, RoutedEventArgs e) {
            if (gender == null) return;
            var colorInfo = ((Rectangle)sender).DataContext as ColorData;
            if (colorInfo == null || colorInfo.Disabled) return;

            bool bAffected = gender.onChangeColor(colorInfo);
            if (bAffected) addHistoryItem();
        }

        public void onMoreColor(object s, EventArgs e) {
            ColorPickerDialog cPicker = new ColorPickerDialog();
            cPicker.Owner = this;
            bool? dialogResult = cPicker.ShowDialog();
            if (dialogResult != null && (bool)dialogResult == true) {                
                gender.onMoreColor(cPicker.SelectedColor);
                addHistoryItem();
            }
        }

        private void onControl(object s, EventArgs e) {
            if (gender == null) return;
            bool bAffected = false;
            if ((System.Windows.Controls.Button)s == btnSubZoomIn) {
                bAffected = gender.onControl(CTRL_TYPE.ZOOM_IN);
            } else if ((System.Windows.Controls.Button)s == btnSubZoomOut) {
                bAffected = gender.onControl(CTRL_TYPE.ZOOM_OUT);
            } else if ((System.Windows.Controls.Button)s == btnSubMoveUp) {
                bAffected = gender.onControl(CTRL_TYPE.UP);
            } else if ((System.Windows.Controls.Button)s == btnSubMoveDown) {
                bAffected = gender.onControl(CTRL_TYPE.DOWN);
            } else if ((System.Windows.Controls.Button)s == btnSubMoveLeft) {
                bAffected = gender.onControl(CTRL_TYPE.LEFT);
            } else if ((System.Windows.Controls.Button)s == btnSubMoveRight) {
                bAffected = gender.onControl(CTRL_TYPE.RIGHT);
            } else if ((System.Windows.Controls.Button)s == btnSubEnlarge) {
                bAffected = gender.onControl(CTRL_TYPE.ENLARGE);
            } else if ((System.Windows.Controls.Button)s == btnSubNarrow) {
                bAffected = gender.onControl(CTRL_TYPE.NARROW);
            }
            if (bAffected) addHistoryItem();
        }

        public void onChangeAlphaValue(object s, EventArgs e) {
            float alpha = (float)slideAlpha.Value;
            if (gender == null) return;
            bool bAffected = gender.onChangeAlpha(alpha);
            //if (bAffected) addHistoryItem();
        }

        public void initBodies() {
            data.m_biBack.setTransforms(trans, scale, rotate);            

            data.m_biFaceShape.setTransforms(transFace, scaleFace, rotateFace);
            data.m_biFaceCheer.setTransforms(transCheer, scaleCheer, rotateCheer);
            data.m_biFaceNose.setTransforms(transNose, scaleNose, rotateNose);
            data.m_biFaceMouth.setTransforms(transMouth, scaleMouth, rotateMouth);
            data.m_biNeck.setTransforms(transNeck, scaleNeck, rotateNeck);
            data.m_biHairAccessory.setTransforms(transHairAccessory, scaleHairAccessory, rotateHairAccessory);
            data.m_biFaceEarLeft.setTransforms(transEarLeft, scaleEarLeft, rotateEarLeft);
            data.m_biFaceEarRight.setTransforms(transEarRight, scaleEarRight, rotateEarRight);
            data.m_biEyeBrowLeft.setTransforms(transBrowLeft, scaleBrowLeft, rotateBrowLeft);
            data.m_biEyeBrowRight.setTransforms(transBrowRight, scaleBrowRight, rotateBrowRight);
            data.m_biEyeShapeLeft.setTransforms(transEyeLeft, scaleEyeLeft, rotateEyeLeft);
            data.m_biEyeShapeRight.setTransforms(transEyeRight, scaleEyeRight, rotateEyeRight);
            data.m_biEyeGlass.setTransforms(transGlass, scaleGlass, rotateGlass);
            data.m_biCloth.setTransforms(transCloth, scaleCloth, rotateCloth);            
            data.m_biHairHair.setTransforms(transHair, scaleHair, rotateHair);
            data.m_biHairMustache.setTransforms(transMustache, scaleMustache, rotateMustache);
            data.m_biHairBeard.setTransforms(transBeard, scaleBeard, rotateBeard);

            data.m_biFaceMouth.setDefaultCenterPt(new Point(512, 799));
            data.m_biFaceNose.setDefaultCenterPt(new Point(512, 659));
            data.m_biEyeShapeLeft.setDefaultCenterPt(new Point(395, 612));
            data.m_biEyeShapeRight.setDefaultCenterPt(new Point(626, 612));
            data.m_biEyeBrowLeft.setDefaultCenterPt(new Point(395, 542));
            data.m_biEyeBrowRight.setDefaultCenterPt(new Point(626, 612));
            data.m_biHairAccessory.setDefaultCenterPt(new Point(506, 347));
            data.m_biFaceShape.setDefaultCenterPt(new Point(512, 897));

            data.m_biBack.setImage(imgBack);
            data.m_biFaceShape.setImage(imgFaceShape);
            data.m_biNeck.setImage(imgNeck);
            data.m_biFaceMouth.setImage(imgMouth);
            data.m_biFaceNose.setImage(imgNose);
            data.m_biFaceCheer.setImage(imgCheer);
            data.m_biFaceEarLeft.setImage(imgEarLeft);
            data.m_biFaceEarRight.setImage(imgEarRight);
            data.m_biEyeGlass.setImage(imgGlass);
            data.m_biEyeShapeLeft.setImage(imgEyeLeft);
            data.m_biEyeShapeRight.setImage(imgEyeRight);
            data.m_biEyeBrowLeft.setImage(imgBrowLeft);
            data.m_biEyeBrowRight.setImage(imgBrowRight);
            data.m_biHairHair.setImage(imgHair);
            data.m_biHairAccessory.setImage(imgHairAccessory);
            data.m_biHairMustache.setImage(imgMustache);
            data.m_biHairBeard.setImage(imgBeard);
            data.m_biCloth.setImage(imgCloth);            

            data.m_biFaceShape.setMask(rectFaceShap, brushFaceShape, imgFaceShapeMask);
            data.m_biNeck.setMask(rectNeck, brushNeck, imgNeckMask);
            data.m_biFaceMouth.setMask(rectMouth, brushMouth, imgMouthMask);
            data.m_biFaceNose.setMask(rectNose, brushNose, imgNoseMask);
            data.m_biFaceCheer.setMask(rectCheer, brushCheer, imgCheerMask);
            data.m_biFaceEarLeft.setMask(rectEarLeft, brushEarLeft, imgEarLeftMask);
            data.m_biFaceEarRight.setMask(rectEarRight, brushEarRight, imgEarRightMask);
            data.m_biEyeShapeLeft.setMask(rectEyeLeft, brushEyeLeft, imgEyeLeftMask);
            data.m_biEyeShapeRight.setMask(rectEyeRight, brushEyeRight, imgEyeRightMask);
            
            data.m_biEyeBrowLeft.setMask(rectBrowLeft, brushBrowLeft, imgBrowLeftMask);
            data.m_biEyeBrowRight.setMask(rectBrowRight, brushBrowRight, imgBrowRightMask);
            data.m_biHairHair.setMask(rectHair, brushHair, imgHairMask);
            data.m_biHairAccessory.setMask(rectHairAccessory, brushHairAccessory, imgHairAccessoryMask);
            data.m_biHairMustache.setMask(rectMustache, brushMustache, imgMustacheMask);
            data.m_biHairBeard.setMask(rectBeard, brushBeard, imgBeardMask);
            data.m_biCloth.setMask(rectCloth, brushCloth, imgClothMask);
            data.m_biEyeGlass.setMask(rectGlass, brushGlass, imgGlassMask);

            data.m_biFaceEar.left = data.m_biFaceEarLeft;
            data.m_biFaceEar.right = data.m_biFaceEarRight;
            data.m_biEyeBrow.left = data.m_biEyeBrowLeft;
            data.m_biEyeBrow.right = data.m_biEyeBrowRight;
            data.m_biEyeShape.left = data.m_biEyeShapeLeft;
            data.m_biEyeShape.right = data.m_biEyeShapeRight;

            btnZoomIn.Click += data.m_biBack.zoomIn;
            btnZoomOut.Click += data.m_biBack.zoomOut;
            btnRotateLeft.Click += onFaceRotateLeft;
            btnRotateRight.Click += onFaceRotateRight;
            btnMoveDown.Click += data.m_biBack.moveDown;
            btnMoveUp.Click += data.m_biBack.moveUp;
            btnMoveLeft.Click += data.m_biBack.moveLeft;
            btnMoveRight.Click += data.m_biBack.moveRight;

            btnSubZoomIn.Click += onControl;
            btnSubZoomOut.Click += onControl;
            btnSubMoveUp.Click += onControl;
            btnSubMoveDown.Click += onControl;
            btnSubMoveLeft.Click += onControl;
            btnSubMoveRight.Click += onControl;
            btnSubEnlarge.Click += onControl;
            btnSubNarrow.Click += onControl;

            btnExport.Click += onDerived;
            btnRedesign.Click += onRedesign;
        }

        void initHistory() {
            history.Clear();
            m_nHisPos = 0;
            addHistoryItem();

            btnPrev.IsEnabled = false;
            btnNext.IsEnabled = false;
        }
        private void onBtnRandom(object sender, RoutedEventArgs e) {
            gender.random();
            addHistoryItem();            
        }
        private void onBtnReset(object sender, RoutedEventArgs e) {
            gender.reset(true);         
            initHistory();
            updateControlPanel();
            subUpdateControlPanel();
        }
        private void onBtnPrev(object sender, RoutedEventArgs e) {            
            m_nHisPos--;
            if (m_nHisPos < history.Count - 1)
                btnNext.IsEnabled = true;
            if (m_nHisPos <= 0) {
                btnPrev.IsEnabled = false;                                
            }
            applyHistory();            
        }
        private void onBtnNext(object sender, RoutedEventArgs e) {
            m_nHisPos++;
            if (m_nHisPos < history.Count - 1)
                btnNext.IsEnabled = true;
            else
                btnNext.IsEnabled = false;

            if (m_nHisPos > 0)
            {
                btnPrev.IsEnabled = true;
            }
            applyHistory();
        }
        private void saveButton_Click(object sender, RoutedEventArgs e) {
            
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();            

            dlg.Filter = "PNG File (*.PNG)|*.PNG|Bitmap File (*.BMP)|*.BMP|Jpeg File (*.JPG)|*.JPG";
            if (dlg.ShowDialog() == true)
            { 
                try
                {
                    using (Stream stream = dlg.OpenFile())
                    {
                        BitmapEncoder encoder = null;
                        RenderTargetBitmap bmp = new RenderTargetBitmap(1024, 1024, 1/1024, 1/1024, PixelFormats.Pbgra32);
                        bmp.Render(gridMain);

                        string strFileName = dlg.FileName;
                        if (strFileName.EndsWith(".jpg", true, null))
                        {
                            encoder = new JpegBitmapEncoder();
                        }
                        else if (strFileName.EndsWith(".png", true, null))
                        {
                            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                            pngEncoder.Interlace = PngInterlaceOption.On;
                            encoder = pngEncoder;
                        }
                        else if (strFileName.EndsWith(".bmp", true, null))
                        {
                            encoder = new BmpBitmapEncoder();
                        }
                        encoder.Frames.Add(BitmapFrame.Create(bmp));
                        encoder.Save(stream);
                        stream.Close();
                        System.Windows.MessageBox.Show("Success to save!");
                    }
                }
                catch (Exception) {
                    System.Windows.MessageBox.Show("Failed to save!");
                }
                
            }
        }

        private void onFaceRotateLeft(object s, EventArgs e) {            
            bool bAffected = data.m_biFaceShape.rotateLeft();
            if (bAffected) addHistoryItem();
        }
        private void onFaceRotateRight(object s, EventArgs e) {
            bool bAffected = data.m_biFaceShape.rotateRight();
            if (bAffected) addHistoryItem();
        }       

        protected override void OnClosing(CancelEventArgs e) {
            #if DEBUG
            #else
            if (bAskConfirm) {
                MessageBoxResult ret = System.Windows.MessageBox.Show(this, "Are you sure?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (ret == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }
            }            
            #endif
            splash spl = new splash();
            spl.Show();
        }        

        private HistoryElemInfo getHisInfoOfElemInfo(ElemInfo ei) {
            HistoryElemInfo hi = new HistoryElemInfo();
            hi.tX = ei.m_translate.X;
            hi.tY = ei.m_translate.Y;
            
            hi.s_centerX = ei.m_scale.CenterX;
            hi.s_centerY = ei.m_scale.CenterY;
            hi.sX = ei.m_scale.ScaleX;
            hi.sY = ei.m_scale.ScaleY;

            hi.angle = ei.m_rotate.Angle;
            hi.r_centerX = ei.m_rotate.CenterX;
            hi.r_centerY = ei.m_rotate.CenterY;

            hi.centerX = ei.m_ptCenter.X;
            hi.centerY = ei.m_ptCenter.Y;
            hi.mX = ei.m_nMoveX;
            hi.mY = ei.m_nMoveY;
            hi.rotate = ei.m_fRotate;
            hi.scale = ei.m_fScale;
            hi.alpha = ei.m_fMaskOpacity;
            hi.color = ei.m_colorMask;
            hi.idx = ei.m_nIdx;
            return hi;
        }
        private HistoryPairElemInfo getHisInfoOfElemInfo(PairElemInfo ei) {
            HistoryPairElemInfo hisInfo = new HistoryPairElemInfo();
            hisInfo.left = getHisInfoOfElemInfo(ei.left);
            hisInfo.right = getHisInfoOfElemInfo(ei.right);
            hisInfo.idx = ei.m_nIdx;
            return hisInfo;
        }
        private void setHisInfoToElemInfo(ElemInfo ei, HistoryElemInfo hi) {
            ei.m_nIdx = hi.idx;
            ei.m_translate.X = hi.tX;
            ei.m_translate.Y =hi.tY;

            ei.m_scale.CenterX = hi.s_centerX;
            ei.m_scale.CenterY = hi.s_centerY;
            ei.m_scale.ScaleX = hi.sX;
            ei.m_scale.ScaleY = hi.sY;

            ei.m_rotate.Angle =hi.angle;
            ei.m_rotate.CenterX =hi.r_centerX; 
            ei.m_rotate.CenterY = hi.r_centerY;

            ei.m_ptCenter.X = hi.centerX;
            ei.m_ptCenter.Y =hi.centerY;
            ei.m_nMoveX = hi.mX;
            ei.m_nMoveY = hi.mY;
            ei.m_fRotate =hi.rotate;
            ei.m_fScale = hi.scale;
            ei.m_fMaskOpacity = hi.alpha;
            ei.m_colorMask = hi.color;            
        }

        private void setHisInfoToElemInfo(PairElemInfo ei, HistoryPairElemInfo hi) {
            ei.m_nIdx = hi.idx;
            setHisInfoToElemInfo(ei.left, hi.left);
            setHisInfoToElemInfo(ei.right, hi.right);                      
        }

        void addHistoryItem() {
            if (m_nHisPos >= 0 && m_nHisPos < history.Count - 1)
            {
                history.RemoveRange(m_nHisPos +1, history.Count - m_nHisPos-1);
            }
            HistoryInfo hi = new HistoryInfo();
            hi.back = getHisInfoOfElemInfo(data.m_biBack);
            hi.neck = getHisInfoOfElemInfo(data.m_biNeck);
            hi.faceShape = getHisInfoOfElemInfo(data.m_biFaceShape);
            hi.faceNose = getHisInfoOfElemInfo(data.m_biFaceNose);
            hi.faceMouth = getHisInfoOfElemInfo(data.m_biFaceMouth);
            hi.faceEar = getHisInfoOfElemInfo(data.m_biFaceEar);

            hi.eyeShape = getHisInfoOfElemInfo(data.m_biEyeShape);
            hi.eyeBrow = getHisInfoOfElemInfo(data.m_biEyeBrow);
            hi.eyeGlass = getHisInfoOfElemInfo(data.m_biEyeGlass);

            hi.hairHair = getHisInfoOfElemInfo(data.m_biHairHair);
            hi.hairAccessory = getHisInfoOfElemInfo(data.m_biHairAccessory);
            hi.hairBeard = getHisInfoOfElemInfo(data.m_biHairBeard);
            hi.hairMustache = getHisInfoOfElemInfo(data.m_biHairMustache);

            hi.cloth = getHisInfoOfElemInfo(data.m_biCloth);

            history.Add(hi);
            m_nHisPos = history.Count-1;
            if (history.Count > 0 && m_nHisPos >= 0)
            {
                btnPrev.IsEnabled = true;
            }
            btnNext.IsEnabled = false;
        }

        void applyHistory() {
            if (m_nHisPos <0 || m_nHisPos >= history.Count)
                return;
            HistoryInfo hi = history.ElementAt(m_nHisPos);

            setHisInfoToElemInfo(data.m_biBack, hi.back); 
            setHisInfoToElemInfo(data.m_biNeck, hi.neck); 
            setHisInfoToElemInfo(data.m_biFaceShape, hi.faceShape); 
            setHisInfoToElemInfo(data.m_biFaceNose, hi.faceNose); 
            setHisInfoToElemInfo(data.m_biFaceMouth, hi.faceMouth); 
            setHisInfoToElemInfo(data.m_biFaceEar, hi.faceEar); 

            setHisInfoToElemInfo(data.m_biEyeShape, hi.eyeShape); 
            setHisInfoToElemInfo(data.m_biEyeBrow, hi.eyeBrow); 
            setHisInfoToElemInfo(data.m_biEyeGlass, hi.eyeGlass); 

            setHisInfoToElemInfo(data.m_biHairHair, hi.hairHair); 
            setHisInfoToElemInfo(data.m_biHairAccessory, hi.hairAccessory); 
            setHisInfoToElemInfo(data.m_biHairBeard, hi.hairBeard); 
            setHisInfoToElemInfo(data.m_biHairMustache, hi.hairMustache); 

            setHisInfoToElemInfo(data.m_biCloth, hi.cloth);

            gender.updateView();            
        }
        private void onRedesign(object s, EventArgs e) {
            bAskConfirm = false;
            Close();
        }
        private void onDerived(object s, EventArgs e) {
            string zipFilePath = string.Empty;
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();            
            dialog.Title = "Select a Directory";
            dialog.Filter = "Directory|*.this.directory";
            dialog.FileName = "select";
            if (dialog.ShowDialog() == true) {
                string path = dialog.FileName;
                path = path.Replace("\\select.this.directory", "");
                path = path.Replace(".this.directory", "");
                if (!System.IO.Directory.Exists(path)) {
                    System.IO.Directory.CreateDirectory(path);
                }
                
                hideCanvasElem();  gridNeck.SetVisible(true); 
                savePartPng("neck", path);

                hideCanvasElem(); gridFaceShape.SetVisible(true); rectFaceShap.SetVisible(true); imgFaceShape.SetVisible(true);        
                savePartPng("face_shape", path);

                hideCanvasElem(); gridNose.SetVisible(true); gridFaceShape.SetVisible(true); 
                savePartPng("face_nose", path);

                hideCanvasElem(); gridMouth.SetVisible(true); gridFaceShape.SetVisible(true); 
                savePartPng("face_mouth", path);

                hideCanvasElem(); gridCheer.SetVisible(true); gridFaceShape.SetVisible(true); 
                savePartPng("face_blusher", path);

                hideCanvasElem(); gridEarLeft.SetVisible(true); gridEarRight.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("face_ear", path);
                hideCanvasElem(); gridEarLeft.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("face_ear_l", path);
                hideCanvasElem(); gridEarRight.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("face_ear_r", path);

                hideCanvasElem(); gridEyeShapeLeft.SetVisible(true); gridEyeShapeRight.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("eye_shape", path);
                hideCanvasElem(); gridEyeShapeLeft.SetVisible(true);  gridFaceShape.SetVisible(true);
                savePartPng("eye_shape_l", path);
                hideCanvasElem(); gridEyeShapeRight.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("eye_shape_r", path);

                hideCanvasElem(); gridBrowLeft.SetVisible(true); gridBrowRight.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("eye_brow", path);
                hideCanvasElem(); gridBrowLeft.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("eye_brow_l", path);
                hideCanvasElem(); gridBrowRight.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("eye_brow_r", path);

                hideCanvasElem(); gridGlass.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("eye_glass", path);
                hideCanvasElem(); gridHair.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("hair_onhead", path);
                hideCanvasElem(); gridHairAccessory.SetVisible(true); gridFaceShape.SetVisible(true);
                savePartPng("hair_accessory", path);
                if (!isFemale)
                {
                    hideCanvasElem(); gridMustache.SetVisible(true); gridFaceShape.SetVisible(true);
                    savePartPng("hair_mustache", path);
                    hideCanvasElem(); gridBeard.SetVisible(true); gridFaceShape.SetVisible(true);
                    savePartPng("hair_beard", path);
                }

                hideCanvasElem(); gridCloth.SetVisible(true);
                savePartPng("cloth", path);
                hideCanvasElem(); gridBack.SetVisible(true); 
                savePartPng("back", path);

                hideCanvasElem(true);
                System.Windows.MessageBox.Show("Success to Derived!");
            }
        }
        
        void hideCanvasElem(bool bShow = false) {
            gridNeck.SetVisible(bShow);
            gridFaceShape.SetVisible(bShow); rectFaceShap.SetVisible(bShow); imgFaceShape.SetVisible(bShow);
            gridNose.SetVisible(bShow);
            gridMouth.SetVisible(bShow);
            gridCheer.SetVisible(bShow);
            gridEarLeft.SetVisible(bShow);
            gridEarRight.SetVisible(bShow);
          
            gridHairAccessory.SetVisible(bShow);
            gridHair.SetVisible(bShow);
            gridMustache.SetVisible(bShow);
            gridBeard.SetVisible(bShow);
            
            gridBrowLeft.SetVisible(bShow); gridBrowRight.SetVisible(bShow);            
            gridEyeShapeLeft.SetVisible(bShow); gridEyeShapeRight.SetVisible(bShow);
            gridGlass.SetVisible(bShow);

            gridCloth.SetVisible(bShow);
            
            gridBack.SetVisible(bShow);
        }
        
        void savePartPng(string strElem, string strPath) {
            string strFileName = (isFemale ? "female_" : "male_") + strElem + ".png";
            Stream stream = File.Create(System.IO.Path.Combine(strPath, strFileName));

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Interlace = PngInterlaceOption.On;
            RenderTargetBitmap bmp = new RenderTargetBitmap(1024, 1024, 1 / 1024, 1 / 1024, PixelFormats.Pbgra32);
            bmp.Render(gridMain);                     
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(stream);
            stream.Close();             
        }

        private bool createDirectory(string dirName) {
            try {
                DirectoryInfo di = Directory.CreateDirectory(dirName);
                if (di == null)  return false;
            }  catch (Exception) {
                return false;
            }
            return true;
        }
        private string createUUID () {
            string def = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random rnd = new Random();
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < 12; i++)
                ret.Append(def.Substring(rnd.Next(def.Length), 1));
            return ret.ToString();
        }
        private void saveBitmapToTempFile(string filePath, BitmapImage bitmap) {
            var fileStream = new FileStream(filePath, FileMode.Create);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(fileStream);
        }
    }

    public class ColorDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var control = (FlowLayoutControl)container;
            ColorData c = (ColorData)item;
            return (DataTemplate)control.Resources[c.type + "DataTemplate"];

        }
    }
    

}
