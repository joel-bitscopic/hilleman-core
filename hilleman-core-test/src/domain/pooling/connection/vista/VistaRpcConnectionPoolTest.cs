using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.dao.vista.rpc;
using System.Threading.Tasks;
using com.bitscopic.hilleman.core.refactoring;
using com.bitscopic.hilleman.core.dao;
using System.Collections.Concurrent;
using System.Threading;
using System.Configuration;
using com.bitscopic.hilleman.core.domain.session;

namespace com.bitscopic.hilleman.core.domain.pooling.connection.vista
{
    [TestFixture]
    public class VistaRpcConnectionPoolTest
    {
        [Test]
        public void testConnectionPools()
        {
            VistaRpcConnectionPoolsSource poolsSource = new VistaRpcConnectionPoolsSource();
            poolsSource.CxnSources = new Dictionary<string, VistaRpcConnectionPoolSource>();
            SourceSystemTable srcTable = new SourceSystemTable(MyConfigurationManager.getValue("SourceSystemTable"));

            VistaRpcConnectionPoolSource cpm = new VistaRpcConnectionPoolSource()
            {
                Timeout = new TimeSpan(0, 0, 5),
                WaitTime = new TimeSpan(0, 0, 5),
                MaxPoolSize = 1,
                MinPoolSize = 1,
                PoolExpansionSize = 1,
                CxnSource = srcTable.getSourceSystem("901"),
                Credentials = new VistaRpcLoginCredentials()
                {
                    username = "01vehu",
                    password = "vehu01"
                }
            };
            VistaRpcConnectionPoolSource dewdrop = new VistaRpcConnectionPoolSource()
            {
                Timeout = new TimeSpan(0, 0, 5),
                WaitTime = new TimeSpan(0, 0, 5),
                MaxPoolSize = 1,
                MinPoolSize = 1,
                PoolExpansionSize = 1,
                CxnSource = srcTable.getSourceSystem("101"),
                Credentials = new VistaRpcLoginCredentials()
                {
                    username = "worldvista6",
                    password = "$#happy7"
                }
            };

            poolsSource.CxnSources.Add("901", cpm);
            poolsSource.CxnSources.Add("101", dewdrop);

            VistaRpcConnectionPools pools = (VistaRpcConnectionPools)new VistaRpcConnectionPoolFactory().getResourcePool(poolsSource);

            System.Threading.Thread.Sleep(2000);

            VistaRpcConnection cxn = (VistaRpcConnection)pools.checkOutAlive("901"); 
            // do something with connection to verify it's connected
            new VistaRpcCrrudDao(cxn).heartbeat();

            PatientSearchDao patientSearch = new PatientSearchDao(null);
            patientSearch.setTarget(cxn);

            Assert.IsTrue(patientSearch.searchForPatient("SMITH").Count > 0);

            // now try to checkout another connection - it should fail because we specified only 1 connection for max pool size
            VistaRpcConnection shouldFail = null;
            try
            {
                shouldFail = (VistaRpcConnection)pools.checkOutAlive("901");
                // if succeeds - immediately check cxns back in and shutdown gracefully before we fail the test
                pools.checkIn(shouldFail);
                pools.checkIn(cxn);
                pools.shutdown();
                Assert.Fail("Second checkout attempt should have failed!!");
            }
            catch (TimeoutException)
            {
                // cool!
            }
            catch (Exception other)
            {
                Assert.Fail(other.Message);
            }

            pools.checkIn(cxn);

            // put cxn back in pool - now check out should succeed
            cxn = (VistaRpcConnection)pools.checkOutAlive("901");
            pools.checkIn(cxn);

            pools.shutdown();
        }

