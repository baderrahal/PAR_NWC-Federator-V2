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
        private WrittenCondition(string lead, string element, string test, int flags, string value)
        {
            Lead = lead;
            Element = element;
            Test = test;
            Flags = flags;
            Value = value;
        }

        /// <summary>The whitespace between the element before it and this one.</summary>
        internal string Lead { get; private set; }

        /// <summary>The condition element itself, from its opening tag to its closing tag.</summary>
        internal string Element { get; private set; }

        /// <summary>The test attribute, equals or contains in the client's files, or null where it carries none.</summary>
        internal string Test { get; private set; }

        /// <summary>The flags attribute, zero where the file wrote none or one that is not a number, the way ExchangeReader reads it.</summary>
        internal int Flags { get; private set; }

        /// <summary>The value it asks for, with the file's escapes read, or null where it holds none.</summary>
        internal string Value { get; private set; }

        /// <summary>
        /// One condition read off its text. An element that is not XML throws, because a
        /// condition this cannot read is one it must not rewrite.
        /// </summary>
        internal static WrittenCondition Read(string lead, string element)
        {
            XElement parsed = XElement.Parse(element);
            XAttribute test = parsed.Attribute("test");
            XAttribute flags = parsed.Attribute("flags");
            XElement data = Child(Child(parsed, "value"), "data");
            int read;

            return new WrittenCondition(
                lead,
                element,
                test == null ? null : test.Value,
                flags != null && int.TryParse(flags.Value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out read) ? read : 0,
                data == null ? null : data.Value);
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
            return new WrittenCondition(Lead, element, Test, Flags, value);
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

            return new WrittenCondition(Lead, element, Test, flags, Value);
        }

        /// <summary>The same condition with other whitespace in front of it, so a copy sits where the set's own conditions do.</summary>
        internal WrittenCondition WithLead(string lead)
        {
            return new WrittenCondition(lead, Element, Test, Flags, Value);
        }

        /// <summary>The text an XML file holds for that value, the three characters element content cannot carry as they are escaped.</summary>
        private static string Escaped(string value)
        {
            return (value ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static XElement Child(XElement parent, string localName)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (XElement child in parent.Elements())
            {
                if (string.Equals(child.Name.LocalName, localName, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }
    }
}
