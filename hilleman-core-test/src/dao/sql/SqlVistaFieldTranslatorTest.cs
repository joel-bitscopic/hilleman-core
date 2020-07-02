using System;
using NUnit.Framework;

namespace com.bitscopic.hilleman.core.dao.sql
{
    [TestFixture]
    public class SqlVistaFieldTranslatorTest
    {
        String sqlConfDirectory = @"C:\Downstream\TESTING\conf";

        #region Read Tests
        [Test]
        public void testGetSqlAllFields()
        {
            ReadRequest request = new ReadRequest(new domain.SourceSystem() { id = "640" }, "44", "11339,", ".01;2;2.1;3;3.5;7;8;9;9.5;10;24;1912;1913;1914;1915;1916;1917;1918;2001;2002", "");
            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromRead(request);

            Assert.AreEqual(sql, "SELECT IEN, NM_X01, TYP_2, TYP_EXTNSN_2X1, INSTTTN_3, DVSN_3X5, VST_LCTN_7, STP_CD_NMBR_8, SRVC_9, TRTNG_SPCLTY_9X5, PHYSCL_LCTN_10, ASK_FOR_CI_CO_TM_24, LNGTH_OF_APPT_1912, VRBL_LNGTH_APPT_1913, HR_CLNC_DSPLY_BGNS_1914, HLD_SC_SLTS_DAYS_1915, PRNCPL_CLNC_1916, DSPLY_INCRMNTS_PER_HR_1917, OVRBKS_DAY_MXMM_1918, ALLWBL_CNSCTV_NO_SHWS_2001, MAX_NMBR_DYS_FTR_BKNG_2002 FROM HSPTL_LCTN_44 WHERE IEN='11339' AND SITECODE='640'");
        }

        [Test]
        public void testGetSqlSubsetOfFields()
        {
            ReadRequest request = new ReadRequest(new domain.SourceSystem() { id = "640" }, "44", "11339,", ".01;1912;1913;1914;1915;1916;1917;1918", "");
            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromRead(request);

            Assert.AreEqual(sql, "SELECT IEN, NM_X01, LNGTH_OF_APPT_1912, VRBL_LNGTH_APPT_1913, HR_CLNC_DSPLY_BGNS_1914, HLD_SC_SLTS_DAYS_1915, PRNCPL_CLNC_1916, DSPLY_INCRMNTS_PER_HR_1917, OVRBKS_DAY_MXMM_1918 FROM HSPTL_LCTN_44 WHERE IEN='11339' AND SITECODE='640'");
        }

        [Test]
        public void testGetSqlSubfile()
        {
            ReadRequest request = new ReadRequest(new domain.SourceSystem() { id = "640" }, "44.003", "1,3150701.14,11339,", ".01;1;5;7;8;9;302;303;304;305;306;309;310;688", "");
            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromRead(request);

            Assert.AreEqual(sql, "SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, PTNT_X01, LNGTH_OF_APPT_1, STTS_5, DATA_ENTRY_CLRK_7, DT_APPT_MD_8, OVERBOOK_9, CHCK_IN_USR_302, CHCKD_OUT_303, CHCK_OUT_USR_304, CHCK_IN_ENTRD_305, CHECK_OUT_ENTRD_306, CHCKD_IN_309, APPT_CNCLLD_310, CNSLT_LNK_688 FROM HSPTL_LCTN_APPTS_PTNT_44X003 WHERE IEN='1_3150701.14_11339' AND SITECODE='640'");
        }

        [Test]
        public void testGetSqlSubfileSubsetOfFields()
        {
            ReadRequest request = new ReadRequest(new domain.SourceSystem() { id = "640" }, "44.003", "1,3150701.14,11339,", ".01;1;5;7;8;9;688", "");
            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromRead(request);

            Assert.AreEqual(sql, "SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, PTNT_X01, LNGTH_OF_APPT_1, STTS_5, DATA_ENTRY_CLRK_7, DT_APPT_MD_8, OVERBOOK_9, CNSLT_LNK_688 FROM HSPTL_LCTN_APPTS_PTNT_44X003 WHERE IEN='1_3150701.14_11339' AND SITECODE='640'");
        }
        #endregion

        #region Read Range Tests
        [Test]
        public void testGetRangeTopLevelFile()
        {
            ReadRangeRequest request = new ReadRangeRequest(new domain.SourceSystem() { id = "640" });
            request.setFields(".01");
            request.setFile("44");
            request.setFrom("1000");
            request.setMax("10");

            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromReadRange(request);

            Assert.AreEqual(sql, "SELECT IEN, NM_X01 FROM (SELECT * FROM HSPTL_LCTN_44 ORDER BY cast(IEN as number)) WHERE SITECODE='640' AND cast(IEN as number)>1000 LIMIT 10");
        }

