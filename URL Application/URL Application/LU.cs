/* 
Projekt JA - Kalkulator układów równań (metoda LU)
Daniel Winkler sem.5 gr.1
Wersja: 1.6
*/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace URLApplication
{
    class LU
    {
        #region DLL Imports Declarations
        [DllImport("DLL_CPP.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void calculateCPP(int size, double[] mat_L, double[] mat_U, double[] vec_Y, double[] vec_B, double[] vec_X);

        [DllImport("DLL_ASM.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void calculateASM(int size, double* matl, double* matu,
            double* vec_Y, double* vecb, double* vec_X);
        #endregion

        /// <summary> containing size of the matrix </summary>
        readonly private int size;
        /// <summary> Two dimensional array for containing input matrix 
        readonly private double[,] matrixA;
        /// <summary> Array for containing result vector of input matrix </summary>
        readonly private double[] vectorB; 
        /// <summary> Two dimensional array for containing L matrix </summary>
        private double[,] matrixL;
        /// <summary> Two dimensional array for containing U matrix </summary>
        private double[,] matrixU;
        /// <summary> Array for containing vector X </summary>
        private double[] vectorX;
        /// <summary> Array for containing vector Y </summary>
        private double[] vectorY;
        /// <summary> Field for containig mode of application </summary>
        readonly private Mode mode;
        /// <summary> MainWindow instance </summary>
        MainWindow mainWindow;

        /// <summary> Constructor for LU class </summary>
        /// <param name="matA"> Matrix A </param>
        /// <param name="vecB"> Vector B </param>
        /// <param name="size"> Size of matrix A </param>
        /// <param name="mode"> Mode of application </param>
        /// <param name="mainWindow"> Main window of application </param>
        public LU(double[,] matA, double[] vecB, int size, Mode mode, MainWindow mainWindow)
        {
            this.matrixA = matA;
            this.vectorB = vecB;
            this.size = size;
            this.mode = mode;
            MatrixCreation();
            this.mainWindow = mainWindow;
        }

        /// <summary> Returns result of set of equations </summary>
        /// <returns> Double array with vector X </returns>
        public double[] GetResult()
        {
            return vectorX;
        }

        /// <summary> Creates empty matrixes L and U and empty vectors X and Y </summary>
        private void MatrixCreation()
        {
            this.matrixU = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    matrixU[i, j] = matrixA[i, j]*1.0;
            }

            this.matrixL = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i == j)
                        matrixL[i, j] = 1.0;
                    else
                        matrixL[i, j] = 0.0;
                }
            }

            this.vectorY = new double[size];
            this.vectorX = new double[size];
 
            for (int i = 0; i < size; i++)
            {
                vectorY[i] = 0.0;
                vectorX[i] = 0.0;
            }
        }

        /// <summary> Creates matrix L and matrix U </summary>
        /// <returns> True if creation of matrixes L and U is possible. If not, returns false </returns>
        private bool CreateLU()
        {
            for (int k = 0; k < size - 1; k++)
            {
                for (int i = k + 1; i < size; i++)
                {
                    try
                    {
                        if (this.matrixU[k, k] == 0)
                            throw new Exception("Dzielenie przez 0");
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    matrixL[i, k] = matrixU[i, k] / matrixU[k, k];

                    for (int j = k + 1; j < size; j++)
                        matrixU[i, j] = matrixU[i, j] - ((matrixU[i, k] * matrixU[k, j]) / matrixU[k, k]);

                    matrixU[i, k] = 0;
                }
            }
            return true;
        }

        /// <summary> Converts two dimensional array to one dimensional array </summary>
        /// <param name="matrix"> Two dimensional array that will be converted </param>
        /// <returns> One dimensional array </returns>
        private double[] MatrixToArray(double[,] matrix)
        {
            double[] array = new double[size * size];
            int k = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    array[k] = matrix[i, j];
                    k++;
                }
            }
            return array;
        }

        /// <summary> Method that calculates set of equations </summary>
        /// <returns> Vector X, which containts the result of set of equations </returns>
        public double[] Calculate()
        {
            MatrixCreation();
            if (CreateLU())
            {
                double[] aU = MatrixToArray(matrixU);
                double[] aL = MatrixToArray(matrixL);

                if (mode == Mode.ASM || mode == Mode.Compare)
                {
                    unsafe
                    {
                        fixed (double* p1 = &aL[0])
                        {
                            fixed (double* p2 = &aU[0])
                            {
                                fixed (double* p3 = &vectorY[0])
                                {
                                    fixed (double* p4 = &vectorB[0])
                                    {
                                        fixed (double* p5 = &vectorX[0])
                                        {
                                            var watch = new Stopwatch();
                                            watch.Start();
                                            calculateASM(size, p1, p2, p3, p4, p5);
                                            watch.Stop();
                                            double elapsedMs = watch.Elapsed.TotalMilliseconds;
                                            mainWindow.SetTime(elapsedMs.ToString()+"ms", Mode.ASM);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (mode == Mode.C || mode == Mode.Compare)
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    calculateCPP(size, aL, aU, vectorY, vectorB, vectorX);
                    watch.Stop();
                    double elapsedMs = watch.Elapsed.TotalMilliseconds;
                    mainWindow.SetTime(elapsedMs.ToString() + "ms", Mode.C);
                }

                try
                {
                    ValidateResult();
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message);
                }

                return vectorX;
            }
            else
            {
                throw new Exception("Dividing by 0");
            }

        }

        /// <summary> Validates if result is correct </summary>
        private void ValidateResult()
        {
            for(int i = 0; i < size; i++)
            {
                if(Double.IsNaN(vectorX[i]))
                {
                    throw new Exception("Result is not a number");
                }
            }
        }
    }
}