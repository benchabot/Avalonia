using System;
using System.Collections;
using Avalonia.Controls.Templates;
using Avalonia.Data;

#nullable enable

namespace Avalonia.Controls.Generators
{
    public class TreeItemContainerGenerator<T> : ItemContainerGenerator
        where T : class, IControl, new()
    {
        public TreeItemContainerGenerator(
            ItemsControl owner,
            AvaloniaProperty<object?> headerProperty,
            AvaloniaProperty<IDataTemplate?> headerTemplateProperty,
            AvaloniaProperty<IEnumerable?> itemsProperty,
            AvaloniaProperty<bool> isExpandedProperty)
            : base(owner)
        {
            HeaderProperty = headerProperty ?? throw new ArgumentNullException(nameof(headerProperty));
            HeaderTemplateProperty = headerTemplateProperty ?? throw new ArgumentNullException(nameof(headerTemplateProperty));
            ItemsProperty = itemsProperty ?? throw new ArgumentNullException(nameof(itemsProperty));
            IsExpandedProperty = isExpandedProperty;
        }

        /// <summary>
        /// Gets the container's Header property.
        /// </summary>
        protected AvaloniaProperty<object?> HeaderProperty { get; }

        /// <summary>
        /// Gets the container's HeaderTemplate property.
        /// </summary>
        protected AvaloniaProperty<IDataTemplate?> HeaderTemplateProperty { get; }

        /// <summary>
        /// Gets the item container's Items property.
        /// </summary>
        protected AvaloniaProperty<IEnumerable?> ItemsProperty { get; }

        /// <summary>
        /// Gets the item container's IsExpanded property.
        /// </summary>
        protected AvaloniaProperty<bool> IsExpandedProperty { get; }

        protected override IControl CreateContainer(ElementFactoryGetArgs args)
        {
            if (args.Data is T c)
            {
                return c;
            }

            var result = new T();
            var template = GetTreeDataTemplate(args.Data, Owner.ItemTemplate);
            var itemsSelector = template.ItemsSelector(args.Data);

            result.Bind(
                HeaderProperty,
                result.GetBindingObservable(Control.DataContextProperty),
                BindingPriority.Style);
            result.Bind(
                HeaderTemplateProperty,
                Owner.GetBindingObservable(ItemsControl.ItemTemplateProperty),
                BindingPriority.Style);

            if (itemsSelector != null)
            {
                BindingOperations.Apply(result, ItemsProperty, itemsSelector, null);
            }

            return result;
        }

        private ITreeDataTemplate GetTreeDataTemplate(object item, IDataTemplate? primary)
        {
            var template = Owner.FindDataTemplate(item, primary) ?? FuncDataTemplate.Default;
            var treeTemplate = template as ITreeDataTemplate ?? new WrapperTreeDataTemplate(template);
            return treeTemplate;
        }

        class WrapperTreeDataTemplate : ITreeDataTemplate
        {
            private readonly IDataTemplate _inner;
            public WrapperTreeDataTemplate(IDataTemplate inner) => _inner = inner;
            public IControl Build(object param) => _inner.Build(param);
            public bool Match(object data) => _inner.Match(data);
            public InstancedBinding? ItemsSelector(object item) => null;
        }
    }
}
