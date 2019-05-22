using System;
using System.Collections.Generic;
using Demo.Domain.Events;
using EventStore;

namespace Demo.Domain
{
    public class Meter
    {
        public string MeterId { get; private set; }

        public string PostalCode { get; private set; }

        public string HouseNumber { get; private set; }

        public bool IsActivated { get; private set; }

        public int FailedActivationAttempts { get; private set; }

        internal int Version { get; private set; }

        internal List<IEvent> Changes { get; } = new List<IEvent>();

        private string _activationCode;

        public Meter(string meterId, string postalCode, string houseNumber, string activationCode)
        {
            Apply(new MeterRegistered
            {
                MeterId = meterId,
                PostalCode = postalCode,
                HouseNumber = houseNumber,
                ActivationCode = activationCode
            });
        }

        public Meter(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            { 
                Mutate(@event);
                Version += 1;
            }
        }

        public bool Activate(string activationCode)
        {
            if (IsActivated) throw new InvalidOperationException("Already activated.");

            if (activationCode == _activationCode)
            {
                Apply(new MeterActivated());
                return true;
            }
            else
            {
                Apply(new MeterActivationFailed { ActivationCode = activationCode });
                return false;
            }
        }

        private void Apply(IEvent @event)
        {
            Changes.Add(@event);
            Mutate(@event);
        }

        private void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }

        private void When(MeterRegistered @event)
        {
            MeterId = @event.MeterId;
            PostalCode = @event.PostalCode;
            HouseNumber = @event.HouseNumber;
            _activationCode = @event.ActivationCode;
        }

        private void When(MeterActivated @event)
        {
            IsActivated = true;
        }

        private void When(MeterReadingsCollected @event)
        {
        }

        private void When(MeterActivationFailed @event)
        {
            FailedActivationAttempts += 1;
        }
    }
}