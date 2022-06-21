using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace WpfTutorialSamples.Audio_and_Video {
    class BitmapMaker {
        //THE BITMAP'S SIZE.
        private int _width;
        private int _height;

        //THE PIXEL ARRAY.
        private byte[] _pixelData;

        //THE NUMBER OF BYTES PER ROW.
        private int _stride;


        #region PROPERTIES
        public int Width {
            get { return _width; }
        }//end property
        public int Height {
            get { return _height; }
        }//end property
        #endregion


        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">Width of the bitmap in pixels</param>
        /// <param name="height">Height of the bitmap in pixels</param>
        public BitmapMaker(int width, int height) {
            // Save the width and height.
            _width = width;
            _height = height;

            // Create the pixel array.
            _pixelData = new byte[width * height * 4];

            // Calculate the stride.
            _stride = width * 4;
        }//end constructor

        public BitmapMaker(string imagePath) {
            LoadImage(imagePath);
        }//end constructor
        #endregion


        #region PUBLIC METHODS
        /// <summary>
        /// Get a pixel's component values in an array of buytes [r,g,b,a]
        /// </summary>
        /// <param name="x">The x position of the pixel to get data from.</param>
        /// <param name="y">The y position of the pixel to get data from.</param>
        /// <returns></returns>
        public byte[] GetPixelData(int x, int y) {
            //STARTING PIXEL INDEX
            int index = y * _stride + x * 4;

            //GET PIXEL COMPONENT VALUES 
            byte blu = _pixelData[index++];// ++ to march forward to get next component 
            byte grn = _pixelData[index++];
            byte red = _pixelData[index++];
            byte alp = _pixelData[index];

            //RETURN DATA
            return new byte[] { red, grn, blu, alp };
        }//end method

        /// <summary>
        /// Get a pixel's component values in a Color instance.
        /// </summary>
        /// <param name="x">The x position of the pixel to get data from.</param>
        /// <param name="y">The y position of the pixel to get data from.</param>
        /// <returns></returns>
        public Color GetPixelColor(int x, int y) {
            //GET PIXEL DATA
            byte[] pixelComponentData = GetPixelData(x, y);

            //CREAT COLOR INSTANCE
            Color returnColor = new Color();

            //POPULATE COLOR INSTANCE DATA THEN RETURN
            returnColor.R = pixelComponentData[0];
            returnColor.G = pixelComponentData[1];
            returnColor.B = pixelComponentData[2];
            returnColor.A = pixelComponentData[3];

            return returnColor;
        }//end method

        /// <summary>
        /// Set a pixel's color values
        /// </summary>
        /// <param name="x">zero based pixel x position</param>
        /// <param name="y">zero based pixel y position</param>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        /// <param name="alpha">0-255 level of alpha</param>
        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha) {
            int index = y * _stride + x * 4;
            _pixelData[index++] = blue;
            _pixelData[index++] = green;
            _pixelData[index++] = red;
            _pixelData[index++] = alpha;
        }//end method

        /// <summary>
        /// Set a pixel's color values
        /// </summary>
        /// <param name="x">zero based pixel x position</param>
        /// <param name="y">zero based pixel y position</param>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        public void SetPixel(int x, int y, byte red, byte green, byte blue) {
            SetPixel(x, y, red, green, blue, 255);
        }//end method

        /// <summary>
        /// Set a pixel's color values
        /// </summary>
        /// <param name="x">zero based pixel x position</param>
        /// <param name="y">zero based pixel y position</param>
        /// <param name="color">Color instance</param>
        public void SetPixel(int x, int y, Color color) {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }//end method

        /// <summary>
        /// Set all pixels to a specific color.
        /// </summary>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        /// <param name="alpha">0-255 level of alpha</param>
        public void SetPixels(byte red, byte green, byte blue, byte alpha) {
            int byteCount = _width * _height * 4;
            int index = 0;

            while (index < byteCount) {
                _pixelData[index++] = blue;
                _pixelData[index++] = green;
                _pixelData[index++] = red;
                _pixelData[index++] = alpha;
            }//end while
        }//end method

        /// <summary>
        /// Set all pixels to a specific color.
        /// </summary>
        /// <param name="red"> 0-255 level of red</param>
        /// <param name="green">0-255 level of green</param>
        /// <param name="blue">0-255 level of blue</param>
        public void SetPixels(byte red, byte green, byte blue) {
            SetPixels(red, green, blue, 255);
        }//end method

        /// <summary>
        /// Create a WriteableBitmap
        /// </summary>
        /// <returns>WriteableBitmap inatance.</returns>
        public WriteableBitmap MakeBitmap() {
            // Create the WriteableBitmap.
            int dpi = 96;

            WriteableBitmap wbitmap = new WriteableBitmap(_width, _height, dpi, dpi, PixelFormats.Bgra32, null);

            // Load the pixel data.
            Int32Rect rect = new Int32Rect(0, 0, _width, _height);
            wbitmap.WritePixels(rect, _pixelData, _stride, 0);

            // Return the bitmap.
            return wbitmap;
        }//end method

        /// <summary>
        /// Returns a copy of the loaded pixel data in BGRA format.
        /// </summary>
        /// <returns>byte[]</returns>
        public byte[] GetPixelData() {
            byte[] returnData = new byte[_pixelData.Length];

            for (int index = 0; index < _pixelData.Length; index++) {
                returnData[index] = _pixelData[index];
            }//end for

            return returnData;
        }//end method 
        #endregion


        #region PRIVATE METHODS
        private void LoadImage(string path) {
            //OPEN THE IMAGE
            FileStream imageStream = File.OpenRead(path);

            BitmapDecoder pngDecoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.None, BitmapCacheOption.Default);

            //TODO CHECK IF IMAGE DATA FORMAT IS RGBA

            BitmapSource srcImage = pngDecoder.Frames[0];

            // SAVE THE WIDTH AND HEIGHT.
            _width = srcImage.PixelWidth;
            _height = srcImage.PixelHeight;

            // CALCULATE THE STRIDE.
            _stride = _width * 4;

            //CREATE AN ARRAY BIG ENOUGHT TO HOLD THE PIXEL DATA
            _pixelData = new byte[_width * _height * 4];

            //STORE THE PIXEL DATA TO THE PRIVATE BACKING FIELD
            srcImage.CopyPixels(_pixelData, _stride, 0);
        }
        #endregion
    }//end class
}//end namespace