        [Test]
        public void testConnectionPool()
        {
            VistaRpcConnectionPool pool = new VistaRpcConnectionPool();
            pool.PoolSource = new VistaRpcConnectionPoolSource()
            {
                CxnSource = new SourceSystem() { id = "500", type = SourceSystemType.VISTA_RPC_BROKER, name = "Dewdrop", connectionString = "192.168.2.110:9430" },
                Credentials = new VistaRpcLoginCredentials() { username = "worldvista6", password = "$#happy7" },
                BrokerContext = new VistaRpcConnectionBrokerContext("", "DVBA CAPRI GUI")
            };
            pool.PoolSource.MaxPoolSize = 1;
            pool.PoolSource.MinPoolSize = 1;
            pool.PoolSource.PoolExpansionSize = 1;
            pool.PoolSource.WaitTime = new TimeSpan(0, 0, 2); // set to short amount of time so unit test finishes quickly
            Task threadRunner = new Task(() => pool.run());
            threadRunner.Start();

            System.Threading.Thread.Sleep(2000);

            VistaRpcConnection cxn = (VistaRpcConnection)pool.checkOutAlive(null); // note this is a single connection pool so we know what we're fetching (i.e. don't need to pass site ID)
            // do something with connection to verify it's connected
            new VistaRpcCrrudDao(cxn).heartbeat();
            Assert.IsTrue(new PatientSearchDao(cxn).searchForPatient("SMITH").Count > 0);

            // now try to checkout another connection - it should fail because we specified only 1 connection for max pool size
            VistaRpcConnection shouldFail = null;
            try
            {
                shouldFail = (VistaRpcConnection)pool.checkOutAlive(null);
                // if succeeds - immediately check cxns back in and shutdown gracefully before we fail the test
                pool.checkIn(shouldFail);
                pool.checkIn(cxn);
                pool.shutdown();
                Assert.Fail("Second checkout attempt should have failed!!");
            }
            catch (TimeoutException)
            {
                // cool!
            }
            catch (Exception other)
            {
                Assert.Fail(other.Message);
            }

            pool.checkIn(cxn);

            // put cxn back in pool - now check out should succeed
            cxn = (VistaRpcConnection)pool.checkOutAlive(null); // note this is a single connection pool so we know what we're fetching (i.e. don't need to pass site ID)
            pool.checkIn(cxn);

            pool.shutdown();
        }

        [Test]
        public void testVistaRpcConnectionPoolPerformance()
        {
            VistaRpcConnectionPool pool = new VistaRpcConnectionPool();
            pool.PoolSource = new VistaRpcConnectionPoolSource()
            {
                CxnSource = new SourceSystem() { id = "500", type = SourceSystemType.VISTA_RPC_BROKER, name = "Dewdrop", connectionString = "192.168.2.110:9430" },
                Credentials = new VistaRpcLoginCredentials() { username = "worldvista6", password = "$#happy7" },
                BrokerContext = new VistaRpcConnectionBrokerContext("", "DVBA CAPRI GUI" )
            };
            pool.PoolSource.MaxPoolSize = 8;
            pool.PoolSource.MinPoolSize = 1;
            pool.PoolSource.PoolExpansionSize = 2;
            pool.PoolSource.WaitTime = new TimeSpan(0, 0, 2); // set to short amount of time so unit test finishes quickly
            Task threadRunner = new Task(() => pool.run());
            threadRunner.Start();

            System.Threading.Thread.Sleep(2000);

            Int32 queryQueueCount = 50000;
            Int32 workerThreads = 8;

            ConcurrentBag<Task> tasks = new ConcurrentBag<Task>();
            while (tasks.Count < queryQueueCount)
            {
                tasks.Add(new Task(() => makeDdrGetsEntryQuery(pool)));
            }

            System.Console.WriteLine(String.Format("Starting {0} worker threads for {1} tasks", workerThreads.ToString(), queryQueueCount.ToString()));
            DateTime start = DateTime.Now;
            runWorkerTasks(workerThreads, tasks);
            TimeSpan elapsed = DateTime.Now.Subtract(start);
            System.Console.WriteLine("Finished all tasks in " + elapsed.ToString());

            pool.shutdown();
        }

        void runWorkerTasks(Int32 numThreads, ConcurrentBag<Task> tasks)
        {
            IList<Thread> workerThreads = new List<Thread>();

            for (int i = 0; i < numThreads; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(takeTask));
                t.Start(tasks);
                workerThreads.Add(t);
            }

            foreach (Thread t in workerThreads)
            {
                t.Join();
            }
        }

        void makeDdrGetsEntryQuery(VistaRpcConnectionPool poolForCxn)
        {
            VistaRpcConnection theCxn = null;
            try
            {
                theCxn = (VistaRpcConnection)poolForCxn.checkOutAlive(null);
                ReadRequest request = new ReadRequest(theCxn.getSource());
                request.setFile("9999999.14");
                request.setIens("129,");
                request.setFlags("IEN");
                request.setFields("*");
                ReadResponse response = new VistaRpcCrrudDao(theCxn).read(request);
                if (response.value.Count < 2)
                {
                    throw new exception.HillemanBaseException("Bad result from read");
                }
            }
            catch (Exception exc)
            {
                System.Console.WriteLine("Problem with query: " + exc.Message);
            }
            finally
            {
                poolForCxn.checkIn(theCxn);
            }
        }

        public void takeTask(object concurrentBagOfTasks)
        {
            ConcurrentBag<Task> tasks = (ConcurrentBag<Task>)concurrentBagOfTasks;
            Task current = null;
            while (tasks.TryTake(out current))
            {
                current.Start();
                current.Wait();
            }
        }
    }
}
