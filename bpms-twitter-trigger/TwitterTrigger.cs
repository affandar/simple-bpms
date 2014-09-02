namespace Simple.Bpms.Triggers.Twitter
{
    using System;
    using System.Threading.Tasks;
    using Tweetinvi;
    using Tweetinvi.Core.Events.EventArguments;
    using Tweetinvi.Core.Interfaces.Streaminvi;

    public class TwitterTrigger
    {
        // TODO: this needs to flow through config
        string consumerKey = "QFDOPs69d9rlisuLhEosLh64d";
        string consumerSecret = "sIg3ztMHIvOEunC0lclzWsjKomcP9WybPmjOgPzSwmsSq2hPh4";
        string accessToken = "884906707-0G9K4Yz5fHsOhMIy4TlrN02zjhc8bQSII3dWVPob";
        string accessTokenSecret = "HQbMfFxmHWqLlbYBthawKZO53lCg0n0kHdm8LNDvbrvQf";

        readonly string[] filters;
        readonly bool matchAnyFilter;

        IFilteredStream filteredStream;
        

        public TwitterTrigger(string[] filters, bool matchAnyFilter)
        {
            // Setup your application credentials
            TwitterCredentials.ApplicationCredentials = TwitterCredentials.CreateCredentials(accessToken, accessTokenSecret, consumerKey, consumerSecret);

            this.filters = filters;
            this.matchAnyFilter = matchAnyFilter;
        }

        public async Task StartAsync()
        {
            if (this.filters == null || this.filters.Length == 0)
            {
                throw new ArgumentException("filters not provided.");
            }

            this.filteredStream = Stream.CreateFilteredStream();
            foreach (string filter in this.filters)
            {
                this.filteredStream.AddTrack(filter);
            }

            this.filteredStream.MatchingTweetReceived += filteredStream_MatchingTweetReceived;

            if (this.matchAnyFilter)
            {
                await this.filteredStream.StartStreamMatchingAnyConditionAsync();
            }
            else
            {
                await this.filteredStream.StartStreamMatchingAllConditionsAsync();
            }
        }

        public void Stop()
        {
            if (this.filteredStream != null)
            {
                this.filteredStream.StopStream();
            }
        }

        void filteredStream_MatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs args)
        {
            Console.WriteLine(args.Tweet.Text);
        }
    }
}
