using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inteligenca_rojev;
using WindowsFormsControlLibrary1;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var stavba = UserControl1.GenerateRandomMap(20);
            var data = PSO.generateRandomData(stavba, 5).ToArray();

            var first = Oddajnik.ToVector(data);
            var second = Oddajnik.ToVector(Oddajnik.FromVector(Oddajnik.ToVector(data)));

            CollectionAssert.AreEqual(first, second);
        }

        [TestMethod]
        public void PSOSpeedTests()
        {
            var stavba = UserControl1.GenerateRandomMap(50);
            var data = PSO.generateRandomData(stavba, 25).ToArray();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //PSO p = new PSO(50, stavba, 10, 10);
            //p.Run(10, (x) => {}, () => {}, ;

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Assert.AreEqual(true, stopwatch.ElapsedMilliseconds < 1000);
        }

        [TestMethod]
        public void PSOFitness()
        {
            var stavba = UserControl1.GenerateRandomMap(100, 0.5);
            var data = PSO.generateRandomData(stavba, 1, 100).ToArray();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Fitness.CalculateFitness(stavba, data);

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Assert.AreEqual(true, stopwatch.ElapsedMilliseconds < 10);
        }


        [TestMethod]
        public void AllWalsShouldHavePowerZero()
        {
            var stavba = UserControl1.GenerateRandomMap(20);
            var data = PSO.generateRandomData(stavba, 5).ToArray();

            var map = Fitness.generatePowerMap(stavba);
            Fitness.CalculatePower(stavba, data, map);

            for (var i = 0; i < stavba.Rows; i++)
            {
                for (var y = 0; y < stavba.Cols; y++)
                {
                    if (stavba.lokacija[i][y] == Lokacija.Zid)
                    {
                        Assert.AreEqual(0, map[i][y]);
                    }
                }
            }
        }
    }
}
