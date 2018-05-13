using System.Threading.Channels;
using System.Threading.Tasks;
using counter.Observers;
using counter.Stats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace counter.Hubs
{
    [Authorize]
    public class StatsHub: Hub 
    {
        BusinessPointStatsChannels _sc;
        public StatsHub(BusinessPointStatsChannels sc)
        {
            _sc = sc;
        }
        public ChannelReader<BusinessPointStats>  Stats()
        {
            return _sc.GetChannel(Context.User.Identity.Name);
        }
    }
}