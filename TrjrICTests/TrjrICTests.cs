using ItemChangerTesting;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrjrICTests
{
    public class TrjrICTests : Mod
    {
        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public override int LoadPriority() => 99999; // force load after TRJR has hooked IC

        public override void Initialize()
        {
            Log("Initializing");

            ItemChangerTestingMenu.TestInjectors += InjectTests;

            Log("Initialized");
        }

        private IEnumerable<Test> InjectTests()
        {
            IEnumerable<Type> types = typeof(TrjrICTests).Assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Test)));
            foreach (Type t in types)
            {
                if (t.GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<object>()) is Test test)
                {
                    yield return test;
                }
            }
        }
    }
}
