using NLua;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace IoT_Server.Modules
{
    class Timer
    {
        static List<TimerInstance> Timers = new List<TimerInstance>();
        static List<TimerInstance> timers = new List<TimerInstance>();

        public static Timer singleton = new Timer();

        Timer()
        {
            Thread timersThread = new Thread(MainThreadCheck);
            timersThread.Start();
        }

        private void MainThreadCheck()
        {
            try
            {
                while(true)
                {
                    if (timers.Count > 0)
                    {
                        var now = DateTime.UtcNow.TimeOfDay.TotalSeconds;

                        var orderedInvoke = timers.OrderBy(t => t.nextInvokeTime).ToList();
                        for (int i = 0; i < orderedInvoke.Count; i++)
                        {
                            var t = orderedInvoke[i];
                            if (now >= t.nextInvokeTime)
                            {
                                t.Invoke();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public LuaTimerInstance Create(string id, float delay, int repeations, LuaFunction func)
        {
            var timer = new LuaTimerInstance(id, delay, repeations, func);
            Timers.Add(timer);
            timer.Start();
            return timer;
        }

        public void Remove(string id)
        {
            TimerInstance timer = Timers.Find(timer => timer.id == id);
            timer.Stop();
            Timers.Remove(timer);
        }

        public bool Exists(string id)
        {
            return Timers.Exists(timer => timer.id == id);
        }

        public void Simple(float delay, LuaFunction func)
        {
            var timer = new LuaTimerInstance("simple_" + DateTime.UtcNow.TimeOfDay.TotalSeconds, delay, 1, func);
            Timers.Add(timer);
            timer.Start();
        }

        public class LuaTimerInstance : TimerInstance
        {
            new internal LuaFunction function { get; set; }

            public LuaTimerInstance(string id, float delay, int repeations, LuaFunction function) : base(id, delay, repeations)
            {
                this.function = function;
            }

            public override void Invoke()
            {
                try
                {
                    function.Call();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                if (loopRemaining != -1)
                {
                    loopRemaining--;
                }
                if (loopRemaining > 0 || loopRemaining == -1)
                {
                    var now = (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
                    nextInvokeTime = now + ((float)delay);
                }
                else
                {
                    Stop();
                }
            }
        }

        public class TimerInstance
        {
            public string id;
            protected float delay;
            protected int repeations;
            protected int loopRemaining;
            protected bool enabled;

            protected Action function;

            internal float nextInvokeTime;

            protected TimerInstance(string id, float delay, int repeations)
            {
                this.id = id;
                this.delay = delay;
                this.repeations = repeations;
                this.loopRemaining = repeations;
            }

            public void Start()
            {
                if (enabled) Stop();
                var now = DateTime.UtcNow.TimeOfDay.TotalSeconds;
                nextInvokeTime = (float)now + ((float)delay);
                enabled = true;
                timers.Add(this);
            }

            public void Stop()
            {
                enabled = false;
                timers.Remove(this);
            }

            public virtual void Invoke()
            {

            }
        }
    }
}
