using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClipboardHelper.FormatProviders
{
    public class HtmlClipboardFormatData
    {
        public string Version { get; set; }

        [FormatedNumeric("D9")]
        public long StartHTML { get; set; }

        [FormatedNumeric("D9")]
        public long EndHTML { get; set; }

        [FormatedNumeric("D9")]
        public long StartFragment { get; set; }

        [FormatedNumeric("D9")]
        public long EndFragment { get; set; }

        [FormatedNumeric("D9")]
        public long StartSelection { get; set; }

        [FormatedNumeric("D9")]
        public long EndSelection { get; set; }

        public Uri SourceURL { get; set; }

        [NonSerializable]
        public string Html { get; set; }

    }

    
    public class HtmlFormatProvider:DataFormatProvider<HtmlClipboardFormatData>
    {
        UnicodeStringSerializer serializer;
        public HtmlFormatProvider()
        {
            serializer = new UnicodeStringSerializer();
        }

        /// <summary>
        ///  Using Expresso Version: 3.0.4750, http://www.ultrapico.com  
        ///  A description of the regular expression:  
        ///  Beginning of line or string
        ///  [GroupName]: A named capture group. [.*]  Any character, any number of repetitions
        ///  :
        ///  [GroupValue]: A named capture group. [.*] Any character, any number of repetitions
        ///  End of line or string
        /// </summary>
        private static Regex groupsRegex = new Regex("^(?<GroupName>[^:]*):(?<GroupValue>.*)$",
            RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);


        /// <summary>
        ///  A description of the regular expression:  
        ///  Beginning of line or string
        ///  [1]: A numbered capture group. [<.*]
        ///      <.*
        ///          <
        ///          Any character, any number of repetitions
        /// </summary>
        public static Regex htmlDoc = new Regex(
              "^(<.*)",
            RegexOptions.Multiline
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.Compiled
            );


        public override string FormatId
        {
            get { return "HTML Format"; }
        }

        public override byte[] Serialize(HtmlClipboardFormatData data)
        {
            StringBuilder clipboardDataBuilder= new StringBuilder();
            var properties = typeof(HtmlClipboardFormatData).GetProperties();
            foreach (var property in properties)
            {
                var nonSerializable = property.GetCustomAttribute<NonSerializableAttribute>();
                if (nonSerializable != null)
                    continue;

                var value=GetPropertyValue(property, data);
                var name = property.Name;
                clipboardDataBuilder.AppendFormat("{0}:{1}",name,value);
                clipboardDataBuilder.AppendLine();
            }
            clipboardDataBuilder.Append(data.Html);
            var dataString= clipboardDataBuilder.ToString();
            return serializer.Serialize(dataString);
        }

        private string GetPropertyValue(PropertyInfo property, object instance)
        {
            var propertyValue = property.GetValue(instance);

            var formated = property.GetCustomAttribute<FormatedAttribute>();
            if (formated != null)
            {
                Debug.Assert(formated.Format != null);
                var propertyConverter = (IFormattable)propertyValue;
                var formatedString = propertyConverter.ToString(formated.Format, CultureInfo.InvariantCulture);
                return formatedString;
            }
            try
            {
                var convertedValue = Convert.ChangeType(propertyValue, typeof(string));
                return (string)convertedValue;
            }
            catch (InvalidCastException)
            {
            }

            return propertyValue.ToString();

        }
        public override HtmlClipboardFormatData Deserialize(byte[] data)
        {
            var htmlData=serializer.Deserialize(data);

            return Parse(htmlData);
        }

        private HtmlClipboardFormatData Parse(string stringData)
        {
            string[] results = htmlDoc.Split(stringData);

            Debug.Assert(results.Length > 1 && results.Length<4);
            if(results.Length==3)
                Debug.Assert(results[2].Trim().Length==0);


            var properties = typeof(HtmlClipboardFormatData).GetProperties();

            var htmlClipboardData = new HtmlClipboardFormatData();
            htmlClipboardData.Html = results[1];


            MatchCollection ms = groupsRegex.Matches(results[0]);
            foreach (Match match in ms)
            {
                if(!match.Success)
                    continue;

                string name = match.Groups["GroupName"].Value;
                var value = match.Groups["GroupValue"].Value.Trim();
                var property = properties.FirstOrDefault(x => x.Name == name);

                SetPropertyValue(property, htmlClipboardData,value);
            }
            return htmlClipboardData;

        }

        private static void SetPropertyValue(PropertyInfo property, object instance,string value)
        {
            Debug.Assert(property != null);
            if (property == null||instance==null|value==null)
                return;
            var propertyType = property.PropertyType;
            var stringType = typeof (string);
            if (propertyType == stringType)
            {
                property.SetValue(instance, value);
                return;
            }
            try
            {
                var convertedValue = Convert.ChangeType(value, propertyType);
                property.SetValue(instance, convertedValue);
                return;
            }
            catch (InvalidCastException)
            {
            }
            //Throws MissingMethodException if corresponding constructor not found
            var propValue = Activator.CreateInstance(propertyType, value);
            property.SetValue(instance, propValue);
        }
    }
}
