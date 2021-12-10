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
using System.ComponentModel;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Core;

namespace Cartoonizer
{

    public class SelectionItemData : INotifyPropertyChanged
    {
        private bool _selected = false;
        private bool _disabled = true;


        public bool Disabled { get { return _disabled; } set { _disabled = value; } }
        public bool Selected { get { return _selected; } set { _selected = value; OnPropertyChanged("Selected"); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class ColorData : SelectionItemData
    {
        public Color color { get; set; }
        public string type { get; set; }
        public ColorData(Color color)
        {
            this.color = color;
            this.type = "Color";
            this.Disabled = false;
        }
        public ColorData() { this.type = "More"; this.Disabled = false; }
    }

    public class ImageData : SelectionItemData
    {
        public string pathImg { get; set; }
        public BitmapImage img { get; set; }
        public string pathThumb { get; set; }
        public BitmapImage thumbnail { get; set; }
        public bool isEmpty { get; set; }

        public ImageData(string imgPath, string thumbPath)
        {
            isEmpty = false;
            try
            {
                this.pathImg = imgPath;
                this.pathThumb = thumbPath;
                this.thumbnail = new BitmapImage(new Uri(thumbPath));
                this.Disabled = false;
            }
            catch
            {
                this.Disabled = true;
            }
        }
        public ImageData() {
            this.isEmpty = true;
            try
            {
                this.pathImg = "";
                this.pathThumb = "pack://application:,,,/Images/Icons/close-32x32.png";
                this.thumbnail = new BitmapImage(new Uri(pathThumb));
                this.Disabled = false;
            }
            catch
            {
                this.Disabled = true;
            }
            
        }
    }

    public struct stResourceFileEntry {
        public string path;
        public int idx;
        public string type;
        public stResourceFileEntry(string path, int idx, string type)
        {
            this.path = path;
            this.idx = idx;
            this.type = type;
        }
    }

    public class ResourceData : SelectionItemData
    {
        public ObservableCollection<ImageData> images = new ObservableCollection<ImageData>();
        public ObservableCollection<ColorData> colors = new ObservableCollection<ColorData>();

        public bool bShowColor = false;
        public bool bShowEnlarge = false;
        public bool bShowMoveV = false;
        public bool bShowMoveH = false;
        public bool bShowZoom = false;
        public bool bShowRotate = false;
        public bool bShowAlpha = false;
    }

    public class TypeData
    {
        public RadioButton button;
        public int type;
        public bool bMale = false;
        public int idxDefault = 0;
        public int currentIdx = -1;

        protected Info body;

        public void setButton(RadioButton button) { this.button = button; }
        public TypeData() { this.button = null; }
        public TypeData(int type, RadioButton btn) { this.type = type; this.button = btn; }
        public TypeData(RadioButton button) { this.setButton(button); }
        public virtual void prepare() { }        
        public virtual void init() { }
        public virtual void setBody(Info body) { this.body = body; }
        public virtual void reset(bool bForce = false) { }
        public virtual void updateView() { onUpdateView();  }
        public virtual void onUpdateView() { }
        public virtual void random() { }
        public virtual bool onItemChanged(object oldValue, object newValue) { return false; }

        public int getRandom(int upper, int lower = 0) {
            Random randObj = new Random();
            return randObj.Next(lower, upper);
        }
    }

    public class SubTypeData : TypeData
    {
        public new ELEMINFO_TYPE type;        
        public ResourceData resource = new ResourceData();
        public IElemInfo bodyElem = null;
        public bool bHasCustomColor = true;
        public int nDefualtImgIdx = 0;
        public bool bHasEmptyImage = true;
        public bool bHasPairElem = false;

        public int currentImageIdx = -1;
        public int currentColorIdx = -1;

        public SubTypeData() { }
        public SubTypeData(ELEMINFO_TYPE type, RadioButton btn) { this.type = type; this.button = btn; }
        public SubTypeData(RadioButton btn) { this.button = btn; }

        public override void init()
        {
            base.init();
            initResources();        

            if (body != null) {
                bodyElem = (IElemInfo) body.m_tableInfos[type.toString()];
            }
            loadExternalResources();
        }
        public virtual void initResources() {
            resource.colors.Clear();
            resource.images.Clear();
            initImages();
            initColors();
            initControlPanel();
        }
        public virtual void initImages() {
            if (bHasEmptyImage) {
                addImage(new ImageData());
            }
        }
        public virtual void addImage(ImageData imageData) {
            resource.images.Add(imageData);
        }

        public virtual void initControlPanel() {

        }

        /// Colors
        public virtual void initColors()
        { //Important !!!! last of inherited method!!!
            if (bHasCustomColor) {
                resource.colors.Add(new ColorData());
            }
        }
        public virtual void addColor(Color color) {
            resource.colors.Add(new ColorData(color));
        }

        public virtual void loadExternalResources() {
            // directory
            loadExternalResourcesWithType("common");
            loadExternalResourcesWithType(bMale ? "male": "female");
        }
        public virtual void loadExternalResourcesWithType(string strType) {
            string path = getDirPath(strType);
            if (string.IsNullOrEmpty(path)) return;

            //string[] dirEntries = Directory.GetDirectories(path);
            //foreach (string dirPath in dirEntries) {
                loadExternalResourcesInDirectory(path);
            //}            
        }

        public virtual void loadExternalResourcesInDirectory(string path) {
            string[] fileEntries = Directory.GetFiles(path);
            Hashtable resGroup = new Hashtable();
            foreach (string filePath in fileEntries) {
                string fileExtension = Path.GetExtension(filePath);
                
                // only accept png files as assets
                if (String.Compare(fileExtension, ".png", true) != 0)  continue;

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] words = fileName.Split(CONST.DELIMITERS);
                if (words.Length <= 0) continue;
                string strIdx = words[0];
                int idx = 0;
                try{
                    // prefix must be numeric!
                    idx = Int32.Parse(strIdx);
                } catch (Exception) { continue; }
                string strType = "";
                if (words.Length > 1) strType = words[1];
                else strType = "main";
                stResourceFileEntry resFileEntry = new stResourceFileEntry(filePath, idx, strType);

                Hashtable oneResEntry = null;
                if (!resGroup.ContainsKey(idx)) {
                    oneResEntry = new Hashtable();
                    resGroup.Add(idx, oneResEntry);
                } else {
                    oneResEntry = resGroup[idx] as Hashtable;
                }
                if (oneResEntry == null) continue;
                oneResEntry.Add(strType, filePath);                
            }

            foreach (DictionaryEntry de in resGroup) {
                Hashtable oneResEntry = (Hashtable)de.Value;
                ImageData id = getImageDataFromResourceEntry(oneResEntry);
                if (id == null) continue;
                addImage(id);
            }
        }

        public virtual ImageData getImageDataFromResourceEntry(Hashtable oneResEntry) {
            if (oneResEntry == null || oneResEntry.Count <= 0) return null;

            string strMain = ""; // main Stuff or left stuff
            string strIntro = ""; // intro 
            if (oneResEntry.ContainsKey("intro")) strIntro = (string)oneResEntry["intro"];
            if (bHasPairElem) {
                if (!oneResEntry.ContainsKey("l") || !oneResEntry.ContainsKey("r")) return null;
                string strL = (string)oneResEntry["l"];
                strMain = Path.Combine(Path.GetDirectoryName(strL), Path.GetFileNameWithoutExtension(strL));
                strMain = strMain.Substring(0, strMain.Length - 2);
                if (string.IsNullOrEmpty(strIntro))
                    strIntro = (string)oneResEntry["l"];
            } else {
                if (!oneResEntry.ContainsKey("main")) return null;
                strMain = (string) oneResEntry["main"];
                if (string.IsNullOrEmpty(strIntro))
                    strIntro = strMain;
            }
            if (string.IsNullOrEmpty(strMain)) return null;
            return new ImageData(strMain, strIntro);
        }
        
        private string getDirPath(string strType)
        {
            string baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "assets", strType);
            path = Path.Combine(path, this.type.main);
            if (!string.IsNullOrEmpty(this.type.sub)) {
                path = Path.Combine(path, this.type.sub);                
            }
            
            if (!Directory.Exists(path)) {
                return "";
            }            
            return path;
        }
        public override void reset(bool bForceClear = false) {            
            setCurrentImageIdx(bForceClear ? 0 : nDefualtImgIdx);
        }
        public override void random() {
            setCurrentImageIdx(getRandom(resource.images.Count,nDefualtImgIdx));
        }
        public ImageData getImageDataOfCurrent() { return getImageDataByIdx(currentImageIdx); }
        public ImageData getImageDataByIdx(int idx)
        {
            if (idx < 0 || idx >= resource.images.Count) return null;
            return resource.images[idx];
        }
        public bool isValidControlPanel() {
            ImageData imageData = getImageDataOfCurrent();
            if (imageData.Disabled || imageData.isEmpty) return false;
            return true;
        }
        public ColorData getColorDataOfCurrent() { return getColorDataByIdx(currentColorIdx); }
        public ColorData getColorDataByIdx(int idx)
        {
            if (idx < 0 || idx >= resource.colors.Count) return null;
            return resource.colors[idx];
        }

        public bool onChangeImage(ImageData image)
        {
            int idx = resource.images.IndexOf(image);
            return setCurrentImageIdx(idx);
        }
        public bool onChangeColor(ColorData cd)
        {
            int idx = resource.colors.IndexOf(cd);
            return setCurrentColorIdx(idx);
        }

        public override void updateView() {
            base.updateView();
            if (this.bodyElem == null) return;
            this.bodyElem.updateView();
            setCurrentImageIdx(this.bodyElem.m_nIdx, true);
        }
        public bool setCurrentImageIdx(int idx, bool bOnlyImage = false) {
            if (currentImageIdx == idx) return false;
            if (idx < 0 || idx >= resource.images.Count)
                idx = 0;
            bool bChange = onImageChanged(getImageDataOfCurrent(), getImageDataByIdx(idx), bOnlyImage);
            if (bChange) { 
                currentImageIdx = idx;
                if (bodyElem != null) {
                    bodyElem.m_nIdx = currentImageIdx;                 
                }
            }
            return bChange;
        }
        public bool setCurrentColorIdx(int idx) {
            if (idx < 0 || idx >= resource.colors.Count || currentColorIdx == idx) return false;
            bool bChange = onColorChanged(getColorDataOfCurrent(), getColorDataByIdx(idx));
            if (bChange) { currentColorIdx = idx; }
            return bChange;
        }
        protected virtual bool onImageChanged(ImageData oldImage, ImageData newImage, bool bOnlyImage = false) {
            bool b = onItemDataChanged(oldImage, newImage);
            if (!b) return false;
            if (bodyElem != null) {
                bodyElem.changeImage(newImage.pathImg, bOnlyImage);
            }
            return true;
        }
        protected virtual bool onColorChanged(ColorData oldColor, ColorData newColor)
        {
            bool b = onItemDataChanged(oldColor, newColor);
            if (!b) return false;
            if (bodyElem != null)
            {
                maskColor(newColor.color);
            }
            return true;
        }   
        protected virtual void maskColor(Color color)
        {
            bodyElem.maskColor(color);
        }
        public void onMoreColor(Color color) {
            if (bodyElem != null) {
                maskColor(color);
            }
        }
        public bool onControl(CTRL_TYPE ctrl)
        {
            bool bRet = false;
            if (ctrl == CTRL_TYPE.LEFT)
                bRet = onControlMoveLeft();
            else if (ctrl == CTRL_TYPE.RIGHT)
                bRet = onControlMoveRight();
            else if (ctrl == CTRL_TYPE.UP)
                bRet = onControlMoveUp();
            else if (ctrl == CTRL_TYPE.DOWN)
                bRet = onControlMoveDown();
            else if (ctrl == CTRL_TYPE.ZOOM_IN)
                bRet = onControlZoomIn();
            else if (ctrl == CTRL_TYPE.ZOOM_OUT)
                bRet = onControlZoomOut();
            else if (ctrl == CTRL_TYPE.ROTATE_LEFT)
                bRet = onControlRotateLeft();
            else if (ctrl == CTRL_TYPE.ROTATE_RIGHT)
                bRet = onControlRotateRight();
            else if (ctrl == CTRL_TYPE.ENLARGE)
                bRet = onControlEnlarge();
            else if (ctrl == CTRL_TYPE.NARROW)
                bRet = onControlNarrow();
            return false;
        }
        protected virtual bool onControlMoveUp()
        {
            if (bodyElem != null)
            {
                return bodyElem.moveUp();
            }
            return false;
        }
        protected virtual bool onControlMoveDown()
        {
            if (bodyElem != null)
            {
                return bodyElem.moveDown();
            }
            return false;
        }
        protected virtual bool onControlMoveLeft()
        {
            if (bodyElem != null)
            {
                return bodyElem.moveLeft();
            }
            return false;
        }
        protected virtual bool onControlMoveRight()
        {
            if (bodyElem != null)
            {
                return bodyElem.moveRight();
            }
            return false;
        }
        protected virtual bool onControlRotateLeft()
        {
            if (bodyElem != null)
            {
                return bodyElem.rotateLeft();
            }
            return false;
        }
        protected virtual bool onControlRotateRight()
        {
            if (bodyElem != null)
            {
                return bodyElem.rotateRight();
            }
            return false;
        }
        protected virtual bool onControlZoomIn()
        {
            if (bodyElem != null)
            {
                return bodyElem.zoomIn();
            }
            return false;
        }
        protected virtual bool onControlZoomOut()
        {
            if (bodyElem != null)
            {
                return bodyElem.zoomOut();
            }
            return false;
        }
        protected virtual bool onControlEnlarge()
        {
            if (bodyElem != null)
            {
                return bodyElem.enlarge();
            }
            return false;
        }
        protected virtual bool onControlNarrow()
        {
            if (bodyElem != null)
            {
                return bodyElem.narrow();
            }
            return false;
        }
        public virtual bool onControlAlpha(float alpha)
        {
            if (bodyElem != null)
            {
                return bodyElem.changeAlpha(alpha);
            }
            return false;
        }
        protected virtual bool onItemDataChanged(SelectionItemData oldImage, SelectionItemData newImage)
        {
            if (oldImage != null)
                oldImage.Selected = false;
            if (newImage == null) return false;
            newImage.Selected = true;
            return true;
        }
    }

