namespace Simple.Bpms.Tasks
{
    using System;

    public class BpmsTaskException : Exception
    {
        public BpmsTaskException(string message)
            : base(message)
        {
        }

        public BpmsTaskException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
