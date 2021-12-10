using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartoonizer
{
    public static class CONST {
        public static string EMPTY_IMAGE_PATH = "/Icons/close-32x32.png";
        public static string INTERNAL_IMG_PREFIX = "pack://application:,,,/Images";
        public static char[] DELIMITERS = { '_', '-', ',', '.', ':', '\t' }; 
    }    
    
    public enum CTRL_TYPE {
        LEFT = 0,
        RIGHT,
        UP,
        DOWN,
        ROTATE_LEFT,
        ROTATE_RIGHT,
        ZOOM_IN,
        ZOOM_OUT,
        ENLARGE,
        NARROW,
        ALPHA,
    };
    public enum MainMode {
        FACE = 0,
        EYES = 1,
        HAIR = 2,
        CLOTHES = 3,
        BACK = 4,
    }    
    public struct ELEMINFO_TYPE {
        public string main;
        public string sub;
        public ELEMINFO_TYPE(string main, string sub)
        {
            this.main = main;
            this.sub = sub;
        }
        public string toString()
        {
            return main + "_" + sub;
        }
    }

    public static class ELEMINFO_TYPES
    {
        public static ELEMINFO_TYPE BACK = new ELEMINFO_TYPE("back", "");
        public static ELEMINFO_TYPE NECK = new ELEMINFO_TYPE("neck", "");

        public static ELEMINFO_TYPE FACE_SHAPE = new ELEMINFO_TYPE("face", "shape");
        public static ELEMINFO_TYPE FACE_NOSE = new ELEMINFO_TYPE("face", "nose");
        public static ELEMINFO_TYPE FACE_MOUTH = new ELEMINFO_TYPE("face", "mouth");
        public static ELEMINFO_TYPE FACE_EAR = new ELEMINFO_TYPE("face", "ear");
        public static ELEMINFO_TYPE FACE_EAR_LEFT = new ELEMINFO_TYPE("face", "ear_left");
        public static ELEMINFO_TYPE FACE_EAR_RIGHT = new ELEMINFO_TYPE("face", "ear_right");
        public static ELEMINFO_TYPE FACE_CHEER = new ELEMINFO_TYPE("face", "blusher");

        public static ELEMINFO_TYPE EYE_SHAPE = new ELEMINFO_TYPE("eye", "shape");
        public static ELEMINFO_TYPE EYE_SHAPE_LEFT = new ELEMINFO_TYPE("eye", "shape_left");
        public static ELEMINFO_TYPE EYE_SHAPE_RIGHT = new ELEMINFO_TYPE("eye", "shape_right");
        public static ELEMINFO_TYPE EYE_IRIS = new ELEMINFO_TYPE("eye", "iris");
        public static ELEMINFO_TYPE EYE_IRIS_LEFT = new ELEMINFO_TYPE("eye", "iris_left");
        public static ELEMINFO_TYPE EYE_IRIS_RIGHT = new ELEMINFO_TYPE("eye", "iris_right");
        public static ELEMINFO_TYPE EYE_BROW = new ELEMINFO_TYPE("eye", "brow");
        public static ELEMINFO_TYPE EYE_BROW_LEFT = new ELEMINFO_TYPE("eye", "brow_left");
        public static ELEMINFO_TYPE EYE_BROW_RIGHT = new ELEMINFO_TYPE("eye", "brow_right");
        public static ELEMINFO_TYPE EYE_MAKEUP = new ELEMINFO_TYPE("eye", "makeup");
        public static ELEMINFO_TYPE EYE_MAKEUP_LEFT = new ELEMINFO_TYPE("eye", "makeup_left");
        public static ELEMINFO_TYPE EYE_MAKEUP_RIGHT = new ELEMINFO_TYPE("eye", "makeup_right");
        public static ELEMINFO_TYPE EYE_GLASS = new ELEMINFO_TYPE("eye", "glass");

        public static ELEMINFO_TYPE HAIR_HAIR = new ELEMINFO_TYPE("hair", "onhead");
        public static ELEMINFO_TYPE HAIR_ACCESSORY = new ELEMINFO_TYPE("hair", "accessory");
        public static ELEMINFO_TYPE HAIR_MUSTACHE = new ELEMINFO_TYPE("hair", "mustache");
        public static ELEMINFO_TYPE HAIR_BEARD = new ELEMINFO_TYPE("hair", "beard");

        public static ELEMINFO_TYPE CLOTH = new ELEMINFO_TYPE("cloth", "");
    }
}