    public class ButtonGroupData : TypeData
    {
        protected List<TypeData> subTypeDatas = new List<TypeData>();
        public void addSubTypeData(TypeData typedata) { subTypeDatas.Add(typedata); }
        public virtual TypeData getDataOfCurrent() { return getDataByIdx(currentIdx); }
        public TypeData getDataByIdx(int idx)
        {
            if (idx < 0 || idx >= subTypeDatas.Count) return null;
            return subTypeDatas[idx];
        }
        public bool onChangeIdxByButton(RadioButton btn)
        {
            foreach (TypeData de in subTypeDatas)
            {
                RadioButton rb = de.button;
                if (rb == btn)
                {
                    return setCurrentIdx(subTypeDatas.IndexOf(de));
                }
            }
            return false;
        }
        public override bool onItemChanged(object oldValue, object newValue)
        {
            if (newValue == null) return false;
            return true;
        }
        public bool setCurrentIdx(int nIdx)
        {
            if (currentIdx == nIdx) return false;
            bool bChange = onItemChanged(getDataOfCurrent(), getDataByIdx(nIdx));
            if (bChange) { currentIdx = nIdx; }
            return bChange;
        }
        public override void init()
        {
            foreach (TypeData mt in subTypeDatas)
            {
                mt.bMale = this.bMale;
                mt.setBody(body);
                mt.init();                
            }
            reset();            
        }

