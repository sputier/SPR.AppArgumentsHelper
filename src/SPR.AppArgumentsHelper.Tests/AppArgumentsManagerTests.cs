using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPR.AppArgumentsHelper.Tests
{
    [TestClass]
    public class AppArgumentsManagerTests
    {
        [TestMethod]
        public void Test_LoadArgs()
        {
            var commandLineArgs = new[] {
                "--stringSwitch",
                "my switch1 value",
                "--intSwitch",
                "42",
                "--booleanSwitch",
                "--arraySwitch",
                "arrayItem1",
                "--arraySwitch",
                "arrayItem2",
                "--arraySwitch",
                "arrayItem3",
            };

            var expectedArgs = new AppArguments
            {
                StringSwitch = "my switch1 value",
                IntSwitch = 42,
                BooleanSwitch = true,
                ArraySwitch = new[] { "arrayItem1", "arrayItem2", "arrayItem3" }
            };

            var mgr = new AppArgumentsManager<AppArguments>();
            var result = mgr.LoadArgs(commandLineArgs);

            Assert.AreEqual(expectedArgs.StringSwitch, result.StringSwitch);
            Assert.AreEqual(expectedArgs.IntSwitch, result.IntSwitch);
            Assert.AreEqual(expectedArgs.BooleanSwitch, result.BooleanSwitch);
            Assert.IsNotNull(result.ArraySwitch);
            Assert.AreEqual(expectedArgs.ArraySwitch.Length, result.ArraySwitch.Length);

            for (int i = 0; i < expectedArgs.ArraySwitch.Length; i++)
                Assert.AreEqual(expectedArgs.ArraySwitch[i], result.ArraySwitch[i]);
        }

        [TestMethod]
        public void Test_LoadArgs_Without_Required_Switch()
        {
            var commandLineArgs = new[] {
                "--stringSwitch",
                "my switch1 value",
                "--stringSwitch",
                "my switch2 value",
            };

            var mgr = new AppArgumentsManager<AppArguments>();

            Assert.ThrowsException<ArgumentException>(() => mgr.LoadArgs(commandLineArgs));
        }

        [TestMethod]
        public void Test_LoadArgs_With_Multiple_Single_Switch()
        {
            var commandLineArgs = new[] {
                "--intSwitch",
                "42"
            };

            var mgr = new AppArgumentsManager<AppArguments>();

            Assert.ThrowsException<ArgumentException>(() => mgr.LoadArgs(commandLineArgs));
        }
    }

    internal class AppArguments
    {
        [ArgumentSwitch("stringSwitch", ArgumentMode.Required)]
        public string StringSwitch { get; set; }

        [ArgumentSwitch("intSwitch", ArgumentMode.Optional)]
        public int IntSwitch { get; set; }

        [ArgumentSwitch("arraySwitch", ArgumentMode.Optional)]
        public string[] ArraySwitch { get; set; }

        [ArgumentSwitch("booleanSwitch", ArgumentMode.Optional)]
        public bool BooleanSwitch { get; set; }
    }
}
