using System;
using System.Collections.Generic;
using System.Text;

namespace Su
{
    public class LockerProvider
    {
        public class Locker
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }

        static Dictionary<string, Locker> _dicLocker = new Dictionary<string, Locker>();

        public static Locker GetLocker(string name)
        {
            if (! _dicLocker.ContainsKey(name))
            {
                lock (_dicLocker)
                {
                    if (!_dicLocker.ContainsKey(name))
                    {
                        var locker = new Locker();
                        locker.Name = name;
                        locker.Content = DateTime.Now.ISO8601();

                        _dicLocker.Add(name, locker);
                    }
                }
            }

            return _dicLocker[name];
        }
    }
}
