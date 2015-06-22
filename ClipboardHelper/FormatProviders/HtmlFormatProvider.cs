using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Utils;
using Utils.TypeMapping;
using Utils.TypeMapping.TypeMappers;

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

        [Formated(null)]
        public Uri SourceURL { get; set; }

        [NonSerializable]
        public string Html { get; set; }

    }


    public class HtmlFormatProvider : DataFormatProvider
    {
        private UnicodeStringSerializer serializer;

        public HtmlFormatProvider(HtmlClipboardFormatData htmlData)
        {
            serializer = new UnicodeStringSerializer();
            HtmlData = htmlData;
        }

        public HtmlFormatProvider() : this(new HtmlClipboardFormatData())
        {
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



        public override byte[] Serialize()
        {
            var mapper = new StringFormatter<HtmlClipboardFormatData>();
            var data = mapper.Map(HtmlData);
            return serializer.Serialize(data + HtmlData.Html);
        }


        public HtmlClipboardFormatData HtmlData { get; private set; }

        public override object Data
        {
            get { return HtmlData; }
        }

        protected override void DeserializeData(byte[] data)
        {
            var htmlData = serializer.Deserialize(data);

            Parse(htmlData);
        }

        protected void Parse(string stringData)
        {
            string[] results = htmlDoc.Split(stringData);

            Debug.Assert(results.Length > 1 && results.Length < 4);
            if (results.Length == 3)
                Debug.Assert(results[2].Trim().Length == 0);


            var propertiesDictionary=new Dictionary<string, string>();
            MatchCollection ms = groupsRegex.Matches(results[0]);
            foreach (Match match in ms)
            {
                if (!match.Success)
                    continue;

                string name = match.Groups["GroupName"].Value;
                var value = match.Groups["GroupValue"].Value.Trim();

                propertiesDictionary.Add(name, value);
                
            }

            propertiesDictionary.Add("Html", results[1]);

            var mapper = new DictionaryMapper<string, HtmlClipboardFormatData>();

            HtmlData = mapper.Map(propertiesDictionary);
            
        }
    }


}
