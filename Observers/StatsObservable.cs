using System;
using counter.Stats;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace counter.Observers
{
    public class BusinessPointStatsChannels
    {
         Dictionary<string,Channel<BusinessPointStats>> _channels = new Dictionary<string,Channel<BusinessPointStats>>();
         public Channel<BusinessPointStats> GetChannel(string user)
         {
            if(_channels.ContainsKey(user))
               return _channels[user];
            else
            {
                _channels.Add(user,Channel.CreateUnbounded<BusinessPointStats>());
                return _channels[user];
            }
         }
    }
}