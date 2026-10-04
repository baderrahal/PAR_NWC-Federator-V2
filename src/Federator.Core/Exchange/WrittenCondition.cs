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

            return new WrittenCondition(
                Lead, WithAttribute("flags", flags.ToString(CultureInfo.InvariantCulture)), Test, flags, Property, Value);
        }

        /// <summary>The same condition asking with another test, equals or contains. The attribute is added where the file wrote none.</summary>
        internal WrittenCondition WithTest(string test)
        {
            if (string.Equals(test, Test, StringComparison.Ordinal))
            {
                return this;
            }

            return new WrittenCondition(Lead, WithAttribute("test", test), test, Flags, Property, Value);
        }

        /// <summary>The same condition with other whitespace in front of it, so a copy sits where the set's own conditions do.</summary>
        internal WrittenCondition WithLead(string lead)
        {
            return new WrittenCondition(lead, Element, Test, Flags, Property, Value);
        }

        /// <summary>
        /// The text an XML file holds for that name or value, the four characters an attribute
        /// or element content cannot always carry as they are escaped. The one escape of the
        /// corrections, for a value written into a condition and for a set name looked for in
        /// the file's text, F116.
        /// </summary>
        internal static string Escaped(string text)
        {
            return (text ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        /// <summary>The element with that attribute of its opening tag set to that value, added at the end of the tag where it carries none.</summary>
        private string WithAttribute(string name, string value)
        {
            int shut = Element.IndexOf('>');
            string opens = name + "=\"";
            string text = Escaped(value);
            int at = Element.IndexOf(opens, StringComparison.Ordinal);

            // An attribute of that name, and not one whose name merely ends with it.
            while (at > 0 && at < shut && !char.IsWhiteSpace(Element[at - 1]))
            {
                at = Element.IndexOf(opens, at + 1, StringComparison.Ordinal);
            }

            if (at > 0 && at < shut)
            {
                at += opens.Length;
                int ends = Element.IndexOf('"', at);
                return Element.Substring(0, at) + text + Element.Substring(ends);
            }

            return Element.Substring(0, shut) + " " + opens + text + "\"" + Element.Substring(shut);
        }
    }
}
