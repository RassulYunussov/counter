using System;
using counter.Stats;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace counter.Observers
{
    public class StatsObservables
    {
        Dictionary<string,StatsObservable> _observables = new Dictionary<string,StatsObservable>();
        public StatsObservable GetObservable(string user)
        {
           if(_observables.ContainsKey(user))
               return _observables[user];
           else
           {
               _observables.Add(user,new StatsObservable());
               return _observables[user];
           }
        }
    }
    public class StatsObservable : IObservable<BusinessPointStats>
    {
        List<IObserver<BusinessPointStats>> _observers = new List<IObserver<BusinessPointStats>>();
        public IDisposable Subscribe(IObserver<BusinessPointStats> observer)
        {
            _observers.Add(observer);
            return new StatsUnsusbscriber(_observers,observer);
        }
        public void BroadcastStats(string operatorName,int businessPointId, decimal amount)
        {
            Task.Run(()=>{
                var stat = new BusinessPointStats{ BusinessPointId = businessPointId,TotalAmount = amount};
                foreach (var o in _observers)
                {
                    o.OnNext(stat);
                }
            });
        }
    }
    public class StatsUnsusbscriber :IDisposable
    {
        List<IObserver<BusinessPointStats>> _observers;
        IObserver<BusinessPointStats> _observer;
        public StatsUnsusbscriber(List<IObserver<BusinessPointStats>> observers, IObserver<BusinessPointStats> observer)
        {
            _observers = observers;
            _observer = observer;
        }
        public void Dispose()
        {
            _observers.Remove(_observer);
        }
    }
}