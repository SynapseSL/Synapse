using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectGraphVisitors;
using YamlDotNet.Serialization.TypeInspectors;

namespace Synapse.Config
{
    /*
     * The code bellow is from https://dotnetfiddle.net/8M6iIE
     * Great thanks to Antoine Aubry for providing this awesome
     * code sample, showing how to emit comments in YamlDotNet
     */
    public class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector _innerTypeDescriptor;

        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            if (innerTypeDescriptor == null)
            {
                throw new ArgumentNullException("innerTypeDescriptor");
            }

            this._innerTypeDescriptor = innerTypeDescriptor;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return _innerTypeDescriptor
                .GetProperties(type, container)
                .Select(d => new CommentsPropertyDescriptor(d));
        }

        private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
        {
            private readonly IPropertyDescriptor _baseDescriptor;

            public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
            {
                this._baseDescriptor = baseDescriptor;
                Name = baseDescriptor.Name;
            }

            public string Name { get; set; }

            public Type Type { get { return _baseDescriptor.Type; } }

            public Type TypeOverride
            {
                get { return _baseDescriptor.TypeOverride; }
                set { _baseDescriptor.TypeOverride = value; }
            }

            public int Order { get; set; }

            public ScalarStyle ScalarStyle
            {
                get { return _baseDescriptor.ScalarStyle; }
                set { _baseDescriptor.ScalarStyle = value; }
            }

            public bool CanWrite { get { return _baseDescriptor.CanWrite; } }

            public void Write(object target, object value)
            {
                _baseDescriptor.Write(target, value);
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return _baseDescriptor.GetCustomAttribute<T>();
            }

            [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
            public IObjectDescriptor Read(object target)
            {
                var description = _baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
                return description != null
                    ? new CommentsObjectDescriptor(_baseDescriptor.Read(target), description.Description)
                    : _baseDescriptor.Read(target);
            }
        }
    }

    public sealed class CommentsObjectDescriptor : IObjectDescriptor
    {
        private readonly IObjectDescriptor _innerDescriptor;

        public CommentsObjectDescriptor(IObjectDescriptor innerDescriptor, string comment)
        {
            this._innerDescriptor = innerDescriptor;
            this.Comment = comment;
        }

        public string Comment { get; private set; }

        public object Value { get { return _innerDescriptor.Value; } }
        public Type Type { get { return _innerDescriptor.Type; } }
        public Type StaticType { get { return _innerDescriptor.StaticType; } }
        public ScalarStyle ScalarStyle { get { return _innerDescriptor.ScalarStyle; } }
    }

    public class CommentsObjectGraphVisitor : ChainedObjectGraphVisitor
    {
        public CommentsObjectGraphVisitor(IObjectGraphVisitor<IEmitter> nextVisitor)
            : base(nextVisitor)
        {
        }

        public override bool EnterMapping(IPropertyDescriptor key, IObjectDescriptor value, IEmitter context)
        {
            var commentsDescriptor = value as CommentsObjectDescriptor;
            if (commentsDescriptor != null && commentsDescriptor.Comment != null)
            {
                context.Emit(new Comment(commentsDescriptor.Comment, false));
            }

            return base.EnterMapping(key, value, context);
        }
    }
}