        [Test]
        public void testGetRangeTopLevelFileBXref()
        {
            ReadRangeRequest request = new ReadRangeRequest(new domain.SourceSystem() { id = "640" });
            request.setFields(".01");
            request.setFile("44");
            request.setFrom("CARDIO"); // set proper FROM arg for B xref
            request.setMax("10");
            request.setCrossRef("B"); // set B xref

            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromReadRange(request);
            Assert.AreEqual(sql, "SELECT IEN, NM_X01 FROM (SELECT * FROM HSPTL_LCTN_44 ORDER BY NM_X01) WHERE SITECODE='640' AND NM_X01>'CARDIO' LIMIT 10");
        }


        [Test]
        public void testGetRangeSubFile()
        {
            ReadRangeRequest request = new ReadRangeRequest(new domain.SourceSystem() { id = "640" });
            request.setFields(".01");
            request.setFile("44.001");
            request.setIens("6861");
            request.setFrom("3150710");
            request.setMax("10");

            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromReadRange(request);

            Assert.AreEqual(sql, "SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, APPT_DT_TM_X01 FROM (SELECT * FROM HSPTL_LCTN_APPTS_44X001 WHERE IEN LIKE('%_6861') ORDER BY cast(substr(IEN, 0, instr(IEN, '_')) AS number)) WHERE IEN LIKE('%_6861') AND SITECODE='640' AND cast(substr(IEN, 0, instr(IEN, '_')) AS number)>3150710 LIMIT 10");

            // WORKS!!!
            //System.Data.DataTable resultFromSql = new sqlite.SqliteDao(@"Data Source=C:\Downstream\sc_appts2.sqlite").executeSql(sql);
            //Assert.AreEqual(resultFromSql.Rows.Count, 10);
            //foreach (System.Data.DataRow row in resultFromSql.Rows)
            //{
            //    foreach (object obj in row.ItemArray)
            //    {
            //        System.Console.Write(obj + "\t");
            //    }
            //    System.Console.WriteLine();
            //}
        }

        [Test]
        public void testGetRangeSubFileBXref()
        {
            ReadRangeRequest request = new ReadRangeRequest(new domain.SourceSystem() { id = "640" });
            request.setFields(".01");
            request.setFile("44.001");
            request.setIens("6861");
            request.setFrom("3150710"); // set FROM for B cross ref
            request.setMax("10");
            request.setCrossRef("B");

            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromReadRange(request);

            Assert.AreEqual(sql, "SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, APPT_DT_TM_X01 FROM (SELECT * FROM HSPTL_LCTN_APPTS_44X001 WHERE IEN LIKE('%_6861') ORDER BY APPT_DT_TM_X01) WHERE IEN LIKE('%_6861') AND SITECODE='640' AND APPT_DT_TM_X01>'3150710' LIMIT 10");
        }

        [Test]
        public void testGetRangeSubFileOfSubFile()
        {
            ReadRangeRequest request = new ReadRangeRequest(new domain.SourceSystem() { id = "640" });
            request.setFields(".01;1;7;8");
            request.setFile("44.003");
            request.setIens("3150709.13,6861");

            String sql = new SqlVistaFieldTranslator(sqlConfDirectory).getSqlQueryFromReadRange(request);

            Assert.AreEqual(sql, "SELECT substr(IEN, 0, instr(IEN, '_')) AS IEN, PTNT_X01, LNGTH_OF_APPT_1, DATA_ENTRY_CLRK_7, DT_APPT_MD_8 FROM (SELECT * FROM HSPTL_LCTN_APPTS_PTNT_44X003 WHERE IEN LIKE('%_3150709.13_6861') ORDER BY cast(substr(IEN, 0, instr(IEN, '_')) AS number)) WHERE IEN LIKE('%_3150709.13_6861') AND SITECODE='640' AND cast(substr(IEN, 0, instr(IEN, '_')) AS number)>0");

            // WORKS!!!
            //System.Data.DataTable resultFromSql = new sqlite.SqliteDao(@"Data Source=C:\Downstream\sc_appts2.sqlite").executeSql(sql);
            //Assert.AreEqual(resultFromSql.Rows.Count, 5);
            //foreach (System.Data.DataRow row in resultFromSql.Rows)
            //{
            //    foreach (object obj in row.ItemArray)
            //    {
            //        System.Console.Write(obj + "\t");
            //    }
            //    System.Console.WriteLine();
            //}
        }

        #endregion

    }
}
