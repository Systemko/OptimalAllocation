using System;
using System.Collections.Generic;

namespace OptimalAllocation
{
    public sealed class Allocation
    {
        private AllocationParameters _parameters;

        public Allocation(AllocationParameters allocationParameters)
        {
            validateAllocationParameters(allocationParameters);
            _parameters = allocationParameters;
        }

        public AllocationResults GetAllocation()
        {
            var data = _parameters.AllocationData;
            var intermidiateResults = new List<StepResult>();
            var totalFunc = _parameters.TotalFunc;
            StepResult previousStepResult = new StepResult();
            previousStepResult.F = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                previousStepResult = iterateStepForward(previousStepResult.F, data[i]);
                intermidiateResults.Add(previousStepResult);
            }

            var optimalIndexes = iterateBackward(intermidiateResults);
            var results = new AllocationResults();
            results.Allocation = new float[optimalIndexes.Length];
            for (int i = 0; i < optimalIndexes.Length; i++)
            {
                results.Allocation[i] = _parameters.AllocationResource[optimalIndexes[i]];
                results.Total += totalFunc(results.Allocation[i], _parameters.AllocationData[i][optimalIndexes[i]]);
            }
            return results;
        }

        public AllocationParameters Parameters
        {
            get { return _parameters; }
            set {
                validateAllocationParameters(value);
                _parameters = value;
            }
        }

        private StepResult iterateStepForward(float[] previous, float[] current)
        {
            int count = previous.Length + current.Length - 1;
            if (count > _parameters.ResourceCount)
            {
                count = _parameters.ResourceCount;
            }
            StepResult result = new StepResult(count);
            for (int i = 0; i < current.Length; i++)
            {
                for (int j = 0; j < previous.Length; j++)
                {
                    if (i + j < count)
                    {
                        float sum = current[i] + previous[j];
                        if (_parameters.CriterialSelect(result.F[i + j], sum))
                        {
                            result.F[i + j] = sum;
                            result.x[i + j] = i;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return result;
        }

        private int[] iterateBackward(List<StepResult> results)
        {
            var indexCount = _parameters.AllocationData.Length;
            var optimalIndexes = new int[indexCount];
            var lastForwardStep = results[results.Count - 1];
            var total = _parameters.ResourceCount - 1;

            optimalIndexes[indexCount - 1] = lastForwardStep.x[lastForwardStep.x.Length - 1];
            
            for (int i = indexCount - 2; i > 0; i--)
            {
                optimalIndexes[i] = results[i - 1].x[total - optimalIndexes[i + 1]];
                total -= optimalIndexes[i + 1];
            }
            optimalIndexes[0] = total - optimalIndexes[1];
            return optimalIndexes;
        }

        private void validateAllocationParameters(AllocationParameters parameters)
        {
            if (parameters == null) throw new ArgumentException("Allocation parameters can not be null");
        }
    }

    public sealed class AllocationParameters 
    {
        private readonly int _resourceCount;
        private readonly float[] _allocationResource;
        private readonly float[][] _allocationData;
        private readonly Func<float, float, float> _totalFunc;
        private readonly Func<float, float, bool> _criterialSelect;

        public AllocationParameters(float[] allocationResource, 
                                    float[][] allocationData,
                                    CriteriaType criteriaType,
                                    Func<float, float, float> totalFunc = null,
                                    bool isLeadingZerosIncluded = false)
        {         
            if (isLeadingZerosIncluded)
            {
                _allocationResource = allocationResource;
                _allocationData = allocationData;
            }
            else
            {
                _allocationResource = addLeadingZeros(allocationResource);
                _allocationData = addLeadingZerosToData(allocationData);
            }
            int dataCount = 0;
            _resourceCount = _allocationResource.Length;
            for (int i = 0; i < _allocationData.Length; i++)
            {
                dataCount += _allocationData[i].Length-1;
            }            
            if (dataCount < _resourceCount - 1)
            {
                _resourceCount = dataCount + 1;
            }            
            _totalFunc = totalFunc;
            if (_totalFunc == null)
            {
                _totalFunc = (curResource, curData) => curData;
            }
            switch (criteriaType)
            {
                case CriteriaType.Min:
                    _criterialSelect = (oldOne, newOne) =>
                    {
                        if (oldOne > newOne || oldOne == 0) return true;
                        return false;
                    };
                    break;
                case CriteriaType.Max:
                    _criterialSelect = (oldOne, newOne) =>
                    {
                        if (oldOne < newOne) return true;
                        return false;
                    };
                    break;
            }
        }

        public int ResourceCount
        {
            get { return _resourceCount; }
        }

        public float[] AllocationResource
        {
            get { return _allocationResource; }
        }

        public float[][] AllocationData
        {
            get { return _allocationData; }
        }

        public Func<float, float, bool> CriterialSelect
        {
            get { return _criterialSelect; }
        }

        public Func<float, float, float> TotalFunc
        {
            get { return _totalFunc; }
        }

        private float[] addLeadingZeros(float[] data)
        {
            float[] dataWithZeros = new float[data.Length + 1];
            Array.ConstrainedCopy(data, 0, dataWithZeros, 1, data.Length);
            return dataWithZeros;
        }

        private float[][] addLeadingZerosToData(float[][] allocationData)
        {
            float[][] allocationDataWithZeros = new float[allocationData.Length][]; ;
            for (int i = 0; i < allocationData.Length; i++)
            {
                allocationDataWithZeros[i] = addLeadingZeros(allocationData[i]);
            }
            return allocationDataWithZeros;
        }
    }

    public struct StepResult
    {
        public float[] F;
        public int[] x;

        public StepResult(int count)
        {
            F = new float[count];
            x = new int[count];
        }
    }

    public struct AllocationResults
    {
        public float[] Allocation;
        public float Total;
    }

    public enum CriteriaType
    {
        Min,
        Max
    }
}
