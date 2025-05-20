using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;


namespace ImageToByteArray
{
    class Program
    {
        static void Main(string[] args)
        {
            // Converts Bitmap Image to Array of ints, depending on the Textures, Representing the terrain
            // The first Row of Pixels codes which color represents which terrain:
            //
            // From Left(x:0, y:0) to right(x:7, y:0):
            // Water, Grass, Mountains, Road, Bridge, IronDeposit, Tree, MainBuilding
            string Imagepath = "C:\\Users\\bened\\source\\repos\\ImagetoByteArray\\ImagetoByteArray\\TestMap.bmp";
            string TextfilePath = "C:\\Users\\bened\\source\\repos\\ImagetoByteArray\\ImagetoByteArray\\TestMap.txt";
            Bitmap img = new Bitmap(Imagepath);
            int[,] array = new int[img.Width, img.Height];
            for (int j = 0; j < img.Height; j++)
            {
                for (int i = 0; i < img.Width; i++)
                {
                    Color pixel = img.GetPixel(i, j);

                    // Water 
                    if (pixel == img.GetPixel(0, 0))
                    {
                        array[i, j] = 0;
                    }
                    // Grass 
                    else if (pixel == img.GetPixel(1, 0))
                    {
                        array[i, j] = 1;
                    }
                    // Mountains
                    else if (pixel == img.GetPixel(2, 0))
                    {
                        array[i, j] = 2;
                    }
                    // Road
                    else if (pixel == img.GetPixel(3, 0))
                    {
                        array[i, j] = 3;
                    }
                    // Bridge
                    else if (pixel == img.GetPixel(4, 0))
                    {
                        array[i, j] = 4;
                    }
                    // IronDeposit
                    else if (pixel == img.GetPixel(5, 0))
                    {
                        array[i, j] = 5;
                    }
                    // error
                    else
                    {
                        array[i, j] = 9;
                    }
                    // Tree
                    if (pixel == img.GetPixel(6, 0))
                    {
                        array[i, j] = 6;
                    }
                    // MainBuilding
                    if (pixel == img.GetPixel(7, 0))
                    {
                        array[i, j] = 7;
                    }

                }
            }
            // Bitmap switches coordinates x/y -> Transponieren von array
            int[,] arrayTrans = new int[img.Height, img.Width];
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    arrayTrans[i, j] = array[j, i];
                }
            }

            // writes int Representation in a txt file
            using (StreamWriter st = new StreamWriter(TextfilePath))
            {
                for (int row = 1; row < img.Height; row++)
                {
                    for (int col = 0; col < img.Width; col++)
                    {
                        st.Write(arrayTrans[row, col]/* + " "*/);
                    }
                    st.Write("_");
                }
            }
        }
    }
}
