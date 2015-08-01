using System;
using System.Linq;
using System.Windows;
using InOfficeOneMonth.Entities;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

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
            int wrong = office.Employment.Where(pair => pair.Key.StartsWith("Rem") && (pair.Value.DischargeDuties.OfType<Responsibility>()
                .Where(resp => resp == Responsibility.CreateReports || resp==Responsibility.SellServices).Count() > 0)).Count();
            string str = "Quantity Employees equals:\n\tDirectors\t\t{0}\n\tAccountants\t{1}\n\tManagers\t{2}\n\t"+
                            "Programmers\t{3}\n\tDesigners\t{4}\n\tTesters\t\t{5}\n\tRemotes\t\t{6}\n\n\tTotal\t\t{7}\n\n\tWrong\t\t{8}";
            textBlock1.Text = string.Format(str, dirs, accs, mans, progss, disns, tests, office.RemoteEmployees.Count, office.TotalEmployees + office.RemoteEmployees.Count, wrong);
            button2.Visibility = Visibility.Visible;
        }

        void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            textBlock2.Text = String.Format("Выполнение  {0,-6} {1} %", new string('.', e.ProgressPercentage/14 % 4), e.ProgressPercentage * 100 / 160);
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
            Environment.Exit(0);
        }


    }
}
