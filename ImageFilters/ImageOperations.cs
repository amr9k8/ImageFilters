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

        public static int[] pickWindowAndItsCenter(byte[,] ImageMatrix, int windowSize, int starty, int startx, ref int centerX, ref int centerY , ref int kthsmallest, ref int kthlargest)
        {

            //check if window can fit on whole matrix or not
            int borderpixeltotal = windowSize - 1; // maxize-1 because if size=5x5 then pixel must have 4 more in each direction
            int endy = starty + borderpixeltotal;
            int endx = startx + borderpixeltotal;
            int hight = ImageOperations.GetHeight(ImageMatrix) - 1;
            int width = ImageOperations.GetWidth(ImageMatrix) - 1;
            if (endy > hight || endx > width)
                return null;

            //if valid then get the whole window  as 1d List

            //1) set the center of the window using refernce param
            centerX = startx + windowSize / 2;
            centerY = starty + windowSize / 2;
            //2) get the window as 1d array and return it
            int[] windowArr = new int[windowSize * windowSize];
            int counter = 0;
            int max = -1;
            int min = 999;
            for (int y = 0; y < windowSize; y++)
            {
                for (int x = 0; x < windowSize; x++)
                {
                    try
                    {
                        windowArr[counter] = ImageMatrix[starty + y, startx + x];
                        if (max < windowArr[counter])
                        {
                            max = windowArr[counter];
                            kthlargest = counter;
                        } 
                        if (min > windowArr[counter])
                        {
                            min = windowArr[counter];
                            kthsmallest = counter;
                        }
                           
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("y = " + y + "x = " + x);
                        Console.WriteLine(ex.Message);
                    }
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
            bool flagKthElement = false;
            //Get Each Pixel of ImageMatrix
            for (int y = 0; y < hight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    
                    int centerX = 0; //to set center of x 
                    int centerY = 0; //to set center of y
                    int kthsmallest = -1;
                    int kthlargest = -1;
                   

                    //get Selected Window as 1darray and get the center pixel of it 
                    int[] Array1d = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY,ref kthsmallest ,ref kthlargest);
                    if (Array1d != null)
                    {
                        // 1) sort array
                        if (sortType == "countingSort")
                            ImageOperations.countingSort(Array1d);
                       else if (sortType == "kthElementSort")
                        {
                            flagKthElement = true;
                        }
                            


                        // 2) remove smallest and biggest values

                        // 3) calculate the average
                        int avg = 0;
                        int sum = 0;
                        if (flagKthElement == false)
                        {
                            for (int k = 1; k < Array1d.Length - 1; k++)
                                sum += Array1d[k];
                        }
                        else if (flagKthElement == true)
                        {
                            for (int k = 0; k < Array1d.Length; k++)
                                sum += Array1d[k];

                            sum = sum - (Array1d[kthsmallest] + Array1d[kthlargest]);
                            flagKthElement = false;
                        }

                        avg = (sum / (Array1d.Length - 2));
                        // 4) change  the selected pixel as center of window to the average value

                        try
                        {
                            ImageMatrix[centerY, centerX] = (byte)avg;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }



                        
                       


                    }




                }


                //show image
                ImageOperations.DisplayImage(ImageMatrix, PicBox);
            }


        }


        public static void AdaptiveFilter(byte[,] ImageMatrix, int windowSize, int maxSize, PictureBox PicBox, String sortType)
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
                    int kthsmallest =-1;
                    int kthlargest=-1;
                    //get windowSize as array and get the center pixel of it 
                    int[] Array1d = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY,ref kthsmallest, ref kthlargest);
                    if (Array1d != null)
                    {
                        int Zmedian = 0;
                        int Zmin = 0;
                        int Zmax = 0;
                        byte Zxy = ImageMatrix[centerY, centerX];
                        int A1 = 0;
                        int A2 = 0;
                        int middleIndex = 0;
                        int B1 = 0;
                        int B2 = 0;
                        while (windowSize <= maxSize)
                        {
                            // 1) sort array
                            if (sortType == "countingSort")
                                ImageOperations.countingSort(Array1d);
                            else if (sortType == "quickSort")
                                ImageOperations.quickSort(Array1d, 0, Array1d.Length - 1);

                            //Array.Sort(Array1d);
                            // 2) calculate the median,A1,A2,Zmax,Zmin,Zmedian
                            Zmin = Array1d[0];
                            Zmax = Array1d[Array1d.Length - 1];
                            Zmedian = Array1d[Array1d.Length / 2];
                            A1 = Zmedian - Zmin;
                            A2 = Zmax - Zmedian;
                            //Check if we found median , break
                            if (A1 > 0 && A2 > 0)
                            {
                                //Console.WriteLine("Found Median value , : "+Zmedian);
                                break;
                            }
                            //Reapeat till we find a median
                            windowSize += 2;
                            Array1d = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY, ref kthsmallest,ref kthlargest);

                        }

                        //Console.WriteLine(Zmedian);

                        // 4) change  the selected pixel if only it's not noise
                        try
                        {
                            B1 = Zxy - Zmin;
                            B2 = Zmax - Zxy;
                            if (B1 > 0 && B2 > 0)
                                ImageMatrix[centerY, centerX] = Zxy; // this pixel is not noise
                            else
                                ImageMatrix[centerY, centerX] = (byte)Zmedian;//this is noise pixel and replace with median

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }


                    }




                }


                //show image
                ImageOperations.DisplayImage(ImageMatrix, PicBox);
            }


        }
        //################################[SORTING ALGORIHTM]################################




        // function for counting sort
        public static void countingSort(int[] Array)
        {
            int n = Array.Length;
            int max = 0;
            //find largest element in the Array
            for (int i = 0; i < n; i++)
            {
                if (max < Array[i])
                {
                    max = Array[i];
                }
            }

            //Create a freq array to store number of occurrences of 
            //each unique elements in the given array 
            int[] freq = new int[max + 1];
            for (int i = 0; i < max + 1; i++)
            {
                freq[i] = 0;
            }
            for (int i = 0; i < n; i++)
            {
                freq[Array[i]]++;
            }

            //sort the given array using freq array
            for (int i = 0, j = 0; i <= max; i++)
            {
                while (freq[i] > 0)
                {
                    Array[j] = i;
                    j++;
                    freq[i]--;
                }
            }
        }

        public static void quickSort(int[] arr, int start, int end)
        {
            int i;
            if (start < end)
            {
                i = Partition(arr, start, end);

                quickSort(arr, start, i - 1);
                quickSort(arr, i + 1, end);
            }
        }

        public static int Partition(int[] arr, int start, int end)
        {
            int temp;
            int p = arr[end];
            int i = start - 1;

            for (int j = start; j <= end - 1; j++)
            {
                if (arr[j] <= p)
                {
                    i++;
                    temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                }
            }

            temp = arr[i + 1];
            arr[i + 1] = arr[end];
            arr[end] = temp;
            return i + 1;
        }


    }
}





 
