using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JAProject
{
    /// <summary>
    /// Enum type to set application mode
    /// </summary>
    public enum Mode { C, ASM, Compare, Default };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Field for containing size of the matrix
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
        /// List containing input data - matrix A annd vector B
        /// </summary>
        private List<List<double>> listInputData;
        /// <summary>
        /// Field for containig mode of application
        /// </summary>
        Mode mode;
        /// <summary>
        /// Bool to define if equations matrix is created or not
        /// </summary>
        bool isEquationsMatrixCreated;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            mode = Mode.Default;
            isEquationsMatrixCreated = false;
            this.MinHeight = 600;
            this.MinWidth = 900;
        }
        /// <summary>
        /// Method that creates matrix and vector for input data
        /// </summary>
        private void matrixCreation()
        {
            matrixA = new double[size, size];
            vectorB = new double[size];
        }
        /// <summary>
        /// Method that creates DataGrid for equations
        /// </summary>
        private void equationsDatagridCreation()
        {
            this.listInputData = new List<List<double>>();

            for (int i = 0; i < size; i++)
            {
                this.listInputData.Add(new List<double>());
            }

            int counter = 0;
            foreach (List<double> sublist in this.listInputData)
            {
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = "x" + counter;
                textColumn.Binding = new Binding("[" + counter + "]");
                textColumn.Width = Main_window.Width / (2 * (size + 1));
                equationsDatagrid.Columns.Add(textColumn);
                counter++;
            }
            DataGridTextColumn resultColumn = new DataGridTextColumn();
            resultColumn.Header = "y";
            resultColumn.Binding = new Binding("[" + counter + "]");
            resultColumn.Width = Main_window.Width / (2 * (size + 1));
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

        /// <summary>
        /// Method that clears equations datagrid
        /// </summary>
        private void equationsDatagridClear()
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
        /// <summary>
        /// Method that creates DataGrid for result data
        /// </summary>
        /// <param name="vectorX">Result data array(double)</param>
        private void resultDatagridCreation(double[] vectorX)
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
                DataGridTextColumn textColumn = new DataGridTextColumn();
                textColumn.Header = "x" + counter;
                textColumn.Binding = new Binding("[" + counter + "]");
                textColumn.Width = Main_window.Width / (2 * (size + 1));
                resultDatagrid.Columns.Add(textColumn);
                counter++;
            }

            for (int i = 1; i < size; i++)
                listVectorX.RemoveAt(1);

            resultDatagrid.ItemsSource = listVectorX;


            resultDatagrid.MaxWidth = Main_window.Width / 2;
            resultDatagrid.Visibility = Visibility.Visible;

        }
        /// <summary>
        /// Method that clears result datagrid
        /// </summary>
        private void resultDatagridClear()
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
        /// <summary>
        /// Method that converts list with input data to arrays
        /// </summary>
        private void listToArray()
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
        /// <summary>
        /// Action performed when createMatrixBtn is clicked
        /// Method creates empty datagrid after user have choosed number of parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createMatrixBtn_Click(object sender, RoutedEventArgs e)
        {
            resetTimeGrid();

            try
            {
                size = Int32.Parse(this.equationSizeTxtBox.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Podana wartość jest niepoprawna");
            }
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
                equationsDatagridClear();
                matrixCreation();
                equationsDatagridCreation();

                equationsDatagrid.Visibility = Visibility.Visible;
                calculateBtn.Visibility = Visibility.Visible;
                numberThreadsLbl.Visibility = Visibility.Visible;
                numberThreadsTxtBox.Visibility = Visibility.Visible;
                dllAsmChbx.Visibility = Visibility.Visible;
                dllCChbx.Visibility = Visibility.Visible;
                dllCompChbx.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// Action performed when calculateBtn is clicked
        /// Method cheks if input data is correct and then calculates set of equations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calculateBtn_Click(object sender, RoutedEventArgs e)
        {
            resetTimeGrid();

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

            listToArray();

            LU result = new LU(this.matrixA, this.vectorB, this.size, this.mode, this);
            try
            {
                result.calculate();
                double[] res = result.getResult();
                resultDatagridClear();
                resultDatagridCreation(res);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                resultDatagridClear();
            }
        }
        /// <summary>
        /// Method that controls dllAsmChbx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dllAsmChbx_Checked(object sender, RoutedEventArgs e)
        {
            dllCChbx.IsChecked = false;
            dllCompChbx.IsChecked = false;
            this.mode = Mode.ASM;
        }
        /// <summary>
        /// Method that controls dllCChbx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dllCChbx_Checked(object sender, RoutedEventArgs e)
        {
            dllAsmChbx.IsChecked = false;
            dllCompChbx.IsChecked = false;
            this.mode = Mode.C;
        }
        /// <summary>
        /// Method that controls dllCompChbx
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dllCompChbx_Checked(object sender, RoutedEventArgs e)
        {
            dllAsmChbx.IsChecked = false;
            dllCChbx.IsChecked = false;
            this.mode = Mode.Compare;
        }
        private void dllChbx_Unchecked(object sender, RoutedEventArgs e)
        {
            this.mode = Mode.Default;
        }
        /// <summary>
        /// Method that measures time and controls time displaying
        /// </summary>
        /// <param name="time"></param>
        /// <param name="mode"></param>
        public void setTime(string time, Mode mode)
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
        /// <summary>
        /// Method that celars time results
        /// </summary>              
        private void resetTimeGrid()
        {
            timeGrid.Visibility = Visibility.Hidden;

            asmExecutionTimeLbl.Visibility = Visibility.Hidden;
            asmTimeLbl.Visibility = Visibility.Hidden;

            cExecutionTimeLbl.Visibility = Visibility.Hidden;
            cTimeLbl.Visibility = Visibility.Hidden;
        }

        private void NumberThreadsTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void EquationSizeTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
