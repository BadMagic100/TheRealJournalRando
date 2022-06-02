using ItemChanger;
using Newtonsoft.Json;

namespace TheRealJournalRando.IC
{
    public class FormatString : IString
    {
        public IString str;
        public object[] format;

        public FormatString(IString str, params object[] format)
        {
            this.str = str;
            this.format = format;
        }

        [JsonIgnore]
        public string Value
        {
            get
            {
                object[] formatCopy = new object[format.Length];
                format.CopyTo(formatCopy, 0);
                for (int i = 0; i < formatCopy.Length; i++)
                {
                    if (formatCopy[i] is IString s)
                    {
                        formatCopy[i] = s.Value;
                    }
                    else if (formatCopy[i] is IBool b)
                    {
                        formatCopy[i] = b.Value;
                    }
                }
                return string.Format(str.Value, formatCopy);
            }
        }

        public IString Clone()
        {
            FormatString clone = (FormatString)MemberwiseClone();
            clone.format = new object[format.Length];
            format.CopyTo(clone.format, 0);
            return clone;
        }
    }
}
