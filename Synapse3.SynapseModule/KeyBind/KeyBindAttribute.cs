using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synapse3.SynapseModule.KeyBind;

[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class KeyBindAttribute : Attribute
{
    public KeyCode Bind { get; set; }

    public string CommandName { get; set; }

    public string CommandDescription { get; set; }

}
