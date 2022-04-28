namespace AntiEpos
{
    public class AEException : System.Exception
    {
        public AEException() : base() { }

        protected AEException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public AEException(string message) : base(message) { }

        public AEException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
