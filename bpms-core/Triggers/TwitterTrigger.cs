namespace Simple.Bpms.Triggers.Twitter
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Simple.Bpms.Triggers;
    using Tweetinvi;
    using Tweetinvi.Core.Events.EventArguments;
    using Tweetinvi.Core.Interfaces.Streaminvi;

    /// <summary>
    /// INPUT:
    ///     key, value = 'hashtag', the hastag you want to start monitoring
    ///
    /// OUTPUT:
    ///     key, value = 'tweet_body', the body of the tweet that matches the hashtag you are monitoring     
    /// </summary>
    /// 

    // TODO : this class needs to be thead-safe but is not
    public class TwitterTrigger : ITrigger
    {
        public const string Input_HashTagKey = "hashtag";
        public const string Output_TweetBodyKey = "tweet_body";

        IDictionary<string, TwitterEventTrigger> eventTriggerMap;

        public TwitterTrigger()
        {
            this.eventTriggerMap = new Dictionary<string, TwitterEventTrigger>();
        }

        public string Type
        {
            get { return "Twitter"; }
        }

        public void RegisterEventTrigger(TriggerManager manager, TriggerEventRegistration registration)
        {
            if(registration == null)
            {
                throw new ArgumentNullException("registration");
            }

            if(string.IsNullOrWhiteSpace(registration.Id))
            {
                throw new ArgumentException("registration");
            }

            if(eventTriggerMap.ContainsKey(registration.Id))
            {
                throw new ArgumentException("duplicate registration id: " + registration.Id);
            }

            object hashtagObj = null;
            if (!registration.TriggerData.TryGetValue(Input_HashTagKey, out hashtagObj))
            {
                throw new ArgumentException("Input data does not contain required key: " + Input_HashTagKey);
            }

            TwitterEventTrigger eventTrigger = new TwitterEventTrigger(new string[] { (string)hashtagObj }, true,
                o =>
                {
                    string tweetText = (string)o;
                    Dictionary<string, string> heap = new Dictionary<string, string>()
                    {
                        { Output_TweetBodyKey, tweetText }
                    };

                    manager.CreateBpmsFlow(registration.Flow, heap);
                });

            eventTriggerMap.Add(registration.Id, eventTrigger);

            // TODO : make true async
            eventTrigger.StartAsync().Wait();
        }

        public void UnregisterEventTrigger(string registrationId)
        {
            if (string.IsNullOrWhiteSpace(registrationId))
            {
                throw new ArgumentException("registration");
            }

            if (!eventTriggerMap.ContainsKey(registrationId))
            {
                return;
            }

            TwitterEventTrigger eventTrigger = this.eventTriggerMap[registrationId];
            eventTrigger.Stop();
            this.eventTriggerMap.Remove(registrationId);
        }
    }

    public class TwitterEventTrigger
    {
        // TODO: this needs to flow through config
        string consumerKey = "QFDOPs69d9rlisuLhEosLh64d";
        string consumerSecret = "sIg3ztMHIvOEunC0lclzWsjKomcP9WybPmjOgPzSwmsSq2hPh4";
        string accessToken = "884906707-0G9K4Yz5fHsOhMIy4TlrN02zjhc8bQSII3dWVPob";
        string accessTokenSecret = "HQbMfFxmHWqLlbYBthawKZO53lCg0n0kHdm8LNDvbrvQf";

        readonly string[] filters;
        readonly bool matchAnyFilter;

        IFilteredStream filteredStream;

        Action<object> eventRaisedFunc;

        public TwitterEventTrigger(string[] filters, bool matchAnyFilter, Action<object> eventRaisedFunc)
        {
            // Setup your application credentials
            TwitterCredentials.ApplicationCredentials = TwitterCredentials.CreateCredentials(accessToken, accessTokenSecret, consumerKey, consumerSecret);

            this.filters = filters;
            this.matchAnyFilter = matchAnyFilter;
            this.eventRaisedFunc = eventRaisedFunc;
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
            this.eventRaisedFunc(args.Tweet.Text);
        }
    }
}
