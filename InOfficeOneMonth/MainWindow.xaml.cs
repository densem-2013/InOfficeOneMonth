using System;
using System.Linq;
using System.Windows;
using InOfficeOneMonth.Entities;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Text;

namespace InOfficeOneMonth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Office office;
        public MainWindow()
        {
            InitializeComponent();
            Office.bgworker = new BackgroundWorker();
            Office.bgworker.WorkerReportsProgress = true;
            Office.bgworker.DoWork += new DoWorkEventHandler(bgworker_DoWork);
            Office.bgworker.ProgressChanged += new ProgressChangedEventHandler(bgworker_ProgressChanged);
            Office.bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgworker_RunWorkerCompleted);
        }

        void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int dirs = office.Employees.Where(emp => emp is Director).Count();
            int accs = office.Employees.Where(emp => emp is Accountant).Count();
            int mans = office.Employees.Where(emp => emp is Manager).Count();
            int progss = office.Employees.Where(emp => emp is Programmer).Count();
            int disns = office.Employees.Where(emp => emp is Designer).Count();
            int tests = office.Employees.Where(emp => emp is Tester).Count();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Quantity Employees equals:");
            sb.AppendLine(String.Format("\tDirectors\t\t{0}", dirs));
            sb.AppendLine(String.Format("\tAccountants\t{0}", accs));
            sb.AppendLine(String.Format("\tManagers\t{0}", mans));
            sb.AppendLine(String.Format("\tProgrammers\t{0}", progss));
            sb.AppendLine(String.Format("\tDesigners\t{0}", disns));
            sb.AppendLine(String.Format("\tTesters\t\t{0}", tests));
            sb.AppendLine(String.Format("\tRemotes\t\t{0}", office.RemoteEmployees.Count));
            sb.AppendLine(String.Format("\n\tTotal\t\t{0}", office.Employees.Count + office.RemoteEmployees.Count));

            int JobGiven = office.GivenTasks.Sum(x => x.Volume * office.Employees.Where(emp => emp.PositionTask.FirstOrDefault() == x.Responsibility).Count());

            sb.AppendFormat("\n\n\n\t\t\tTotal tasks given by {0} man-hours\n", JobGiven);

            textBlock1.Text = sb.ToString();
            button2.Visibility = Visibility.Visible;
        }

        void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            textBlock2.Text = String.Format("Processing  {0,-6} {1} %", new string('.', e.ProgressPercentage/14 % 4), e.ProgressPercentage * 100 / 160);
        }

        void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            office = new Office();
            office.CountDown();
            office.CreateReport();
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            button2.Visibility = Visibility.Hidden;
            Office.bgworker.RunWorkerAsync();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((ThreadStart)(() =>
            {
                this.progressBar1.Value = 0;
                textBlock1.Text = String.Format("{0}{1}{2}",new string('\n',20),new string(' ',20),"Processing...");
            }), DispatcherPriority.Normal);
                onLoaded(sender, e);

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("Output\\Employment.html");
            //Environment.Exit(0);
        }


    }
}
