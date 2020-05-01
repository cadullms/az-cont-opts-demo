using System;
namespace lib.Messages
{
    public class RequestMessage
    {
        public RequestMessage()
        {
            RequestID = Guid.NewGuid();
        }

        public Guid RequestID { get; set; }

        public DateTime DateNow => DateTime.UtcNow;

        public string Content { get; set; }

        public int Count { get; set; }

        public override string ToString()
        {
            return $"Message ID {RequestID} was generated @ { DateNow}";
        }
    }
}