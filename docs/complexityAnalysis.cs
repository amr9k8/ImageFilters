using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
namespace ImageFilters
{


        // O(N^2)
        public static int[] pickWindowAndItsCenter(byte[,] ImageMatrix, int windowSize, int CurrentPixelRow, int CurrentPixelColumn, ref int centerX, ref int centerY, ref int kthsmallest, ref int kthlargest)
        {

            // O(1)
                        int borderpixeltotal = windowSize - 1; 
                        int lastpixelRow = CurrentPixelRow + borderpixeltotal;
                        int lastpixelColumn = CurrentPixelColumn + borderpixeltotal;
                        int Imghight = ImageOperations.GetHeight(ImageMatrix) - 1;
                        int Imgwidth = ImageOperations.GetWidth(ImageMatrix) - 1;
                        if (lastpixelRow > Imghight || lastpixelColumn > Imgwidth)
                            return null;
                        centerX = CurrentPixelColumn + windowSize / 2;
                        centerY = CurrentPixelRow + windowSize / 2;
                        int[] windowArr = new int[windowSize * windowSize];
                        int counter = 0;
                        kthlargest = -1;
                        kthsmallest = 999;
            // O(N^2)
                        for (int y = 0; y < windowSize; y++)//O(N)
            {
                            for (int x = 0; x < windowSize; x++) //O(N)
                            {
                                    //O(1)
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

            }   // O(1)


            return windowArr;//O(1)
        }
        
        
        public static void AlphaFilter(byte[,] ImageMatrix, int windowSize, PictureBox PicBox, String sortType)
        {

            // O(1)
                    windowSize = (windowSize % 2 == 0) ? windowSize + 1 : windowSize;
                    int hight = GetHeight(ImageMatrix);
                    int width = GetWidth(ImageMatrix);
                    bool flagKthElement = false;
            // O(   N^2 * K^2 * (n+k) ) N=> FOR IMG (W,H) , K=> for windowsize , (n+k) for counting sort
            // O(   N^2 * K^2 * n ) N=> FOR IMG (W,H) , K=> for windowsize , n for Kth sort
            for (int y = 0; y < hight; y++)     // O(N)
            {
                for (int x = 0; x < width; x++) // O(N)
                {
                    // O(1)         
                        int centerX = 0; //to set center of x 
                        int centerY = 0; //to set center of y
                        int kthsmallest = -1;
                        int kthlargest = -1;


                    // O(K^2) k => windowsize
                        int[] Array1d = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY, ref kthsmallest, ref kthlargest);

                    if (Array1d != null)
                    {

                        int avg = 0;
                        int sum = 0;

                        //O(n+k)
                        if (sortType == "countingSort")
                        {     

                            ImageOperations.countingSort(Array1d);//O(n+k)

                            for (int k = 1; k < Array1d.Length - 1; k++)//O(n)
                                sum += Array1d[k];
                        }
                        else if (sortType == "kthElementSort")    //O(n) 
                        {
                            for (int k = 0; k < Array1d.Length; k++) //kthelement => add total item and subtract max,min
                                sum += Array1d[k];

                            sum = sum - (kthsmallest + kthlargest);
                        }
                        //O(1)
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
                ImageOperations.DisplayImage(ImageMatrix, PicBox);

            }


        }


        public static void AdaptiveFilter(byte[,] ImageMatrix, int windowSize, int maxSize, PictureBox PicBox, String sortType)
        {
        //O(1)
                windowSize = (windowSize % 2 == 0) ? windowSize + 1 : windowSize;
                int hight = GetHeight(ImageMatrix);
                int width = GetWidth(ImageMatrix);


            // O(   N^2 * K^2 * M*( (n^2)(K^2) ) )  N^2=> FOR IMG (W,H) , K^2=> for windowsize , (n+k) for counting sort
            // O(   N^2 * K^2 * M*( (n+k)(K^2) ) )  N^2=> FOR IMG (W,H) , K^2=> for windowsize , n^2 for Quick sort
            for (int y = 0; y < hight; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int centerX = 0; //to set center of x 
                    int centerY = 0; //to set center of y
                    int kthsmallest = -1;
                    int kthlargest = -1;
                    // O(K ^ 2)
                    int[] Array1d = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY, ref kthsmallest, ref kthlargest);
                    if (Array1d != null)
                    {
                        //O(1)
                                int Zmedian = 0;
                                int Zmin = 0;
                                int Zmax = 0;
                                byte Zxy = ImageMatrix[centerY, centerX];
                                int A1 = 0;
                                int A2 = 0;
                                int middleIndex = 0;
                                int B1 = 0;
                                int B2 = 0;
                        // M=> MAXSIZE for while loop
                        //O(M*(n^2)(K^2) ) // FOR QUCIK SORT
                        //O(M*(n+k)(K^2) )// FOR Counting SORT
                        while (windowSize <= maxSize)
                                {
                                    if (sortType == "countingSort")
                                    //O(n + k)
                                        ImageOperations.countingSort(Array1d);
                                    else if (sortType == "quickSort")
                                    //O(n^2)
                                         ImageOperations.quickSort(Array1d, 0, Array1d.Length - 1);
                                    Zmin = Array1d[0];
                                    Zmax = Array1d[Array1d.Length - 1];
                                    Zmedian = Array1d[Array1d.Length / 2];
                                    A1 = Zmedian - Zmin;
                                    A2 = Zmax - Zmedian;
                                    if (A1 > 0 && A2 > 0)
                                        break;
                                    windowSize += 2;
                                 // O(K^2) 
                                    Array1d = pickWindowAndItsCenter(ImageMatrix, windowSize, y, x, ref centerX, ref centerY, ref kthsmallest, ref kthlargest);

                                }

                          //O(1)
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
                //O(1)
                ImageOperations.DisplayImage(ImageMatrix, PicBox);

            }


        }


        //################################[SORTING ALGORIHTM]################################

        //O(n+k) n is the number of elements , k is the range of elements (k= largest element - smallest element)
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
      

        //O(N^2)
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
            if (x == 1)
                MessageBox.Show("Please Enter WindowSize & WindowMaxSize", "Error Fields Are Null");
            else if (x == 2)
                MessageBox.Show("Please Load An Image ", "Error No Image Found");
            else if (x == 3)
                MessageBox.Show("Done ! ", "Task Completed Successfully");
            else if (x == 4)
                MessageBox.Show("Please Enter Valid Values For WindowSize & WindowMaxSize");
        }


    }
}






