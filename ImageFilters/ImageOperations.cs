using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ImageFilters
{
    public class ImageOperations
    {
        /// <summary>
        /// Open an image, convert it to gray scale and load it into 2D array of size (Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of gray values</returns>
        /// 

        // A Struct To Help in Return Multiple Values
        public class WindowRes
        {
            public int centerX;
            public int centerY;
            public byte[,] windowArr;

            public WindowRes()
            {

            }
            public WindowRes(int x, int y , byte[,] windowArr)
            {
                this.centerX = x;
                this.centerY = y;
                this.windowArr = windowArr;
            }
        }
        public static byte[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            byte[,] Buffer = new byte[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x] = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x] = (byte)((int)(p[0] + p[1] + p[2]) / 3);
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(byte[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(byte[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(byte[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[0] = p[1] = p[2] = ImageMatrix[i, j];
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        public static List<int> pickWindowAndItsCenter(byte[,] ImageMatrix, int windowSize, int starty , int startx,ref int centerX,ref int centerY)
        {
            
            //check if window can fit on whole matrix or not
            int borderpixeltotal = windowSize - 1; // maxize-1 because if size=5x5 then pixel must have 4 more in each direction
                int endy = starty + borderpixeltotal;
                int endx = startx + borderpixeltotal;
                int hight = ImageOperations.GetHeight(ImageMatrix) - 1;
                int width = ImageOperations.GetWidth(ImageMatrix) - 1;
                if (endy > hight ||  endx > width)
                    return null;

          //if valid then get the whole window  as 1d List

          //1) set the center of the window using refernce param
            centerX = (int)(startx + windowSize / 2);
            centerY = (int)(starty + windowSize / 2);
           
            //2) get the window as 1d array and return it
            List<int> windowArr = new List<int>();
            for (int y = 0; y < windowSize; y++)
                for (int x = 0; x < windowSize; x++)
                {
                    try
                    {
                       
                        windowArr.Add(ImageMatrix[starty + y, startx + x]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("y = " + y + "x = " + x);
                        Console.WriteLine(ex.Message);
                    }
                     
                }
           
            return windowArr;
        }
        public static void AlphaFilter(byte[,] ImageMatrix, int windowSize, PictureBox PicBox, String sortType)
        {
             
            //make sure windowsize is odd
            windowSize = (windowSize % 2 == 0) ? windowSize + 1 : windowSize;
            //get matrix size
            int hight = GetHeight(ImageMatrix);
            int width = GetWidth(ImageMatrix);
            //Get Each Pixel of ImageMatrix

                for (int y = 0; y < hight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int centerX = 0; //to set center of x 
                        int centerY = 0; //to set center of y
                        List<int> CurrentwindowArr = new List<int>();

                        //get windowSize as array and get the center pixel of it 
                        CurrentwindowArr = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY);
                        if (CurrentwindowArr != null)
                        {
                            // 1) sort array
                            CurrentwindowArr.Sort();
                            // 2) remove smallest and biggest values
                            CurrentwindowArr.RemoveAt(0);
                            CurrentwindowArr.RemoveAt(CurrentwindowArr.Count - 1);

                            // 3) calculate the average
                            int avg = 0;
                            int sum = 0;
                            foreach (int val in CurrentwindowArr)
                                sum += val;
                            avg = (int)sum / CurrentwindowArr.Count;


                            // 4) change  the selected pixel as center of window to the average value
                            try
                            {
                                ImageMatrix[centerY,centerX] = (byte)avg;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
    

                        }
                       



                    }
                

                // Display it using Imageoperation.DisplayImage()
                ImageOperations.DisplayImage(ImageMatrix, PicBox);
            }


        }




    }
}
