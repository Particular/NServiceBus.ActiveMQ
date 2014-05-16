namespace NServiceBus.Transports.ActiveMQ
{
    using System;
    using Apache.NMS;

    public class ActiveMqSchedulerManagementJob
    {
        public ActiveMqSchedulerManagementJob(IMessageConsumer consumer, IDestination temporaryDestination, DateTime expirationDate)
        {
            Consumer = consumer;
            Destination = temporaryDestination;
            ExpirationDate = expirationDate;
        }

        public IMessageConsumer Consumer { get; set; }
        public IDestination Destination { get; set; }

        [Obsolete]
        // ReSharper disable once IdentifierTypo
        public DateTime ExprirationDate
        {
            get { return ExpirationDate; }
            set { ExpirationDate = value; }
        }

        public DateTime ExpirationDate { get; set; }
    }
}