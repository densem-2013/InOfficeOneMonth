using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InOfficeOneMonth.Entities;

namespace OfficeTest
{
    [TestClass]
    public class UnitTest1
    {
        //метод проверяет соответствие объёма выданных директором заданий и фактически отмеченной выполненной работы в Office.Employment
        //при большой загруженности офиса возможны ошибки тестирования
        [TestMethod]
        [STAThread]
        public void Test_GivenJob_vs_JobDone()
        {
            Office office = new Office();
            int JobGiven = 0, JobDone = 0;
                office.CountDown();
                foreach (KeyValuePair<string, Report> pair in office.Employment.Where(emp => !emp.Key.StartsWith("Director")))
                {
                    foreach (Responsibility resp in pair.Value.DischargeDuties)
                    {
                        JobDone += (resp == Responsibility.None) ? 0 : 1;
                    }
                }
                JobGiven = office.GivenTasks.Sum(x => x.Volume * office.Employees.Where(emp => emp.PositionTask.FirstOrDefault() == x.Responsibility).Count());
            Assert.AreEqual(JobGiven, JobDone, "Work issued by {0} man-hours, the work actually carried out on {1} man-hours", JobGiven, JobDone);
        }
    }
}
