using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace Federator.Core.Exchange
{
    /// <summary>
    /// One condition of a set exactly as the exchange file writes it, with the whitespace in
    /// front of it, and what it asks read off that text. A correction changes the flags or
    /// the value and nothing else, so every other byte of the condition is written back as
    /// the file had it, F116.
    /// </summary>
    internal sealed class WrittenCondition
    {
        private WrittenCondition(string lead, string element, string test, int flags, string property, string value)
        {
            Lead = lead;
            Element = element;
            Test = test;
            Flags = flags;
            Property = property;
            Value = value;
        }

        /// <summary>The whitespace between the element before it and this one.</summary>
        internal string Lead { get; private set; }

        /// <summary>The condition element itself, from its opening tag to its closing tag.</summary>
        internal string Element { get; private set; }

        /// <summary>The test attribute, equals or contains in the client's files, or null where it carries none.</summary>
        internal string Test { get; private set; }

        /// <summary>The flags attribute, read by ExchangeReader.</summary>
        internal int Flags { get; private set; }

        /// <summary>The internal name of the property it asks on, or null where it names none.</summary>
        internal string Property { get; private set; }

        /// <summary>The value it asks for, with the file's escapes read, or null where it holds none.</summary>
        internal string Value { get; private set; }

        /// <summary>
        /// One condition read off its text by ExchangeReader's own reading of a condition, so
        /// the condition a correction reads and the one the plan builds cannot disagree. An
        /// element that is not XML throws, because a condition this cannot read is one it
        /// must not rewrite.
        /// </summary>
        internal static WrittenCondition Read(string lead, string element)
        {
            SearchConditionDefinition read = ExchangeReader.ReadCondition(XElement.Parse(element));

            return new WrittenCondition(
                lead,
                element,
                read.Test,
                read.Flags,
                read.Property == null ? null : read.Property.InternalName,
                read.Value == null ? null : read.Value.Data);
        }

        /// <summary>The same condition asking for another value, written with the escapes an XML file needs.</summary>
        internal WrittenCondition WithValue(string value)
        {
            int holder = Element.IndexOf("<value", StringComparison.Ordinal);
            int data = holder < 0 ? -1 : Element.IndexOf("<data", holder, StringComparison.Ordinal);
            int shut = data < 0 ? -1 : Element.IndexOf('>', data);
            int end = shut < 0 ? -1 : Element.IndexOf("</data>", shut, StringComparison.Ordinal);

            if (end < 0)
            {
                throw new InvalidDataException("A condition asking for \"" + Value
                    + "\" holds no value element this can rewrite: " + Element);
            }

            string element = Element.Substring(0, shut + 1) + Escaped(value) + Element.Substring(end);
            return new WrittenCondition(Lead, element, Test, Flags, Property, value);
        }

        /// <summary>The same condition carrying other flags. The attribute is added where the file wrote none.</summary>
        internal WrittenCondition WithFlags(int flags)
        {
            if (flags == Flags)
            {
                return this;
            }

            int shut = Element.IndexOf('>');
            int at = Element.IndexOf("flags=\"", StringComparison.Ordinal);
            string number = flags.ToString(CultureInfo.InvariantCulture);
            string element;

            if (at >= 0 && at < shut)
            {
                at += "flags=\"".Length;
                int ends = Element.IndexOf('"', at);
                element = Element.Substring(0, at) + number + Element.Substring(ends);
            }
            else
            {
                element = Element.Substring(0, shut) + " flags=\"" + number + "\"" + Element.Substring(shut);
            }

            return new WrittenCondition(Lead, element, Test, flags, Property, Value);
        }

        /// <summary>The same condition with other whitespace in front of it, so a copy sits where the set's own conditions do.</summary>
        internal WrittenCondition WithLead(string lead)
        {
            return new WrittenCondition(lead, Element, Test, Flags, Property, Value);
        }

        /// <summary>The text an XML file holds for that value, the three characters element content cannot carry as they are escaped.</summary>
        private static string Escaped(string value)
        {
            return (value ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
