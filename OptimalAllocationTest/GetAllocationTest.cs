using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OptimalAllocation;

namespace OptimalAllocationTest
{
    [TestClass]
    public class GetAllocationTest
    {
        [TestMethod]
        public void GetAllocation_MaxCriteria_1_Ok()
        {
            float[] expectedResult = new float[] { 200, 200, 100, 200 };

            float[] resource = new float[] { 100, 200, 300, 400, 500, 600, 700 };
            float[] f1 = new float[] { 42, 58, 71, 80, 89, 95, 100 };
            float[] f2 = new float[] { 30, 49, 63, 68, 69, 65, 60 };
            float[] f3 = new float[] { 22, 37, 49, 59, 68, 76, 82 };
            float[] f4 = new float[] { 50, 68, 82, 92, 100, 107, 112 };
            float[][] data = new float[][] { f1, f2, f3, f4 };

            AllocationParameters parameters = new AllocationParameters(resource, data, CriteriaType.Max);
            testAllocationResults(parameters, expectedResult, 197F);
        }

        [TestMethod]
        public void GetAllocation_MaxCriteria_2_Ok()
        {
            float[] expectedResult = new float[] { 0, 1, 4 };
            float[] resource = new float[] { 1, 2, 3, 4, 5 };
            float[] f1 = new float[] { 3.22F, 3.57F, 4.12F, 4F, 4.85F };
            float[] f2 = new float[] { 3.33F, 4.87F, 5.26F, 7.34F, 9.49F };
            float[] f3 = new float[] { 4.27F, 7.64F, 10.25F, 15.93F, 16.12F };
            float[][] data = new float[][] { f1, f2, f3 };

            AllocationParameters parameters = new AllocationParameters(resource, data, CriteriaType.Max);
            testAllocationResults(parameters, expectedResult, 19.26F);
        }

        [TestMethod]
        public void GetAllocation_MinCriteria_1_Ok()
        {
            float[] expectedResult = new float[] { 7, 0, 1, 5 };

            float[] resource = Enumerable.Range(1, 13).Select(x => (float) x).ToArray(); 
            float[] f1 = new float[] { 1.08F, 2.04F, 3F, 4F, 5F, 5.82F, 6.79F };
            float[] f2 = new float[] { 1.04F, 2.02F };
            float[] f3 = new float[] { 1.03F, 2.02F, 3F, 4F, 5F, 6F, 7F };
            float[] f4 = new float[] { 1.01F, 2F, 3F, 4F, 4.9F, 6F, 7F };
            float[][] data = new float[][] { f1, f2, f3, f4 };

            AllocationParameters parameters = new AllocationParameters(resource, data, CriteriaType.Min);
            testAllocationResults(parameters, expectedResult, 12.72F);
        }

        private void testAllocationResults(AllocationParameters parameters, float[] expectedResult, float expectedTotal)
        {
            Allocation allocation = new Allocation(parameters);
            AllocationResults result = allocation.GetAllocation();
            for (int i = 0; i < expectedResult.Length; i++)
            {
                Assert.IsTrue(expectedResult[i] == result.Allocation[i]);
            }
            Assert.AreEqual(Math.Round(expectedTotal,2), Math.Round(result.Total, 2));
        }
    }
}
