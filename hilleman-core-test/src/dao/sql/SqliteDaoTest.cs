using System;
using System.Collections.Generic;
using NUnit.Framework;
using com.bitscopic.hilleman.core.domain;
using com.bitscopic.hilleman.core.refactoring;
using com.bitscopic.hilleman.core.dao.vista;
using com.bitscopic.hilleman.core.dao.vista.sql.sqlite;

namespace com.bitscopic.hilleman.core.dao.sql.sqlite
{
    [TestFixture]
    public class SqliteDaoTest
    {
        SourceSystem _src = new SourceSystem()
        {
            connectionString = @"Data Source=C:\Downstream\sc_appts2.sqlite",
            id = "640",
            name = "SQLite Extract",
            type = SourceSystemType.SQLITE_CACHE
        };

        [Test]
        public void testReadTopLevel()
        {
            ReadRequest request = new ReadRequest(_src, "44", "6861", ".01", "");
            ICrrudDao dao = new CrrudDaoFactory().getCrrudDao(_src);
            ReadResponse response = dao.read(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.convertResponseToInternalDict().Keys.Count > 0);
        }


        [Test]
        public void testReadSubFile()
        {
            ReadRequest request = new ReadRequest(_src, "44.001", "3150710.09,6861", ".01", "");
            ICrrudDao dao = new CrrudDaoFactory().getCrrudDao(_src);
            ReadResponse response = dao.read(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.convertResponseToInternalDict().Keys.Count > 0);
        }


        [Test]
        public void testReadSubFileOfSubFile()
        {
            ReadRequest request = new ReadRequest(_src, "44.003", "1,3150710.09,6861", ".01;1;7;8", "");
            ICrrudDao dao = new CrrudDaoFactory().getCrrudDao(_src);
            ReadResponse response = dao.read(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.convertResponseToInternalDict().Keys.Count > 4); // 4 fields requested + IEN
        }

        [Test]
        public void testReadUsingSchedulingDaoRequest()
        {
            IVistaConnection sqliteVistaCxn = new VistaSqliteCacheConnection(_src);
            SchedulingDao schedulingDao = new SchedulingDao(sqliteVistaCxn);

            IList<Clinic> clinicsFromSql = schedulingDao.getClinics("DERM", 44);

            Assert.IsNotNull(clinicsFromSql);
            Assert.IsTrue(clinicsFromSql.Count == 44); // 4 fields requested + IEN
        }

    }
}