        public override void reset(bool bForce = false){
            foreach (TypeData mt in subTypeDatas) {               
                mt.reset(bForce);
            }   
            setCurrentIdx(0);
            if (bForce) {
                try {
                    TypeData de = subTypeDatas[0];
                    if(de.button != null)
                        de.button.IsChecked = true;
                } catch(Exception) {
                }
            }            
        }
        public override void updateView() {
            foreach (TypeData mt in subTypeDatas){
                mt.updateView();
            }            
        }
        public override void random() {
            foreach (TypeData mt in subTypeDatas) {
                mt.random();
            }            
        }
    }
    public class MainTypeData : ButtonGroupData
    {
        private StackPanel subTypeButtonPanel;
        public MainTypeData() { }
        public MainTypeData(int type, RadioButton button, StackPanel panel = null)
        {
            this.type = type;            
            this.setButton(button);
            subTypeButtonPanel = panel;
        }
        public StackPanel getTypeButtonPanel() { return subTypeButtonPanel; }
        public override void init()
        {
            base.init();
            if (subTypeButtonPanel != null) subTypeButtonPanel.SetVisible(false);
        }
    }
    public class GenderTypeData : ButtonGroupData
    {        
        public Info info = null;

        public GenderTypeData(bool bMale) { 
            this.bMale = bMale; 
        }
        public bool onChangeMainType(RadioButton mainBtn)
        {
            return onChangeIdxByButton(mainBtn);
        }
        public bool onChangeSubType(RadioButton subBtn)
        {
            MainTypeData td = (MainTypeData)getDataOfCurrent();
            if (td == null) return false;
            return td.onChangeIdxByButton(subBtn);
        }
        public SubTypeData getSubTypeData()
        {
            MainTypeData td = (MainTypeData)getDataOfCurrent();
            if (td == null) return null;
            return (SubTypeData)td.getDataOfCurrent();
        }

