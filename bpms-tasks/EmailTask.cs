namespace Simple.Bpms.Tasks.SystemTasks
{
    using System.Collections.Generic;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.DurableTask;
    using Typesafe.Mailgun;

    /// <summary>
    /// INPUT:
    ///     key : value = 'text' : the text that we want to run analysis on
    ///     
    /// OUTPUT:
    ///     key : value = 'sentiment_score' : the sentiment score, +ve numbers are good sentiment, -ve bad
    /// 
    /// </summary>
    public class EmailTask : BpmsTask
    {
        const string ToInputKey = "to";
        const string FromInputkey = "from";
        const string SubjectInputKey = "subject";
        const string BodyInputKey = "body";

        protected override async Task<IDictionary<string, string>> OnExecuteAsync(TaskContext context, 
            IDictionary<string, string> input)
        {
            string to = input[ToInputKey];
            string from = input[FromInputkey];
            string subject = input[SubjectInputKey];
            string body = input[BodyInputKey];

            if(string.IsNullOrWhiteSpace(to))
            {
                throw new BpmsTaskException("Invalid To");
            }

            var client = new MailgunClient("sandboxcc8bc56fb7e74f10899af762bb556db4.mailgun.org", "key-7lj9mnn3wuk4f7hyb4763llcd-s84u-7");
            
            MailMessage message = new MailMessage(from, to, subject, body);
            client.SendMail(message);

            return new Dictionary<string, string>() { };
        }

        protected override string[] RequiredInputKeys
        {
            get 
            {
                return new string[] { ToInputKey, FromInputkey, SubjectInputKey, BodyInputKey };
            } 
        }
    }
}
