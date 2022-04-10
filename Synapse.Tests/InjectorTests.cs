using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NUnit.Framework;
using Synapse.Injector;
using System;
using System.IO;
using System.Linq;

namespace Synapse.Tests
{
    public class InjectorTests
    {
        private ModuleDefMD _cleanDataAssembly;
        private ModuleDefMD _injectedDataAssembly;

        [SetUp]
        public void Setup()
        {
            var exampleAssemblyName = "ExampleAssembly.dll";
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var exampleAssemblyDir = Path.Combine(baseDir, "Data", exampleAssemblyName);

            var injector = new SynapseInjector(false);
            _injectedDataAssembly = ModuleDefMD.Load(@exampleAssemblyDir);
            _cleanDataAssembly = ModuleDefMD.Load(exampleAssemblyDir);
            injector.Start(_injectedDataAssembly);
        }

        [Test]
        public void Publicizes_EveryMember()
        {
            foreach (var type in _injectedDataAssembly.Types)
            {
                var events = type.Events.Select(_ => _.Name).ToArray();
                // Fields
                foreach (var field in type.Fields)
                {
                    var isEventBackingField = events.Any(_ => String.Equals(_, field.Name, StringComparison.InvariantCultureIgnoreCase));
                    // wenn kein Event backing-field
                    if (!isEventBackingField)
                    {
                        Assert.That(field.IsPublic);
                    }
                }
                // Methods
                foreach (var method in type.Methods)
                {
                    Assert.That(method.IsPublic);
                }
                // Properties, getters & setters
                foreach (var method in type.Properties)
                {
                    if (method.SetMethod is not null)
                        Assert.That(method.SetMethod.IsPublic);
                    if (method.GetMethod is not null)
                        Assert.That(method.GetMethod.IsPublic);
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
        public void HasInjected_MethodCall()
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
        public void HasInjected_LoaderType()
        {
            Assert.That(_injectedDataAssembly.Types.Any(_ => _.FullName == "Synapse.Injector.Loader"));
        }
        [Test]
        public void HasExpected_TypesCount()
        {
            Assert.AreEqual(_cleanDataAssembly.Types.Count, _injectedDataAssembly.Types.Count - 1);
        }
    }
}