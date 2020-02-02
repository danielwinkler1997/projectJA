//------------------------------
// Projekt JA
// Daniel Winkler sem.5 gr.1
// Version: 1.0
//------------------------------

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JAProject
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

        /// <summary>
        /// Field containing size of the matrix
        /// </summary>
        private int size;
        /// <summary>
        /// Two dimensional array for containing input matrix
        /// </summary>
        private double[,] matrixA;
        /// <summary>
        /// Array for containing result vector of input matrix 
        /// </summary>
        private double[] vectorB;
        /// <summary>
        /// Two dimensional array for containing L matrix 
        /// </summary>
        private double[,] matrixL;
        /// <summary>
        /// Two dimensional array for containing U matrix 
        /// </summary>
        private double[,] matrixU;
        /// <summary>
        /// Array for containing vector X 
        /// </summary>
        private double[] vectorX;
        /// <summary>
        /// Array for containing vector Y 
        /// </summary>
        private double[] vectorY;
        /// <summary>
        /// Field for containig mode of application
        /// </summary>
        private Mode mode;
        /// <summary>
        /// Contains mainWindow to allow set time results
        /// </summary>
        MainWindow mainWindow;

        /// <summary>
        /// Constructor for LU class
        /// </summary>
        /// <param name="matA">Matrix A</param>
        /// <param name="vecB">Vector B</param>
        /// <param name="size">Size of matrix A</param>
        /// <param name="mode">Mode of application</param>
        /// <param name="mainWindow">Main window of application</param>
        public LU(double[,] matA, double[] vecB, int size, Mode mode, MainWindow mainWindow)
        {
            this.matrixA = matA;
            this.vectorB = vecB;
            this.size = size;
            this.mode = mode;
            matrixCreation();
            this.mainWindow = mainWindow;
        }
        /// <summary>
        /// Returns result
        /// </summary>
        /// <returns>double array with vector X</returns>
        public double[] getResult()
        {
            return vectorX;
        }
        /// <summary>
        /// Creates empty matrixes
        /// </summary>
        private void matrixCreation()
        {
            this.matrixU = new double[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    matrixU[i, j] = matrixA[i, j] * 1.0;
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
        /// <summary>
        /// Creates matrix L and matrix U
        /// </summary>
        /// <returns>True if creation of matrixes L and U is possible. If not, returns false</returns>
        private bool createLU()
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
        /// <summary>
        /// Converts two dimensional array to one dimensional array
        /// </summary>
        /// <param name="matrix">Two dimensional array to convert</param>
        /// <returns>Converted one dimensional array</returns>
        private double[] matrixToArray(double[,] matrix)
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
        /// <summary>
        /// Method that calculates set of equations
        /// </summary>
        /// <returns>Vector X as an array</returns>
        public double[] calculate()
        {
            matrixCreation();
            if (createLU())
            {
                double[] aU = matrixToArray(matrixU);
                double[] aL = matrixToArray(matrixL);

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
                                            //calculateASM(size, p1, p2, p3, p4, p5);
                                            watch.Stop();
                                            double elapsedMs = watch.Elapsed.TotalMilliseconds;
                                            mainWindow.setTime(elapsedMs.ToString() + "ms", Mode.ASM);
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
                    mainWindow.setTime(elapsedMs.ToString() + "ms", Mode.C);
                }

                try
                {
                    validateResult();
                }
                catch (Exception e)
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
        /// <summary>
        /// Validates if result is correct
        /// </summary>
        private void validateResult()
        {
            for (int i = 0; i < size; i++)
            {
                if (Double.IsNaN(vectorX[i]))
                {
                    throw new Exception("Result is not a number");
                }
            }
        }
    }
}