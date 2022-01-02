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

        public static int[] pickWindowAndItsCenter(byte[,] ImageMatrix, int windowSize, int CurrentPixelRow, int CurrentPixelColumn, ref int centerX, ref int centerY , ref int kthsmallest, ref int kthlargest)
        {

            //check if window can fit on whole matrix or not
            int borderpixeltotal = windowSize - 1; // maxize-1 because if size=5x5 then Currentpixelposition must have 4 pixels more in y and x direction
            int lastpixelRow = CurrentPixelRow + borderpixeltotal;
            int lastpixelColumn = CurrentPixelColumn + borderpixeltotal;
            int Imghight = ImageOperations.GetHeight(ImageMatrix) - 1;
            int Imgwidth = ImageOperations.GetWidth(ImageMatrix) - 1;
            if (lastpixelRow > Imghight || lastpixelColumn > Imgwidth)
                return null;

            //if valid then get the whole window  as 1d List

            //1) set the center of the window using refernce param
            centerX = CurrentPixelColumn + windowSize / 2;
            centerY = CurrentPixelRow + windowSize / 2;
            //2) get the window as 1d array and return it
            int[] windowArr = new int[windowSize * windowSize];
            int counter = 0;
            kthlargest = -1;
            kthsmallest = 999;
            for (int y = 0; y < windowSize; y++)
            {
                for (int x = 0; x < windowSize; x++)
                {
                    try
                    {
                        windowArr[counter] = ImageMatrix[CurrentPixelRow + y, CurrentPixelColumn + x];
                        if (kthlargest < windowArr[counter])
                            kthlargest = windowArr[counter];
                        
                        if (kthsmallest > windowArr[counter])
                            kthsmallest = windowArr[counter];
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("y = " + y + "x = " + x);
                        Console.WriteLine(ex.Message);
                        return null;
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
                 
                        int avg = 0;
                        int sum = 0;

                        // 1) sort array
                        if (sortType == "countingSort")
                        {
                            ImageOperations.countingSort(Array1d);
                            for (int k = 1; k < Array1d.Length - 1; k++)// countingsort => sort then add from 2nd to last -1 skipped min,max values 
                                sum += Array1d[k];
                        }
                        else if (sortType == "kthElementSort")
                        {
                            for (int k = 0; k < Array1d.Length; k++) //kthelement => add total item and subtract max,min
                                sum += Array1d[k];

                            sum = sum - (kthsmallest+kthlargest);
                        }

                        // 2) calculate the average
                        avg = (sum / (Array1d.Length - 2));
                        
                       // 3) change  the selected pixel as center of window to the average value

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

                //
                
            }

            //show image
            ImageOperations.DisplayImage(ImageMatrix, PicBox);

        }


        public static void AdaptiveFilter(byte[,] ImageMatrix, int windowSize, int maxSize, PictureBox PicBox, String sortType,bool debugMode )
        {

            //make sure windowsize is odd
            windowSize = (windowSize % 2 == 0) ? windowSize + 1 : windowSize;
            maxSize = (maxSize % 2 == 0) ? maxSize + 1 : maxSize;
            maxSize = (maxSize == 3) ? maxSize + 2 : maxSize;
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
                        bool medianflag = false;
                        while (windowSize <= maxSize)
                        {
                            // 1) sort array
                            if (sortType == "countingSort") //beter preformance with adaptive
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
                            //Console.WriteLine(Zmedian);
                            //if(Zmedian == 0)
                            //    Console.WriteLine("Zeroooooooooooo");
                            //Check if we found median , break
                            if (A1 > 0 && A2 > 0)
                            {
                                medianflag = true;
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
                            if (medianflag == true)
                            {
                                B1 = Zxy - Zmin;
                                B2 = Zmax - Zxy;
                                if (B1 > 0 && B2 > 0)
                                    ImageMatrix[centerY, centerX] = Zxy; // this pixel is not noise
                                else
                                    ImageMatrix[centerY, centerX] = (byte)Zmedian;//this is noise pixel and replace with median

                            }else //windowsize is greater than maxsize 
                            {
                                Console.WriteLine(Zmedian);

                               if(debugMode == true)
                                    alert(5);

                                ImageMatrix[centerY, centerX] = (byte)Zxy;
                                return;
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }


                    }




                }


                
           
            }
            //show image
            ImageOperations.DisplayImage(ImageMatrix, PicBox);


        }
        //################################[SORTING ALGORIHTM]################################


       public  static void countingSort(int[] arr)
        {
            int n = arr.Length;

            // The output intger array that
            // will have sorted arr
            int[] output = new int[n];

            // Create a count array to store
            // count of individual intger
            // and initialize count array as 0
            int[] count = new int[256];

            for (int i = 0; i < 256; ++i)
                count[i] = 0;

            // store count of each intger
            for (int i = 0; i < n; ++i)
                ++count[arr[i]];

            // Change count[i] so that count[i]
            // now contains actual position of
            // this intger in output array
            for (int i = 1; i <= 255; ++i)
                count[i] += count[i - 1];

            // Build the output intger array
            // To make it stable we are operating in reverse order.
            for (int i = n - 1; i >= 0; i--)
            {
                output[count[arr[i]] - 1] = arr[i];
                --count[arr[i]];
            }

            // Copy the output array to arr, so
            // that arr now contains sorted
            // intger
            for (int i = 0; i < n; ++i)
                arr[i] = output[i];
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


        public static void alert(int x)
        {
            if(x==1)
           MessageBox.Show("Please Enter WindowSize & WindowMaxSize","Error Fields Are Null");
            else if (x==2)
                MessageBox.Show("Please Load An Image ","Error No Image Found");
            else if (x == 3)
                MessageBox.Show("Done ! ", "Task Completed Successfully");
            else if (x == 4)
                MessageBox.Show("Please Enter Valid Values For WindowSize & WindowMaxSize");
            else if (x == 5)
                MessageBox.Show("This program Uses Filtering By SlidingWindow[No-Padding Method]Please Increase MaxWindowSize , To Properly Filter the Image  ");
        }


    }
}





 
