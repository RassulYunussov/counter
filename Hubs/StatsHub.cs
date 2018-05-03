using counter.Observers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace counter.Hubs
{
    [Authorize]
    public class StatsHub: Hub 
    {
        StatsObservables _so;
        public StatsHub(StatsObservables so)
        {
            _so = so;
        }
        public StatsObservable Stats()
        {
            return _so.GetObservable(Context.User.Identity.Name);
        }
    }
}