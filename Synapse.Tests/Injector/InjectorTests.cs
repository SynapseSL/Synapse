using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NUnit.Framework;
using Synapse.Injector;
using System;
using System.IO;
using System.Linq;

namespace Synapse.Tests.Injector
{
    public class InjectorTests
    {
        private const string InjectionTargetName = "InjectionTarget.dll";
        private ModuleDefMD _cleanDataAssembly;
        private ModuleDefMD _injectedDataAssembly;

        [SetUp]
        public void Setup()
        {
            // Replace with TestContext.CurrentContext.TestDirectory at some point
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exampleAssemblyDir = Path.Combine(baseDir, "Data", InjectionTargetName);

            var injector = new SynapseInjector(false);
            _injectedDataAssembly = ModuleDefMD.Load(@exampleAssemblyDir);
            _cleanDataAssembly = ModuleDefMD.Load(exampleAssemblyDir);
            injector.Start(_injectedDataAssembly);
        }

        [Test]
        public void AllMember_Publicized()
        {
            foreach (var type in _injectedDataAssembly.Types)
            {
                var events = type.Events.Select(_ => _.Name).ToList();
                // Fields
                foreach (var field in type.Fields)
                {
                    var isEventBackingField = events.Any(_ => String.Equals(_, field.Name, StringComparison.InvariantCultureIgnoreCase));
                    // wenn kein Event backing-field
                    if (!isEventBackingField)
                    {
                        Assert.IsTrue(field.IsPublic);
                    }
                }
                // Methods
                foreach (var method in type.Methods)
                {
                    Assert.IsTrue(method.IsPublic);
                }
                // Properties, getters & setters
                foreach (var method in type.Properties)
                {
                    if (method.SetMethod is not null)
                        Assert.IsTrue(method.SetMethod.IsPublic);
                    if (method.GetMethod is not null)
                        Assert.IsTrue(method.GetMethod.IsPublic);
                }
            }
        }
        [Test]
        public void Types_StayAbstract()
        {
            Assert.That(
                _injectedDataAssembly.Types.Where(_ => _.IsAbstract).Select(_ => _.FullName),
                Is.EquivalentTo(_cleanDataAssembly.Types.Where(_ => _.IsAbstract).Select(_ => _.FullName))
                );
        }
        [Test]
        public void Types_StayInterface()
        {
            Assert.That(
                _injectedDataAssembly.Types.Where(_ => _.IsInterface).Select(_ => _.FullName),
                Is.EquivalentTo(_cleanDataAssembly.Types.Where(_ => _.IsInterface).Select(_ => _.FullName))
                );
        }
        [Test]
        public void MethodCall_IsInjected()
        {
            var networkManagerType = _injectedDataAssembly.Types.First(_ => _.Name == "CustomNetworkManager");
            var method = networkManagerType.FindMethod("CreateMatch");
            var firstInstruction = method.Body.Instructions[0];

            Assert.AreEqual(OpCodes.Call, firstInstruction.OpCode);
            Assert.NotNull(firstInstruction.Operand);
            Assert.AreEqual(firstInstruction.Operand.GetType().Name, "MethodDefMD");
            Assert.AreEqual((firstInstruction.Operand as dynamic).DeclaringType.FullName, "Synapse.Injector.Loader");
        }
        [Test]
        public void LoaderType_IsInjected() => Assert.That(_injectedDataAssembly.Types.Any(_ => _.FullName == "Synapse.Injector.Loader"));
        [Test]
        public void TypesCount_AsExpected() => Assert.AreEqual(_cleanDataAssembly.Types.Count, _injectedDataAssembly.Types.Count - 1);
    }
}