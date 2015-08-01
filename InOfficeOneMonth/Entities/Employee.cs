using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace InOfficeOneMonth.Entities
{
    /// <summary>
    /// Перечисление отображает список обязанностей, которые могут быть у работника офиса
    /// </summary>
    public enum Responsibility
    {
        None = 0,
        WriteCode,
        DrawLayout,
        TestProgram,
        SellServices,
        CreateReports,
        GivenTask
    }
    /// <summary>
    /// Перечисление отражает должности, которые могут занимать работники офиса
    /// </summary>
    public enum Position
    {
        None = 0,
        Programmer,
        Designer,
        Tester,
        Manager,
        Accountant,
        Director
    }
    /// <summary>
    /// Класс описывает выданную директором задачу
    /// </summary>
    public class Task
    {
        //вид обязанностей
        public Responsibility Responsibility;
        //количество необходимых на выполнение часов
        public int Volume;
        Random rand = new Random();
        public Task()
        {
            Responsibility = (Responsibility)Enum.Parse(typeof(Responsibility), rand.Next(1, 6).ToString());
            Volume = rand.Next(1, 3);
        }
    }

    /// <summary>
    /// класс инкапсулирует передаваемую задачу
    /// </summary>
    class TaskEventArgs : EventArgs
    {
        public readonly Task Task;
        public TaskEventArgs(Task task)
        {
            this.Task = task;
        }
    }
    /// <summary>
    /// класс применяется для отправки в оффис информации о занятости в текущем часу
    /// </summary>
    public class TaskCurrentTimeArgs : EventArgs
    {
        //имя сотрудника
        public string EmpName { get; set; }
        //выполняемые в данном часу обязанности
        public Responsibility CurResp { get; set; }
        //текущая дата
        public TimeEventArgs CurTime { get; set; }
    }

    //вспомогательный делегат для подписки на публичное событие для сохранения принципа инкапсуляции
    public delegate void TaskCurHandler(TaskCurrentTimeArgs tctargs);

    /// <summary>
    /// абстрактный класс, описывающий служащего
    /// </summary>
    public abstract class Employee
    {
        //Имя сотрудника
        public string Name { get; set; }
        //свойство отражает загруженность сотрудника на данный момент
        //true, если объём взятых заданий больше недельной занятости
        public bool MainBusy
        {
            get
            {
                return (CurrentTasks.Sum(x => x.Volume) < WorkShedule) ? false : true;
            }
        }
        //свойство отражает загруженность своими непосредственными обязанностями, находящимися в 
        //списке обязанностей, которые может выполнять сотрудник, на первом или втором месте
        public virtual bool PartBusy
        {
            get
            {
                return (!MainBusy && (CurrentTasks.Where(x => PositionTask.IndexOf(x.Responsibility) < 2)
                    .Sum(x => x.Volume) < WorkShedule / 3)) ? false : true;
            }
        }
        //очередь взятых сотрудником заданий
        public Queue<Task> CurrentTasks { get; set; }
        //Текущее выполняемое задание
        public Responsibility CurrentJob { get; set; }
        //время, оставщееся на выполнение текущей задачи
        public int CurrentTaskTime { get; set; }
        //время, оставшееся на выполнение всех взятых заданий
        public int BusyTime { get; set; }
        //событие, генерируемое сотрудником кождый час при выполнении задания
        public event TaskCurHandler JobProceeding;
        //вспомогательный метод для сохранения инкапсуляции
        public void OnJobProceedingMethod(TaskCurrentTimeArgs tctargs)
        {
            OnJobProceeding(tctargs);
        }
        //вспомогательный метод для сохранения инкапсуляции
        protected void OnJobProceeding(TaskCurrentTimeArgs tctargs)
        {
            TaskCurHandler handler = JobProceeding;
            if (handler != null)
            {
                handler(tctargs);
            }
        }
        //список обязанностей, которые может выполнять сотрудник
        public List<Responsibility> PositionTask { get; set; }
        //рабочий график сотрудника
        public uint WorkShedule { get; set; }
        //почасовая ставка сотрудника
        public uint Rate { get; set; }
        protected Random rand = new Random();
        public Employee()
        {
            PositionTask = new List<Responsibility>();
            CurrentTasks = new Queue<Task>();
            CurrentTaskTime = 0;
            BusyTime = 0;
        }
        //метод, обрабатывающий каждый час работы сотрудника
        //отправляет данные в Office
        public virtual void HourBegin(object sender, TimeEventArgs targs)
        {
            if (CurrentTasks.Count > 0 || CurrentTaskTime > 0) //если список текущих заданий не пуст, или текущее задание не завершено
            {
                if (CurrentTasks.Count > 0 && CurrentTaskTime == 0)
                {
                    Task nextTask = CurrentTasks.Dequeue();
                    CurrentTaskTime = nextTask.Volume;
                    CurrentJob = nextTask.Responsibility;
                }
                if (CurrentTaskTime > 0)
                {
                    CurrentTaskTime--;
                }
                if (BusyTime > 0)
                {
                    BusyTime--;
                }
            }
            else
            {
                CurrentJob = Responsibility.None;
            }
            OnJobProceeding(new TaskCurrentTimeArgs { EmpName = Name, CurResp = CurrentJob, CurTime = targs });
        }
    }
    /// <summary>
    /// класс описывает сотрудника с почасовой оплатой труда
    /// </summary>
    abstract class HourlyEmployee : Employee
    {
        public HourlyEmployee(Type type)
            : base()
        {
            WorkShedule = (uint)rand.Next(20, 40);  //рабочий график не более 40 часов в неделю
            if (type != typeof(RemoteEmployee))
            {
                int respNumber = rand.Next(3);      //количество обязанностей произвольно от 1 до 3
                int index = (int)Enum.Parse(typeof(Position), type.Name);
                int index_1 = (index + 1) % 3 + 1;
                int index_2 = (index + 2) % 3 + 1;

                Responsibility resp_1, resp_2, resp_3;

                Enum.TryParse<Responsibility>(index.ToString(), out resp_1);
                Enum.TryParse<Responsibility>(index_1.ToString(), out resp_2);
                Enum.TryParse<Responsibility>(index_2.ToString(), out resp_3);
                for (int j = 0; j < respNumber; j++)
                {
                    int val = rand.Next(1, 2);
                    switch (j)
                    {
                        case 0: PositionTask.Add(resp_1); break;
                        case 1: if (val == 1) PositionTask.Add(resp_2);
                            else PositionTask.Add(resp_3);
                            break;
                        case 2: if (val == 2) PositionTask.Add(resp_2);
                            else PositionTask.Add(resp_3);
                            break;
                    }

                }
                Rate = (uint)(5 * (respNumber + 1));    //почасовая ставка зависит от количества обязанностей, возможных к выполнению
            }
            else { Rate = 15; } //для удалённых сотрудников 
        }

    }
    /// <summary>
    /// класс, описывающий сотрудника, работающего по фиксированной ставке
    /// </summary>
    abstract class FixedEmployee : Employee
    {
        //Фиксированная месячная ставка
        public uint FixedRate { get; set; }

        public FixedEmployee(Type type)
            : base()
        {
            Rate = 15; //почасовая ставка, если сотрудник возьмёт дополнительно задания с фиксированной оплатой
            Responsibility resp, resp_2;
            int index = (int)Enum.Parse(typeof(Position), type.Name);
            Enum.TryParse<Responsibility>(index.ToString(), out resp);
            PositionTask.Add(resp);
            if (resp != Responsibility.GivenTask)
            {
                resp_2 = (resp == Responsibility.CreateReports) ? Responsibility.SellServices : Responsibility.CreateReports;
                PositionTask.Add(resp_2);
            }
        }
    }

    class Programmer : HourlyEmployee
    {
        public Programmer() : base(typeof(Programmer)) { }
    }

    class Designer : HourlyEmployee
    {
        public Designer() : base(typeof(Designer)) { }
    }

    class Tester : HourlyEmployee
    {
        public Tester() : base(typeof(Tester)) { }
    }

    class Accountant : FixedEmployee
    {
        public Accountant() : base(typeof(Accountant)) { }
        //каждую неделю бухгалтер расчитывает зарплату для всех сотрудников и заносит
        //её значение в ведомость Employment
        public static void EndWeekHandler(object sender, TimeEventArgs targs)
        {
            Dictionary<string, Report> Employment = ((Office)sender).Employment;
            int numWeek = targs.Week;
            uint salary;
            foreach (Employee emp in ((Office)sender).Employees.Where(x => x is HourlyEmployee))
            {
                salary = 0;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        salary += (Employment[emp.Name].DischargeDuties[numWeek, i, j] != Responsibility.None) ? emp.Rate : 0;
                    }
                }
                Employment[emp.Name].WeekSalary[numWeek] = salary;
            }
            foreach (Employee emp in ((Office)sender).Employees.Where(x => x is FixedEmployee))
            {
                salary = 0;
                if (!(emp is Director))
                {

                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            salary += (Employment[emp.Name].DischargeDuties[numWeek, i, j] != Responsibility.None &&
                                Employment[emp.Name].DischargeDuties[numWeek, i, j] != emp.PositionTask.FirstOrDefault()) ? emp.Rate : 0;
                        }
                    }
                }
                salary += (emp as FixedEmployee).FixedRate / 4;
                Employment[emp.Name].WeekSalary[numWeek] = salary;
            }
            foreach (Employee emp in ((Office)sender).RemoteEmployees)
            {
                salary = 0;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        salary += (Employment[emp.Name].DischargeDuties[numWeek, i, j] != Responsibility.None) ? emp.Rate : 0;
                    }
                }
                Employment[emp.Name].WeekSalary[numWeek] = salary;
            }
            if (numWeek == 3)
            {
                foreach (KeyValuePair<string, Report> pair in ((Office)sender).Employment)
                {
                    pair.Value.WeekSalary[4] = (uint)pair.Value.WeekSalary.Take(4).Sum(x => x);
                }
            }
        }
    }

    class Manager : FixedEmployee
    {
        public Manager() : base(typeof(Manager)) { }
    }

    class RemoteEmployee : HourlyEmployee
    {
        public RemoteEmployee()
            : base(typeof(RemoteEmployee))
        {

        }
    }
    class Director : FixedEmployee
    {
        public Director() : base(typeof(Director)) { }
        //делегат для передачи обработки события в Office
        public delegate void TaskHandler(TaskEventArgs targs);
        //событие, которое генерирует директор при очередного задания
        public event TaskHandler JobReady;
        public override void HourBegin(object sender, TimeEventArgs targs)
        {
            if (!(targs.Week == 3 && targs.Day == 4 && targs.Hour > 5))
            {
                OnJobProceeding(new TaskCurrentTimeArgs { EmpName = Name, CurResp = Responsibility.GivenTask, CurTime = targs });
                int taskNumber = rand.Next(3);
                for (int i = 0; i < taskNumber; i++)
                {
                    Task task = new Task();
                    JobReady(new TaskEventArgs(task));
                    //Запись выданного задания в офисный список выданных заданий
                    ((Office)sender).GivenTasks.Add(task);
                }
            }
        }
    }
}
