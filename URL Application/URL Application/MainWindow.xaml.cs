/*
Projekt JA - Kalkulator układów równań(metoda LU)
Daniel Winkler sem.5 gr.1
Wersja: 1.5
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace URLApplication
{
    /// <summary> Enum type to set application mode </summary>
    public enum Mode { C, ASM, Compare, Default };

    /// <summary> Interaction logic for MainWindow.xaml </summary>
    public partial class MainWindow : Window
    {
        /// <summary> Field for containing size of the matrix </summary>
        private int size;
        /// <summary> Two dimensional array for containing input matrix </summary>
        private double[,] matrixA;
        /// <summary> Array for containing result vector of input matrix </summary>
        private double[] vectorB;
        /// <summary> List containing input data - matrix A annd vector B </summary>
        private List<List<double>> listInputData;
        /// <summary> Field for containig mode of application </summary>
        Mode mode;
        /// <summary> Bool to define if equations matrix is created or not </summary>
        bool isEquationsMatrixCreated;

        /// <summary> MainWindow Constructor </summary>
        public MainWindow()
        {
            InitializeComponent();
            mode = Mode.Default;
            isEquationsMatrixCreated = false;
            this.MinHeight = 600;
            this.MinWidth = 900;
        }

        /// <summary> Method that creates matrix and vector for input data </summary>
        private void MatrixCreation()
        {
            matrixA = new double[size, size];
            vectorB = new double[size];
        }

        /// <summary> Method that creates DataGrid for equations </summary>
        private void EquationsDatagridCreation()
        {
            this.listInputData = new List<List<double>>();

            for (int i = 0; i < size; i++)
            {
                this.listInputData.Add(new List<double>());
            }

            int counter = 0;
            foreach (List<double> sublist in this.listInputData)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Header = "x" + counter,
                    Binding = new Binding("[" + counter + "]"),
                    Width = Main_window.Width / (2 * (size + 1))
                };
                equationsDatagrid.Columns.Add(textColumn);
                counter++;
            }
            DataGridTextColumn resultColumn = new DataGridTextColumn
            {
                Header = "y",
                Binding = new Binding("[" + counter + "]"),
                Width = Main_window.Width / (2 * (size + 1))
            };
            equationsDatagrid.Columns.Add(resultColumn);

            Random rand = new Random();
            foreach (List<double> sublist in this.listInputData)
            {
                for (int i = 0; i <= counter; i++)
                    sublist.Add(rand.NextDouble());
            }

            equationsDatagrid.MaxWidth = Main_window.Width / 2;
            equationsDatagrid.MaxHeight = Main_window.Height / 2;

            equationsDatagrid.ItemsSource = listInputData;
            this.isEquationsMatrixCreated = true;
        }

        /// <summary> Method that clears equations datagrid </summary>
        private void EquationsDatagridClear()
        {
            try
            {
                this.listInputData.RemoveAll(item => item != null);
                equationsDatagrid.Columns.Clear();
                equationsDatagrid.Items.Clear();
                equationsDatagrid.Items.Refresh();
            }
            catch (Exception)
            { }
            equationsDatagrid.Visibility = Visibility.Hidden;
            try
            {
                resultDatagrid.Columns.Clear();
                resultDatagrid.Items.Clear();
                resultDatagrid.Items.Refresh();
            }
            catch (Exception)
            { }
            resultDatagrid.Visibility = Visibility.Hidden;

        }

        /// <summary> Method that creates DataGrid for result data </summary>
        /// <param name="vectorX"> Result data array </param>
        private void ResultDatagridCreation(double[] vectorX)
        {
            List<List<double>> listVectorX = new List<List<double>>();

            for (int i = 0; i < size; i++)
            {
                listVectorX.Add(new List<double>());
            }

            List<double> temp = listVectorX.First();
            for (int i = 0; i < size; i++)
            {
                temp.Add(vectorX[i]);
            }

            int counter = 0;
            foreach (List<double> sublist in listVectorX)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn
                {
                    Header = "x" + counter,
                    Binding = new Binding("[" + counter + "]"),
                    Width = Main_window.Width / (2 * (size + 1))
                };
                resultDatagrid.Columns.Add(textColumn);
                counter++;
            }

            for (int i = 1; i < size; i++)
                listVectorX.RemoveAt(1);

            resultDatagrid.ItemsSource = listVectorX;


            resultDatagrid.MaxWidth = Main_window.Width / 2;
            resultDatagrid.Visibility = Visibility.Visible;

        }

        /// <summary> Method that clears result datagrid </summary>
        private void ResultDatagridClear()
        {
            try
            {
                resultDatagrid.Columns.Clear();
                resultDatagrid.Items.Clear();
                resultDatagrid.Items.Refresh();
            }
            catch (Exception)
            { }
        }

        /// <summary> Method that converts list with input data to arrays: matrixA and vectorB </summary>
        private void ListToArray()
        {
            int j = 0;
            int i = 0;

            foreach (List<double> sublist in this.listInputData)
            {
                i = 0;
                foreach (double elem in sublist)
                {
                    if (elem == sublist.Last() && i == size)
                    {
                        vectorB[j] = elem;
                    }
                    else
                    {
                        matrixA[j, i] = elem;
                        i++;
                    }
                }
                j++;
            }
        }

        /// <summary> Action performed when createMatrixBtn is clicked. Method creates empty DataGrid after user has written number of parameters </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateMatrixBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetTimeGrid();

            try
            {
                size = Int32.Parse(this.equationSizeTxtBox.Text);
                if (size > 20)
                {
                    MessageBox.Show("Maksymalna ilość równań to 20");
                }
                else if (size < 0)
                {
                    MessageBox.Show("Wartość nie może być ujemna");
                }
                else
                {
                    EquationsDatagridClear();
                    MatrixCreation();
                    EquationsDatagridCreation();

                    equationsDatagrid.Visibility = Visibility.Visible;
                    calculateBtn.Visibility = Visibility.Visible;
                    dllAsmChbx.Visibility = Visibility.Visible;
                    dllCChbx.Visibility = Visibility.Visible;
                    dllCompChbx.Visibility = Visibility.Visible;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Podana wartość jest niepoprawna");
            }
        }

        /// <summary> Action performed when calculateBtn is clicked. Method checks if input data is correct and then calculates set of equations </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateBtn_Click(object sender, RoutedEventArgs e)
        {
                ResetTimeGrid();

                if (!isEquationsMatrixCreated)
                {
                    MessageBox.Show("Wprowadź układ równań");
                    return;
                }

                if (this.mode == Mode.Default)
                {
                    MessageBox.Show("Wybierz dll");
                    return;
                }

                ListToArray();

                LU result = new LU(this.matrixA, this.vectorB, this.size, this.mode, this);
                try
                {
                    result.Calculate();
                    double[] res = result.GetResult();
                    ResultDatagridClear();
                    ResultDatagridCreation(res);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    ResultDatagridClear();
                }  
        }

        /// <summary> Method that controls dllAsmChbx </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DllAsmChbx_Checked(object sender, RoutedEventArgs e)
        {
            dllCChbx.IsChecked = false;
            dllCompChbx.IsChecked = false;
            this.mode = Mode.ASM;
        }

        /// <summary> Method that controls dllCChbx </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DllCChbx_Checked(object sender, RoutedEventArgs e)
        {
            dllAsmChbx.IsChecked = false;
            dllCompChbx.IsChecked = false;
            this.mode = Mode.C;
        }

        /// <summary> Method that controls dllCompChbx </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DllCompChbx_Checked(object sender, RoutedEventArgs e)
        {
            dllAsmChbx.IsChecked = false;
            dllCChbx.IsChecked = false;
            this.mode = Mode.Compare;
        }
        /// <summary> Method used when no checkbox has been chosen </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DllChbx_Unchecked(object sender, RoutedEventArgs e)
        {
            this.mode = Mode.Default;
        }

        /// <summary> Method that measures time and controls time displaying </summary>
        /// <param name="time"></param>
        /// <param name="mode"></param>
        public void SetTime(string time, Mode mode)
        {
            timeGrid.Visibility = Visibility.Visible;
            if (mode == Mode.ASM)
            {
                asmExecutionTimeLbl.Visibility = Visibility.Visible;
                asmTimeLbl.Visibility = Visibility.Visible;
                asmTimeLbl.Content = time;
            }
            if (mode == Mode.C)
            {
                cExecutionTimeLbl.Visibility = Visibility.Visible;
                cTimeLbl.Visibility = Visibility.Visible;
                cTimeLbl.Content = time;
            }
        }

        /// <summary> Method that clears time results </summary>              
        private void ResetTimeGrid()
        {
            timeGrid.Visibility = Visibility.Hidden;
            asmExecutionTimeLbl.Visibility = Visibility.Hidden;
            asmTimeLbl.Visibility = Visibility.Hidden;
            cExecutionTimeLbl.Visibility = Visibility.Hidden;
            cTimeLbl.Visibility = Visibility.Hidden;
        }

        /// <summary> Method used when EquationSizeTxtBox's value has been changed </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EquationSizeTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
