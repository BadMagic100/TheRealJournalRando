using System;

namespace TheRealJournalRando.Rando
{
    [Serializable]
    public class MissingTermException : Exception
    {
        public MissingTermException() { }
        public MissingTermException(string message) : base(message) { }
        public MissingTermException(string message, Exception inner) : base(message, inner) { }
        protected MissingTermException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
