using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quanto.Printer;

namespace Quanto.SMS
{
    public class SMSManager
    {
        private static SMSManager instance;
        public static SMSManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SMSManager();
                return instance;
            }
        }

        MessageQueue<SMS> delayqueue;
        MessageQueue<SMS> senderqueue;
        MessageQueue<SMS> notifierqueue;
        bool hasstopped = false;
        System.Threading.Thread senderthread;
        System.Threading.Thread notifierthread;
        System.Threading.Thread delaythread;
        System.Threading.AutoResetEvent are;

        System.Collections.Concurrent.ConcurrentDictionary<long, List<String>> confirmedMessages = new System.Collections.Concurrent.ConcurrentDictionary<long, List<string>>();
        public void Start()
        {
            are = new System.Threading.AutoResetEvent(false);
            senderqueue = new MessageQueue<SMS>(int.MaxValue);
            notifierqueue = new MessageQueue<SMS>(int.MaxValue);
            delayqueue = new MessageQueue<Quanto.SMS.SMS>(int.MaxValue);

            senderthread = new System.Threading.Thread(new System.Threading.ThreadStart(Sender));
            senderthread.Start();

            notifierthread = new System.Threading.Thread(new System.Threading.ThreadStart(Notifier));
            notifierthread.Start();

            delaythread = new System.Threading.Thread(new System.Threading.ThreadStart(Delayer));
            delaythread.Start();
        }

        public void Stop()
        {
            hasstopped = true;
            are.Set();
            System.Threading.Thread.Sleep(1000);
            foreach (var entity in senderqueue.GetAll())
            {
                bool result = SMSService.Instance.Send(entity);
                notifierqueue.Enqueue(entity);
            }
            foreach (var entity in notifierqueue.GetAll())
            {
                bool result = SMSService.Instance.Notify(entity);
            }
        }

        public void Delayer()
        {
            System.Threading.Thread.Sleep(100);
            SMS entity;
            List<String> mobileList=null;
            while (delayqueue.Dequeue(out entity))
            {
                Quanto.Logger.Current.InfoFormat("Delay message {0}", entity.message);
                while (entity.scheduleto>DateTime.Now)
                {
                    System.Threading.Thread.Sleep(5 * 1000);
                }
                if(confirmedMessages.TryGetValue(entity.id, out mobileList))
                {
                    Quanto.Logger.Current.InfoFormat("Delay message before purging {0}", entity.items.Count);
                    entity.items.RemoveAll(e => mobileList.Contains(e.mobile));
                    Quanto.Logger.Current.InfoFormat("Delay message aftger purging {0}", entity.items.Count);
                }
                if(entity.items.Count>0)
                    senderqueue.Enqueue(entity);
            }
        }

        public void Sender()
        {
            System.Threading.Thread.Sleep(100);
            SMS entity;
            while(senderqueue.Dequeue(out entity))
            {
                bool result = SMSService.Instance.Send(entity);
                notifierqueue.Enqueue(entity);
                if (are.WaitOne(10))
                {
                    are.Set();
                    break;
                }
            }
        }

        public bool KillSMS(string messageid,string mobile)
        {
            long id;
            if (long.TryParse(messageid, out id))
            {
                List<String> mobileList = null;
                if (confirmedMessages.TryGetValue(id, out mobileList))
                {
                    mobileList.Add(mobile);
                    return true;
                }
            }
            return false;
        }
        internal bool Sent(SMS document)
        {
            if (hasstopped)
                return false;
            SMSService.Instance.Persist(document);
            if (document.isdirect)
            {
                return senderqueue.Enqueue(document);
            }
            else
            {
                Quanto.Logger.Current.InfoFormat("Delay message added {0} - {1} - {2}", document.id, document.message, document.items.Count);
                confirmedMessages.TryAdd(document.id, new List<string>());
                document.scheduleto = DateTime.Now.AddMinutes(5);
                return delayqueue.Enqueue(document);
            }
        }

        public void Notifier()
        {
            System.Threading.Thread.Sleep(100);
            SMS entity;
            while (notifierqueue.Dequeue(out entity))
            {
                bool result = SMSService.Instance.Notify(entity);
                if (are.WaitOne(10))
                {
                    are.Set();
                    break;
                }
            }
        }
    }
}
