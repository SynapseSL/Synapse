using System;

namespace Synapse.Api
{
    /// <summary>
    /// An Annotation that marks a class/method as injected.
    /// 
    /// This Attribute is used by the injector to
    /// find fields to inject into
    /// </summary>
    public class Injected : Attribute { }

    /// <summary>
    /// An Annotation that marks a class/method as unstable.
    /// This Attribute should generally be applied to something
    /// that can be used from outside but has an incalculable
    /// outcome or might break other plugins and/or the framework
    /// itself in 
    /// </summary>
    public class Unstable : Attribute { }
    
    /// <summary>
    /// An Annotation that marks a class/method of a plugin as
    /// safe to use in other plugins
    /// </summary>
    public class API : Attribute { }
    
}