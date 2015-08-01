using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.ComponentModel;
using System.Web.UI;
using System.IO;

namespace InOfficeOneMonth.Entities
{
    /// <summary>
    /// Класс содержит массив занятости за весь месяц а также зарплату по неделям
    /// </summary>
    public class Report
    {
        public Responsibility[, ,] DischargeDuties;
        public uint[] WeekSalary;
        public Report()
        {
            DischargeDuties = new Responsibility[4, 5, 8];
            WeekSalary = new uint[5];
        }
    }
    /// <summary>
    /// Основной класс, описывающий работу офиса
    /// </summary>
    public class Office
    {
        //содержит ссылку на обработчик временного события
        public delegate void TimePassedHandler(object sender,TimeEventArgs hpArgs);
        //выполняет операцию в отдельном потоке для работы индикатора течения времени
        public static BackgroundWorker bgworker;
        //список всех нанятых сотрудников
        public List<Employee> Employees { get; set; }
        //список нанятых удалённых сотрудников
        public List<Employee> RemoteEmployees { get; set; }
        //событие наступление нового часа
        public event TimePassedHandler NewHour;
        //событие окончания недели
        public event TimePassedHandler EndWeek;
        //ведомость, хранящая информацию о занятости всех сотрудников по именам,
        //а также о недельной зарплате каждого из них
        public Dictionary<string, Report> Employment;
        Random rand = new Random();
        //содержит список выданных директором задач
        public List<Task> GivenTasks;

        public Office()
        {
            GivenTasks = new List<Task>();
            Employees = new List<Employee>();
            RemoteEmployees = new List<Employee>();
            EndWeek += new TimePassedHandler(Accountant.EndWeekHandler);
            Employment = new Dictionary<string, Report>();
            TakeEmployers();
        }

        //метод моделирует наём в офис произвольного числа сотрудников
        public void TakeEmployers()
        {
            int employersNumber = rand.Next(9, 100);

            int directorsNumber = employersNumber / 40;

            int accountantNumber = employersNumber / 20;

            int managerNumber = accountantNumber + 2;

            int programmerNumber = (employersNumber - accountantNumber - managerNumber - directorsNumber) * rand.Next(36, 45) / 100;

            int disignerNumber = (employersNumber - accountantNumber - managerNumber - directorsNumber) * rand.Next(36, 45) / 100;

            int testerNumber = employersNumber - accountantNumber - managerNumber - programmerNumber - disignerNumber - directorsNumber;

            for (int i = 0; i < employersNumber; i++)
            {
                Employee newEmployee;
                if (i <= directorsNumber)
                {
                    newEmployee = new Director();
                    newEmployee.Name = "Director_" + (i + 1);
                    ((FixedEmployee)newEmployee).FixedRate = 3000;
                    newEmployee.WorkShedule = 40;

                    ((Director)newEmployee).JobReady += GiveJobEmployees;

                }
                else if (i <= directorsNumber + accountantNumber + 1)
                {
                    newEmployee = new Accountant();
                    newEmployee.Name = "Accountant_" + (i - directorsNumber);
                    ((FixedEmployee)newEmployee).FixedRate = 1500;
                    newEmployee.PositionTask.Add((Responsibility)Enum.Parse(typeof(Responsibility), rand.Next(3).ToString()));
                    newEmployee.WorkShedule = 40;

                }
                else if (i <= accountantNumber + managerNumber + directorsNumber + 1)
                {
                    newEmployee = new Manager();
                    newEmployee.Name = "Manager_" + (i - directorsNumber - accountantNumber - 1);
                    ((FixedEmployee)newEmployee).FixedRate = 2000;
                    newEmployee.PositionTask.Add((Responsibility)Enum.Parse(typeof(Responsibility), rand.Next(3).ToString()));
                    newEmployee.WorkShedule = 40;

                }
                else if (i <= accountantNumber + managerNumber + directorsNumber + programmerNumber + 1)
                {
                    newEmployee = new Programmer();
                    newEmployee.Name = "Programmer_" + (i - directorsNumber - accountantNumber - managerNumber - 1);

                }
                else if (i <= accountantNumber + managerNumber + programmerNumber + directorsNumber + disignerNumber + 1)
                {
                    newEmployee = new Designer();
                    newEmployee.Name = "Designer_" + (i - directorsNumber - accountantNumber - managerNumber - programmerNumber - 1);

                }
                else
                {
                    newEmployee = new Tester();
                    newEmployee.Name = "Tester_" + (i - directorsNumber - accountantNumber - managerNumber - programmerNumber - disignerNumber - 1);

                }

                this.NewHour += newEmployee.HourBegin;
                Employees.Add(newEmployee);
                newEmployee.JobProceeding += newEmployee_JobProceeding;
                Employment.Add(newEmployee.Name, new Report());

            }

        }
        //метод заносит данные о занятости сотрудником каждый час
        void newEmployee_JobProceeding(TaskCurrentTimeArgs tctargs)
        {
            Employment[tctargs.EmpName].DischargeDuties[tctargs.CurTime.Week, (int)tctargs.CurTime.Day, (int)tctargs.CurTime.Hour] = tctargs.CurResp;
        }
        //метод раздаёт задачу всем целевым сотрудникам
        void GiveJobEmployees(TaskEventArgs targs)
        {
            IEnumerable<Employee> targetEmployees = TakeFreeResource(targs.Task);

            foreach (Employee emp in targetEmployees)
            {
                emp.BusyTime += targs.Task.Volume;
                emp.CurrentTasks.Enqueue(targs.Task);
            }

        }
        //отчёт времени
        public void CountDown()
        {
            int progress = 1;
            for (int week = 0; week < 4; week++)
            {
                for (int day = 0; day < 5; day++)
                {
                    for (int hour = 0; hour < 8; hour++)
                    {
                        NewHour(this, new TimeEventArgs(week, day, hour));
                        Thread.CurrentThread.Join(20);
                        if (bgworker != null)
                        {
                            bgworker.ReportProgress(progress++);
                        }
                    }

                }

                EndWeek(this, new TimeEventArgs(week, null, null));
            }
        }