        public bool onChangeImage(ImageData id)
        {
            SubTypeData sd = getSubTypeData();
            if (sd == null) return false;
            return sd.onChangeImage(id);
        }

        public bool onChangeColor(ColorData cd)
        {
            SubTypeData sd = getSubTypeData();
            if (sd == null) return false;
            return sd.onChangeColor(cd);
        }
        public void onMoreColor(Color c)
        {
            SubTypeData sd = getSubTypeData();
            if (sd == null) return;
            sd.onMoreColor(c);
        }

        public bool onControl(CTRL_TYPE ctrl)
        {
            SubTypeData sd = getSubTypeData();
            if (sd == null) return false;
            return sd.onControl(ctrl);
        }
        public bool onChangeAlpha(float alpha)
        {
            SubTypeData sd = getSubTypeData();
            if (sd == null) return false;
            return sd.onControlAlpha(alpha);
        }

        public override bool onItemChanged(object oldValue, object newValue)
        {
            if (oldValue != null)
            {
                StackPanel sp = ((MainTypeData)oldValue).getTypeButtonPanel();
                if (sp != null) sp.SetVisible(false);
            }
            if (newValue != null)
            {
                StackPanel sp = ((MainTypeData)newValue).getTypeButtonPanel();
                if (sp != null)
                    sp.SetVisible(true);
                return true;
            }
            return false;
        }
    }
}
