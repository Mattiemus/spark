﻿namespace Spark.UI.Media
{
    using System.IO;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml;

    using Data;

    public static class XamlReader
    {
        public static object Load(Stream stream)
        {
            XmlReader xml = XmlReader.Create(stream);
            return Load(xml);
        }

        public static object Load(XmlReader reader)
        {
            XamlXmlReader xaml = new XamlXmlReader(reader);
            return Load(xaml);
        }

        public static object Load(System.Xaml.XamlReader reader)
        {
            XamlObjectWriter writer = new XamlObjectWriter(
                new XamlSchemaContext(),
                new XamlObjectWriterSettings
                {
                    XamlSetValueHandler = SetValue,
                });

            while (reader.Read())
            {
                writer.WriteNode(reader);
            }

            object result = writer.Result;
            DependencyObject dependencyObject = result as DependencyObject;

            if (dependencyObject != null)
            {
                NameScope.SetNameScope(dependencyObject, writer.RootNameScope);
            }

            return result;
        }

        public static void Load(Stream stream, object component)
        {
            DependencyObject dependencyObject = component as DependencyObject;
            NameScope nameScope = new NameScope();

            if (dependencyObject != null)
            {
                NameScope.SetNameScope(dependencyObject, nameScope);
            }

            XmlReader xml = XmlReader.Create(stream);
            XamlXmlReader reader = new XamlXmlReader(xml);
            XamlObjectWriter writer = new XamlObjectWriter(
                new XamlSchemaContext(),
                new XamlObjectWriterSettings
                {
                    RootObjectInstance = component,
                    ExternalNameScope = nameScope,
                    RegisterNamesOnExternalNamescope = true,
                    XamlSetValueHandler = SetValue,
                });

            while (reader.Read())
            {
                writer.WriteNode(reader);
            }
        }

        private static void SetValue(object sender, XamlSetValueEventArgs e)
        {
            BindingBase binding = e.Value as BindingBase;

            if (binding != null)
            {
                DependencyProperty dp = DependencyObject.GetPropertyFromName(
                    e.Member.DeclaringType.UnderlyingType,
                    e.Member.Name);
                ((FrameworkElement)sender).SetBinding(dp, binding);
                e.Handled = true;
            }
        }
    }
}
