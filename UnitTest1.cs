using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
    public enum ContractModelType
    {
        None = 0,
        TZV_Month = 1,
        TZV_Day = 2,
        TZV_VariableBlock = 7,
        TZV_VariableDay = 8,
        TZV_FixBlock = 9,
        TZV_FixDay = 10,
        FL_A = 3,
        FL_B = 4,
        FL_C = 5,
        FL_S = 6
    }

    public enum ContractType
    {
        None = 0,
        Fix = 1,
        Freelance = 2,
        Fse = 3,
        Tzv = 4
    }

    public class Tests
    {
        public static List<ContractRange> GetPersonContracts(params string[] contracts)
        {
            return contracts.Select(x => x.Split(' ')).Select(r => new ContractRange
            {
                DateFrom = DateTime.Parse(r[0]),
                DateTo = DateTime.Parse(r[2]),
                ContractType = (r[3] == "TZV"
                    ? ContractType.Tzv
                    : r[3] == "Fix" ? ContractType.Fix : ContractType.Freelance)
            }).ToList();
        }

        public static List<ContractRange> GetPersonContractsWithYearOfService(params string[] contracts)
        {
            return contracts.Select(x => x.Split(' ')).Select(r => new ContractRange
            {
                DateFrom = DateTime.Parse(r[0]),
                DateTo = DateTime.Parse(r[2]),
                ContractType = (ContractType)(r[3] == "Fix" ? 1 : 2),
                YearsOfServiceFreelance = int.Parse(r[4]),
                YearsOfServiceTotal = int.Parse(r[5])
            }).ToList();
        }

        public interface IRange
        {
            DateTime DateFrom { get; set; }
            DateTime DateTo { get; set; }
            int Days { get; }
        }

        public class Range : IRange
        {
            public DateTime DateFrom { get; set; }
            public DateTime DateTo { get; set; }
            public int Days => (int)(DateTo - DateFrom).TotalDays + 1;
        }

        [DebuggerDisplay("{DateFrom.ToString(\"yyyy-MM-dd\")} - {DateTo.ToString(\"yyyy-MM-dd\")}, {FunctionName}")]
        public class FunctionRange : Range
        {
            public Guid PersonId { get; set; }
            public bool IsCockpit { get; set; }
            public Guid FunctionId { get; set; }
            public Guid? FunctionGroupId { get; set; }
            public string FunctionName { get; set; }
        }

        [DebuggerDisplay("{DateFrom.ToString(\"yyyy-MM-dd\")} - {DateTo.ToString(\"yyyy-MM-dd\")}, {ContractType}")]
        public class ContractRange : Range
        {
            public Guid Id { get; set; }
            public decimal? OfficePercentage { get; set; }
            public decimal? PartTimePercentage { get; set; }
            public Guid PersonId { get; set; }
            public ContractType ContractType { get; set; }
            public ContractModelType TypeModel { get; set; }
            public int? PartTimeFixDaysPerWeek { get; set; }
            public int YearsOfServiceFreelance { get; set; }
            public int YearsOfServiceTotal { get; set; }

            public bool IsFreelancer => ContractType == ContractType.Freelance;

            public ContractRange Clone()
            {
                return this;
            }
        }

        #region TestDataForBreakContractsByYearOfService
        private static List<ContractRange> Contracts_1 => GetPersonContracts(
            "2017-01-01 - 2017-01-10 Freelance",
            "2017-01-11 - 2017-01-20 Fix",
            "2018-01-01 - 2018-12-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService1 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-01-10 Freelance 1 1",
            "2017-01-11 - 2017-01-20 Fix 1 1",
            "2018-01-01 - 2018-12-21 Freelance 1 2",
            "2018-12-22 - 2018-12-31 Freelance 2 2");

        private static List<ContractRange> Contracts_2 => GetPersonContracts(
           "2017-01-01 - 2018-03-10 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService2 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-12-31 Freelance 1 1",
            "2018-01-01 - 2018-03-10 Freelance 2 2");

        private static List<ContractRange> Contracts_3 => GetPersonContracts(
            "2019-01-01 - 2019-12-15 Freelance",
            "2019-12-16 - 2020-01-10 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService3 => GetPersonContractsWithYearOfService(
            "2019-01-01 - 2019-12-15 Freelance 1 1",
            "2019-12-16 - 2019-12-31 Freelance 1 1",
            "2020-01-01 - 2020-01-10 Freelance 2 2");

        private static List<ContractRange> Contracts_4 => GetPersonContracts(
            "2017-01-01 - 2020-01-10 Freelance",
            "2020-01-15 - 2020-12-15 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService4 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-12-31 Freelance 1 1",
            "2018-01-01 - 2018-12-31 Freelance 2 2",
            "2019-01-01 - 2019-12-31 Freelance 3 3",
            "2020-01-01 - 2020-01-10 Freelance 4 4",
            "2020-01-15 - 2020-12-15 Fix 4 4");

        private static List<ContractRange> Contracts_5 => GetPersonContracts(
            "2017-01-01 - 2017-01-10 Fix",
            "2017-01-11 - 2019-10-15 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService5 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-01-10 Fix 0 1",
            "2017-01-11 - 2017-12-31 Fix 0 1",
            "2018-01-01 - 2018-12-31 Fix 0 2",
            "2019-01-01 - 2019-10-15 Fix 0 3");

        private static List<ContractRange> Contracts_6 => GetPersonContracts(
            "2017-01-01 - 2017-01-10 Fix",
            "2017-01-11 - 2018-10-15 Freelance",
            "2018-10-16 - 2018-12-30 Fix",
            "2019-01-01 - 2019-12-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService6 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-01-10 Fix 0 1",
            "2017-01-11 - 2018-01-10 Freelance 1 2",
            "2018-01-11 - 2018-10-15 Freelance 2 2",
            "2018-10-16 - 2018-12-30 Fix 2 2",
            "2019-01-01 - 2019-03-28 Freelance 2 3",
            "2019-03-29 - 2019-12-31 Freelance 3 3");

        private static List<ContractRange> Contracts_7 => GetPersonContracts(
            "2017-01-01 - 2017-12-31 Fix",
            "2018-01-01 - 2018-12-31 Freelance",
            "2019-01-01 - 2019-12-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService7 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-12-31 Fix 0 1",
            "2018-01-01 - 2018-12-31 Freelance 1 2",
            "2019-01-01 - 2019-12-31 Freelance 2 3");

        private static List<ContractRange> Contracts_8 => GetPersonContracts(
            "2017-01-01 - 2017-12-31 Fix",
            "2018-01-01 - 2018-01-01 Fix",
            "2019-01-01 - 2019-12-31 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService8 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-12-31 Fix 0 1",
            "2018-01-01 - 2018-01-01 Fix 0 2",
            "2019-01-01 - 2019-12-30 Fix 0 2",
            "2019-12-31 - 2019-12-31 Fix 0 3");

        private static List<ContractRange> Contracts_9 => GetPersonContracts(
            "2017-01-01 - 2017-12-31 Freelance",
            "2018-01-01 - 2018-01-01 Freelance",
            "2019-01-01 - 2019-01-10 Freelance",
            "2019-01-11 - 2019-01-25 Freelance",
            "2019-02-01 - 2020-10-25 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService9 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-12-31 Freelance 1 1",
            "2018-01-01 - 2018-01-01 Freelance 2 2",
            "2019-01-01 - 2019-01-10 Freelance 2 2",
            "2019-01-11 - 2019-01-25 Freelance 2 2",
            "2019-02-01 - 2020-01-05 Fix 2 2",
            "2020-01-06 - 2020-10-25 Fix 2 3");

        private static List<ContractRange> Contracts_10 => GetPersonContracts(
            "2016-01-01 - 2021-12-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService10 => GetPersonContractsWithYearOfService(
            "2016-01-01 - 2016-12-31 Freelance 1 1",
            "2017-01-01 - 2017-12-31 Freelance 2 2",
            "2018-01-01 - 2018-12-31 Freelance 3 3",
            "2019-01-01 - 2019-12-31 Freelance 4 4",
            "2020-01-01 - 2020-12-31 Freelance 5 5",
            "2021-01-01 - 2021-12-31 Freelance 6 6");

        private static List<ContractRange> Contracts_11 => GetPersonContracts(
            "2018-07-01 - 2022-06-30 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService11 => GetPersonContractsWithYearOfService(
            "2018-07-01 - 2019-06-30 Freelance 1 1",
            "2019-07-01 - 2020-06-30 Freelance 2 2",
            "2020-07-01 - 2021-06-30 Freelance 3 3",
            "2021-07-01 - 2022-06-30 Freelance 4 4");

        private static List<ContractRange> Contracts_12 => GetPersonContracts(
            "2020-02-29 - 2022-06-30 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService12 => GetPersonContractsWithYearOfService(
            "2020-02-29 - 2021-02-28 Freelance 1 1",
            "2021-03-01 - 2022-02-28 Freelance 2 2",
            "2022-03-01 - 2022-06-30 Freelance 3 3");

        private static List<ContractRange> Contracts_13 => GetPersonContracts(
            "2019-02-28 - 2021-06-30 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService13 => GetPersonContractsWithYearOfService(
            "2019-02-28 - 2020-02-27 Freelance 1 1",
            "2020-02-28 - 2021-02-27 Freelance 2 2",
            "2021-02-28 - 2021-06-30 Freelance 3 3");

        private static List<ContractRange> Contracts_14 => GetPersonContracts(
            "2018-01-10 - 2021-01-09 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService14 => GetPersonContractsWithYearOfService(
            "2018-01-10 - 2019-01-09 Freelance 1 1",
            "2019-01-10 - 2020-01-09 Freelance 2 2",
            "2020-01-10 - 2021-01-09 Freelance 3 3");

        private static List<ContractRange> Contracts_15 => GetPersonContracts(
            "2020-01-01 - 2020-03-01 Freelance",
            "2020-03-02 - 2022-12-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService15 => GetPersonContractsWithYearOfService(
            "2020-01-01 - 2020-03-01 Freelance 1 1",
            "2020-03-02 - 2020-12-31 Freelance 1 1",
            "2021-01-01 - 2021-12-31 Freelance 2 2",
            "2022-01-01 - 2022-12-31 Freelance 3 3");

        private static List<ContractRange> Contracts_16 => GetPersonContracts(
            "2017-01-01 - 2017-07-19 Freelance",
            "2017-07-20 - 2017-12-31 Freelance",
            "2018-01-01 - 2018-01-02 Freelance",
            "2018-01-03 - 2019-01-01 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService16 => GetPersonContractsWithYearOfService(
            "2017-01-01 - 2017-07-19 Freelance 1 1",
            "2017-07-20 - 2017-12-31 Freelance 1 1",
            "2018-01-01 - 2018-01-02 Freelance 2 2",
            "2018-01-03 - 2018-12-31 Fix 2 2",
            "2019-01-01 - 2019-01-01 Fix 2 3");

        private static List<ContractRange> Contracts_17 => GetPersonContracts(
            "2020-01-01 - 2020-07-19 Freelance",
            "2020-07-20 - 2020-10-31 Freelance",
            "2020-11-01 - 2020-12-21 Freelance",
            "2020-12-22 - 2020-12-31 Freelance",
            "2021-01-01 - 2021-01-02 Freelance",
            "2021-01-03 - 2022-01-01 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService17 => GetPersonContractsWithYearOfService(
            "2020-01-01 - 2020-07-19 Freelance 1 1",
            "2020-07-20 - 2020-10-31 Freelance 1 1",
            "2020-11-01 - 2020-12-21 Freelance 1 1",
            "2020-12-22 - 2020-12-31 Freelance 1 1",
            "2021-01-01 - 2021-01-02 Freelance 2 2",
            "2021-01-03 - 2021-12-31 Fix 2 2",
            "2022-01-01 - 2022-01-01 Fix 2 3");

        private static List<ContractRange> Contracts_18 => GetPersonContracts(
            "2020-01-01 - 2020-12-31 Freelance",
            "2021-01-01 - 2021-12-31 Freelance",
            "2022-01-01 - 2022-12-31 Freelance",
            "2023-01-01 - 2023-12-31 Fix",
            "2024-01-01 - 2025-12-31 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService18 => GetPersonContractsWithYearOfService(
            "2020-01-01 - 2020-12-31 Freelance 1 1",
            "2021-01-01 - 2021-12-31 Freelance 2 2",
            "2022-01-01 - 2022-12-31 Freelance 3 3",
            "2023-01-01 - 2023-12-31 Fix 3 4",
            "2024-01-01 - 2024-12-31 Fix 3 5",
            "2025-01-01 - 2025-12-31 Fix 3 6");

        private static List<ContractRange> Contracts_19 => GetPersonContracts(
            "2019-03-01 - 2020-02-28 Freelance",
            "2020-03-01 - 2024-02-24 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService19 => GetPersonContractsWithYearOfService(
            "2019-03-01 - 2020-02-28 Freelance 1 1",
            "2020-03-01 - 2021-02-28 Freelance 2 2",
            "2021-03-01 - 2022-02-28 Freelance 3 3",
            "2022-03-01 - 2023-02-28 Freelance 4 4",
            "2023-03-01 - 2024-02-24 Freelance 5 5");

        private static List<ContractRange> Contracts_20 => GetPersonContracts(
            "2018-05-01 - 2018-05-01 Freelance",
            "2020-01-01 - 2023-12-30 Fix");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService20 => GetPersonContractsWithYearOfService(
            "2018-05-01 - 2018-05-01 Freelance 1 1",
            "2020-01-01 - 2020-12-30 Fix 1 1",
            "2020-12-31 - 2021-12-30 Fix 1 2",
            "2021-12-31 - 2022-12-30 Fix 1 3",
            "2022-12-31 - 2023-12-30 Fix 1 4");

        private static List<ContractRange> Contracts_21 => GetPersonContracts(
            "2019-09-01 - 2020-03-01 Freelance",
            "2020-03-02 - 2020-08-31 Freelance",
            "2020-09-01 - 2022-08-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService21 => GetPersonContractsWithYearOfService(
            "2019-09-01 - 2020-03-01 Freelance 1 1",
            "2020-03-02 - 2020-08-31 Freelance 1 1",
            "2020-09-01 - 2021-08-31 Freelance 2 2",
            "2021-09-01 - 2022-08-31 Freelance 3 3");

        private static List<ContractRange> Contracts_22 => GetPersonContracts(
            "2019-10-01 - 2019-12-30 Fix",
            "2020-01-01 - 2020-02-29 Freelance",
            "2020-03-01 - 2020-12-31 Freelance",
            "2021-01-01 - 2022-12-31 Freelance");
        private static List<ContractRange> ExpectedResultForBreakContractsByYearOfService22 => GetPersonContractsWithYearOfService(
            "2019-10-01 - 2019-12-30 Fix 0 1",
            "2020-01-01 - 2020-02-29 Freelance 1 1",
            "2020-03-01 - 2020-12-31 Freelance 1 2",
            "2021-01-01 - 2021-12-31 Freelance 2 3",
            "2022-01-01 - 2022-12-31 Freelance 3 4");
        #endregion

        static readonly object[] CasesForBreakContractsByYearOfService =
        {
            new object[] { Contracts_1, ExpectedResultForBreakContractsByYearOfService1 },
            new object[] { Contracts_2, ExpectedResultForBreakContractsByYearOfService2 },
            new object[] { Contracts_3, ExpectedResultForBreakContractsByYearOfService3 },
            new object[] { Contracts_4, ExpectedResultForBreakContractsByYearOfService4 },
            new object[] { Contracts_5, ExpectedResultForBreakContractsByYearOfService5 },
            new object[] { Contracts_6, ExpectedResultForBreakContractsByYearOfService6 },
            new object[] { Contracts_7, ExpectedResultForBreakContractsByYearOfService7 },
            new object[] { Contracts_8, ExpectedResultForBreakContractsByYearOfService8 },
            new object[] { Contracts_9, ExpectedResultForBreakContractsByYearOfService9 },
            new object[] { Contracts_10, ExpectedResultForBreakContractsByYearOfService10 },
            new object[] { Contracts_11, ExpectedResultForBreakContractsByYearOfService11 },
            new object[] { Contracts_12, ExpectedResultForBreakContractsByYearOfService12 },
            new object[] { Contracts_13, ExpectedResultForBreakContractsByYearOfService13 },
            new object[] { Contracts_14, ExpectedResultForBreakContractsByYearOfService14 },
            new object[] { Contracts_15, ExpectedResultForBreakContractsByYearOfService15 },
            new object[] { Contracts_16, ExpectedResultForBreakContractsByYearOfService16 },
            new object[] { Contracts_17, ExpectedResultForBreakContractsByYearOfService17 },
            new object[] { Contracts_18, ExpectedResultForBreakContractsByYearOfService18 },
            new object[] { Contracts_19, ExpectedResultForBreakContractsByYearOfService19 },
            new object[] { Contracts_20, ExpectedResultForBreakContractsByYearOfService20 },
            new object[] { Contracts_21, ExpectedResultForBreakContractsByYearOfService21 },
            new object[] { Contracts_22, ExpectedResultForBreakContractsByYearOfService22 },
        };

        [SetUp]
        public void Setup()
        {
        }

        [Test, TestCaseSource(nameof(CasesForBreakContractsByYearOfService))]
        public void BreakContractsByYearOfService(
            List<ContractRange> personContracts,
            List<ContractRange> expectedResult)
        {
        }



        static readonly object[] CasesForBreakContractsByYearOfService1 =
        {
            new List<string>{"s1" },
            new List<string>{"s2" }
        };

        [Test, TestCaseSource(nameof(CasesForBreakContractsByYearOfService1))]
        public void BreakContractsByYearOfService1(List<string> s)
        {
        }



        static readonly object[] CasesForBreakContractsByYearOfService2 =
        {
            "s1", "s2"
        };

        [Test, TestCaseSource(nameof(CasesForBreakContractsByYearOfService2))]
        public void BreakContractsByYearOfService2(string s)
        {
        }
    }
}