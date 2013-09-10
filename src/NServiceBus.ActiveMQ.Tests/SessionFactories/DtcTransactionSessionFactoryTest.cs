﻿namespace NServiceBus.Transports.ActiveMQ.Tests.SessionFactories
{
    using System.Transactions;
    using ActiveMQ.SessionFactories;
    using Apache.NMS;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class DtcTransactionSessionFactoryTest
    {
        private DTCTransactionSessionFactory testee;
        private PooledSessionFactoryMock pooledPooledSessionFactoryMock;

        [SetUp]
        public void SetUp()
        {
            pooledPooledSessionFactoryMock = new PooledSessionFactoryMock();
            testee = new DTCTransactionSessionFactory(pooledPooledSessionFactoryMock);
        }

        [Test]
        public void WhenSessionIsRequested_OneFromThePooledSessionFactoryIsReturned()
        {
            var expectedSessions = pooledPooledSessionFactoryMock.EnqueueNewSessions(1);

            var session = testee.GetSession();

            session.Should().BeSameAs(expectedSessions[0]);
        }

        [Test]
        public void WhenSessionIsReleased_ItIsReturnedToThePooledSessionFactory()
        {
            pooledPooledSessionFactoryMock.EnqueueNewSessions(1);

            var session = testee.GetSession();
            testee.Release(session);

            pooledPooledSessionFactoryMock.sessions.Should().Contain(session);
        }
        
        [Test]
        public void GetSession_WhenInTransaction_ThenSameSessionIsUsed()
        {
            pooledPooledSessionFactoryMock.EnqueueNewSessions(1);

            ISession session1;
            ISession session2;

            using (var tx = new TransactionScope())
            {
                session1 = testee.GetSession();
                testee.Release(session1);

                session2 = testee.GetSession();
                testee.Release(session2);

                tx.Complete();
            }

            session1.Should().BeSameAs(session2);
        }

        [Test]
        public void GetSession_WhenInDifferentTransaction_ThenDifferentSessionAreUsed()
        {
            pooledPooledSessionFactoryMock.EnqueueNewSessions(2);

            ISession session1;
            ISession session2;

            using (var tx1 = new TransactionScope())
            {
                session1 = testee.GetSession();
                testee.Release(session1);

                using (var tx2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    session2 = testee.GetSession();
                    testee.Release(session2);

                    tx2.Complete();
                }

                tx1.Complete();
            }

            session1.Should().NotBeSameAs(session2);
        }

        [Test]
        public void GetSession_WhenInDifferentCompletedTransaction_ThenSessionIsReused()
        {
            pooledPooledSessionFactoryMock.EnqueueNewSessions(1);

            ISession session1;
            ISession session2;
            using (var tx1 = new TransactionScope())
            {
                session1 = testee.GetSession();
                testee.Release(session1);

                tx1.Complete();
            }

            using (var tx2 = new TransactionScope())
            {
                session2 = testee.GetSession();
                testee.Release(session2);

                tx2.Complete();
            }

            session1.Should().BeSameAs(session2);
        }
    }
}
