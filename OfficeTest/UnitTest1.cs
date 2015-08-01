using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InOfficeOneMonth.Entities;
using System.IO;

namespace OfficeTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestWriteHtml()
        {
            Office office = new Office();
            office.CountDown();
            int JobDone = 0;
            foreach (KeyValuePair<string, Report> pair in office.Employment.Where(emp => !emp.Key.StartsWith("Director")))
            {
                foreach (Responsibility resp in pair.Value.DischargeDuties)
                {
                    JobDone += (resp == Responsibility.None) ? 0 : 1;
                }
            }
            int JobGiven = office.GivenTasks.Sum(x => x.Volume*office.Employees.Where(emp => emp.PositionTask.FirstOrDefault() == x.Responsibility).Count());
            Assert.AreEqual(JobGiven, JobDone, "Work issued by {0} man-hours, the work actually carried out on {1} man-hours", JobGiven, JobDone);
        }
    }
}
