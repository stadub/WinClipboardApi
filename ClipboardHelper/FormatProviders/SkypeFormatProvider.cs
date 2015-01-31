using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ClipboardHelper.FormatProviders
{
    public class SkypeMessageTextLine
    {
        public string Text { get; set; }
        public bool Quote { get; set; }
    }


    public class SkypeQuote 
    {

        public string Author { get; set; }

        public string AuthorName { get; set; }

        public string Conversation { get; set; }

        public string Guid { get; set; }

        public long Timestamp { get; set; }

        public SkypeQuote()
        {
            LegacyQuote= new List<SkypeMessageTextLine>();
        }
        public List<SkypeMessageTextLine> LegacyQuote { get; private set; }

        public void AddQuoteText(string text,bool quote=false)
        {
            LegacyQuote.Add(new SkypeMessageTextLine{Quote = quote,Text = text});
        }
    }

    [XmlRoot("quote")]
    public class SkypeQuoteSerializable:IXmlSerializable
    {
        public readonly SkypeQuote Quote;

        public SkypeQuoteSerializable(SkypeQuote quote)
        {
            this.Quote = quote;
        }

        public SkypeQuoteSerializable()
        {
            Quote= new SkypeQuote();
        }
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            //reader.ReadStartElement("quote");
            Quote.Author=reader.GetAttribute("author");

            Quote.AuthorName=reader.GetAttribute("authorname");
            Quote.Conversation=reader.GetAttribute("conversation");
            Quote.Guid=reader.GetAttribute("guid");
            Quote.Timestamp=Convert.ToInt64(reader.GetAttribute("timestamp"));

            if(reader.IsEmptyElement)
                return;

            reader.MoveToContent();
            bool legacyquoteNode = false;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        Debug.Assert(reader.Name == "legacyquote");
                        legacyquoteNode = true;
                        break;
                    case XmlNodeType.Text:
                        Quote.AddQuoteText(reader.Value, legacyquoteNode);
                        break;
                    case XmlNodeType.EndElement:
                        legacyquoteNode = false;
                        break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            //writer.WriteStartElement("quote");
            WriteAttributeIfNotNull("author", Quote.Author, writer);

            WriteAttributeIfNotNull("authorname", Quote.AuthorName, writer);
            WriteAttributeIfNotNull("conversation", Quote.Conversation, writer);
            WriteAttributeIfNotNull("guid", Quote.Guid, writer);
            WriteAttributeIfNotNull("timestamp", Quote.Timestamp.ToString(),writer);

            foreach (var textLine in Quote.LegacyQuote)
            {
                if (textLine.Quote)
                    writer.WriteElementString("legacyquote", textLine.Text);
                else
                    writer.WriteString(textLine.Text);
            }
            //writer.WriteEndElement();
        }

        private void WriteAttributeIfNotNull(string attributeName, string attributeValue, XmlWriter writer)
        {
            if (attributeValue != null) 
                writer.WriteAttributeString(attributeName, attributeValue);
            
        }
    }

    public class SkypeFormatProvider : DataFormatProvider
    {
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(SkypeQuoteSerializable));

        private UnicodeStringSerializer provider;
        private const int XmlHeaderLenght = 39;

        public SkypeFormatProvider(): this(new SkypeQuote()){}

        public SkypeFormatProvider(SkypeQuote quote)
        {
            Quote = quote;
            provider = new UnicodeStringSerializer();
        }

        public override string FormatId
        {
            get { return "SkypeMessageFragment"; }
        }
        public SkypeQuote FromString(string data)
        {
            var reader = new StringReader(data);
            var quoteSerializable = (SkypeQuoteSerializable) XmlSerializer.Deserialize(reader);
            return quoteSerializable.Quote;
        }
        public string SerilizeToString(SkypeQuote quote)
        {
            var serializeble = new SkypeQuoteSerializable(quote);
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var xmlWriter = new XmlTextWriter(writer);
            var xmlSerializer = new XmlSerializer(typeof(SkypeQuoteSerializable));
            xmlSerializer.Serialize(xmlWriter, serializeble);
            Debug.Assert(builder.ToString(0,XmlHeaderLenght)=="<?xml version=\"1.0\" encoding=\"utf-16\"?>");

            return builder.ToString(XmlHeaderLenght, builder.Length - XmlHeaderLenght);
        }

        public SkypeQuote Quote { get;private set;}

        public override object Data
        {
            get { return Quote; }
        }

        protected override void DeserializeData(byte[] data)
        {
            var strData = provider.Deserialize(data);
            Quote= FromString(strData);
        }

        public override byte[] Serialize()
        {
            var stringData = SerilizeToString(Quote);
            return provider.Serialize(stringData);
        }
    }
}