        //метод ищет свободные ресурсы, необходимые для выполнения задания директора и возвращает в виде списка сотрудников
        public List<Employee> TakeFreeResource(Task task)
        {
            Position pos;
            Enum.TryParse<Position>(Enum.GetName(typeof(Position), task.Responsibility), out pos);

            //количество сотрудников, необходимое для выполнения задания
            //определяется по тому количеству основных сотрудников, у кого первая обязанность в списке
            //соответствует целевой обязанности в выданном задании
            int taskCount = Employees.Where(emp => emp.PositionTask.FirstOrDefault() == task.Responsibility).Count(); 

            List<Employee> lst = Employees
                .Where(emp => emp.PositionTask.FirstOrDefault() == task.Responsibility && !emp.MainBusy).ToList();
            int lstCount = lst.Count();
            if (lstCount < taskCount)
            {
                List<Employee> lst_1 = Employees.Where(emp => emp.PositionTask.Contains(task.Responsibility) && (!emp.PartBusy))
                                            .OrderBy(emp => emp.Rate).Take(taskCount).ToList<Employee>();
                lst.InsertRange(lst.Count, lst_1);
            }

            lstCount = lst.Count();
            if (lstCount < taskCount)
            {
                if (RemoteEmployees.Count > 0)
                {
                    List<Employee> remEmployees = RemoteEmployees
                        .Where<Employee>(emp => !emp.MainBusy && emp.PositionTask.FirstOrDefault() == task.Responsibility)
                        .Take(taskCount - lstCount).ToList<Employee>();
                    lst.InsertRange(lst.Count, remEmployees);
                }
                lstCount = lst.Count();
                if (lstCount < taskCount)
                {
                    
                    List<Employee> frls = new List<Employee>();
                    for (int i = lstCount; i <= taskCount; i++)
                    {
                        Employee newFreelance = new RemoteEmployee();
                        newFreelance.Name = "RemoteEmployees_" + (RemoteEmployees.Count + 1);
                        newFreelance.PositionTask.Add(task.Responsibility);
                        this.NewHour += newFreelance.HourBegin;
                        newFreelance.JobProceeding += newEmployee_JobProceeding;
                        RemoteEmployees.Add(newFreelance);
                        frls.Add(newFreelance);
                        Employment.Add(newFreelance.Name, new Report());
                    }
                    lst.InsertRange(lst.Count, frls);
                }
            }

            return lst;
        }

        //Метод вызывает создание отчётного документа
        public void CreateReport()
        {
            PrintHelper hlp = new PrintHelper(this);
            hlp.CreateDataScript();
            hlp.CreateHtml();
        }
    }

    //инкапсулирует аргументы для делегата
    public class TimeEventArgs : EventArgs
    {
        public readonly int Week;
        public readonly int? Day;
        public readonly int? Hour;
        public TimeEventArgs(int week, int? day, int? hour)
        {
            this.Week = week;
            this.Day = day;
            this.Hour = hour;
        }
    }
}
