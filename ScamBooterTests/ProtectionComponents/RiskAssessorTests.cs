using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScamBooter.ProtectionComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace ScamBooter.ProtectionComponents.Tests
{
    [TestClass()]
    public class RiskAssessorTests
    {
        [TestMethod()]
        public void AddRemoteConnectionRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.addRemoteConnectionRisk();
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 50);
        }

        [TestMethod()]
        public void AddCommandPromptRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.AddRisk(RiskAssessor.EventRisk.COMMAND_PROMPT);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 20);
        }

        [TestMethod()]
        public void AddCommandScanRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.AddRisk(RiskAssessor.EventRisk.CMD_SCAN);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 20);
        }

        [TestMethod()]
        public void AddSuspiciousInputRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.AddRisk(RiskAssessor.EventRisk.SUSPICIOUS_KEYBOARD_INPUT);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 40);
        }

        [TestMethod()]
        public void AddEventViewerRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.AddRisk(RiskAssessor.EventRisk.EVENT_VIEWER);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 30);
        }

        [TestMethod()]
        public void AddSystemWindowRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.AddRisk(RiskAssessor.EventRisk.SYSTEM_WINDOW);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 20);
        }

        [TestMethod()]
        public void AddRunWindowRiskTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            riskAssessor.AddRisk(RiskAssessor.EventRisk.RUN_WINDOW);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 20);
        }

        [TestMethod()]
        public void AddIExplorerRiskTest()
        {
            Mock<GlobalInputDetection> mockGlobalInputDetection = new Mock<GlobalInputDetection>();
            RiskAssessor riskAssessor = new RiskAssessor(mockGlobalInputDetection.Object);
            riskAssessor.AddRisk(RiskAssessor.EventRisk.RUN_IEXPLORER);
            int riskScore = riskAssessor.calculateRiskScore();

            Assert.AreEqual(riskScore, 20);
        }

        [TestMethod()]
        public void FakeCMDScanTest()
        {
            RiskAssessor riskAssessor = NewRiskAssessor();
            bool reachedThreshold = false;
            int riskScore = 0;

            reachedThreshold = riskAssessor.addRemoteConnectionRisk();
            riskScore = riskAssessor.calculateRiskScore();
            Assert.IsFalse(reachedThreshold);
            Assert.AreEqual(riskScore, 50);


            reachedThreshold = riskAssessor.addAndAssessRisks(RiskAssessor.EventRisk.COMMAND_PROMPT);
            riskScore = riskAssessor.calculateRiskScore();
            Assert.IsFalse(reachedThreshold);
            Assert.AreEqual(riskScore, 70);

            reachedThreshold = riskAssessor.addAndAssessRisks(RiskAssessor.EventRisk.CMD_SCAN);
            riskScore = riskAssessor.calculateRiskScore();
            Assert.IsFalse(reachedThreshold);
            Assert.AreEqual(riskScore, 90);

            reachedThreshold = riskAssessor.addAndAssessRisks(RiskAssessor.EventRisk.SUSPICIOUS_KEYBOARD_INPUT);
            riskScore = riskAssessor.calculateRiskScore();
            Assert.IsTrue(reachedThreshold);
            Assert.AreEqual(riskScore, 130);
        }

        private static RiskAssessor NewRiskAssessor()
        {
            Mock<GlobalInputDetection> mockGlobalInputDetection = new Mock<GlobalInputDetection>();
            RiskAssessor riskAssessor = new RiskAssessor(mockGlobalInputDetection.Object);
            return riskAssessor;
        }

        [TestMethod()]
        public void GlobalHooks_SuspiciousInputTest()
        {
            Mock<GlobalInputDetection> mockGlobalInputDetection = new Mock<GlobalInputDetection>();
            Mock<GlobalInputDetection.SuspiciousInputArgs> mockSuspiciousInputArgs = new Mock<GlobalInputDetection.SuspiciousInputArgs>();
            RiskAssessor riskAssessor = new RiskAssessor(mockGlobalInputDetection.Object);
            
            SetupAndAssertSuspiciousInput(mockSuspiciousInputArgs, riskAssessor, "dir/s", RiskAssessor.EventRisk.CMD_SCAN);
            SetupAndAssertSuspiciousInput(mockSuspiciousInputArgs, riskAssessor, "tree", RiskAssessor.EventRisk.CMD_SCAN);
            SetupAndAssertSuspiciousInput(mockSuspiciousInputArgs, riskAssessor, "iexplorer", RiskAssessor.EventRisk.RUN_IEXPLORER);
            SetupAndAssertSuspiciousInput(mockSuspiciousInputArgs, riskAssessor, "virus", RiskAssessor.EventRisk.SUSPICIOUS_KEYBOARD_INPUT);
        }

        private static void SetupAndAssertSuspiciousInput(Mock<GlobalInputDetection.SuspiciousInputArgs> mockSuspiciousInputArgs, RiskAssessor riskAssessor, string mockArg, RiskAssessor.EventRisk eventRisk)
        {
            mockSuspiciousInputArgs.SetupGet(c => c.matcherFound).Returns(mockArg);
            riskAssessor.GlobalHooks_SuspiciousInput(null, mockSuspiciousInputArgs.Object);

            bool result = riskAssessor.GetDetectedRisks().Contains(eventRisk);
            Assert.IsTrue(result);
        }
    }
}