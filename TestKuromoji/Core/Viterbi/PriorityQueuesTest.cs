using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLPJDict.Kuromoji.Core.Compile;
using NLPJDict.Kuromoji.Core.IO;
using NLPJDict.Kuromoji.Core.Dict;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLPJDict.Kuromoji.Core;
using NLPJDict.Kuromoji.Core.HelperClasses;
using NLPJDict.Kuromoji.Core.Buffer;
using NLPJDict.Kuromoji.Core.FST;
using NLPJDict.Kuromoji.Core.util;
using NLPJDict.Kuromoji.Core.Viterbi;

namespace NLPJDictTest.kuromoji.Core.Viterbi
{
    [TestClass]
    public class PriorityQueuesTest
    {
        [TestMethod]
        public void TestPriorityQueue()
        {
            PriorityQueue<Employee> pq = new PriorityQueue<Employee>();

            Employee e1 = new Employee("Aiden", 1.0);
            Employee e2 = new Employee("Baker", 2.0);
            Employee e3 = new Employee("Chung", 3.0);
            Employee e4 = new Employee("Dunne", 4.0);
            Employee e5 = new Employee("Eason", 5.0);
            Employee e6 = new Employee("Flynn", 6.0);

            Employee e = pq.Poll();
            e = pq.Poll();

            RunTestPriorityQueue(50000);
        } 

        public void RunTestPriorityQueue(int numOperations)
        {
            Random rand = new Random(0);
            PriorityQueue<Employee> pq = new PriorityQueue<Employee>();
            for (int op = 0; op < numOperations; ++op)
            {
                int opType = rand.Next(0, 2);

                if (opType == 0) // enqueue
                {
                    string lastName = op + "man";
                    double priority = (100.0 - 1.0) * rand.NextDouble() + 1.0;
                    pq.Add(new Employee(lastName, priority));
                    if (!pq.IsConsistent())
                    {
                        Assert.Fail();
                    }
                }
                else
                {
                    if (pq.Count > 0)
                    {
                        Employee e = pq.Poll();
                        if (!pq.IsConsistent())
                        {
                            Assert.Fail();
                        }
                    }
                }
            }

        }
    }

    public class Employee : IComparable<Employee>
    {
        public string lastName;
        public double priority; // smaller values are higher priority

        public Employee(string lastName, double priority)
        {
            this.lastName = lastName;
            this.priority = priority;
        }

        public override string ToString()
        {
            return "(" + lastName + ", " + priority.ToString("F1") + ")";
        }

        public int CompareTo(Employee other)
        {
            if (this.priority < other.priority) return -1;
            else if (this.priority > other.priority) return 1;
            else return 0;
        }
    }
}
