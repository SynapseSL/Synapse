using System;
using UnityEngine;

namespace Synapse3.SynapseModule.KeyBind;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class KeyBindAttribute : Attribute
{
    public KeyCode Bind { get; set; }

    public string CommandName { get; set; }

    public string CommandDescription { get; set; }

}
