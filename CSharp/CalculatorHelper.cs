﻿using BusinessLogic.Contracts.Common;
using StorageServices.Contracts.Contacts;
using System;

namespace BusinessLogic.TaxSystem.Calculator
{
    public static class CalculatorHelper
    {
        /// <summary>
        /// часть, соответствующая начисленному налогу по УСН.
        /// </summary>
        /// <param name="taxRate"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public static RurMoney CalculateUsnDChargedTax(TaxParameters @params)
        {
            if (@params is UsnTaxParameters idp && idp.UsnType == EUsnType.Income)
            {
                return idp.UsnRate.Value * @params.CustomerTaxParameters.Income;
            }

            throw new NotSupportedException();
        }

        public static RurMoney CalculateUsnDRChargedTax(TaxParameters @params, RurMoney expense)
        {
            if (@params is UsnTaxParameters idp && idp.UsnType == EUsnType.IncomeExpense)
            {
                return idp.UsnRate.Value * (@params.CustomerTaxParameters.Income - expense);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// часть, соответствующая НДС
        /// </summary>
        /// <param name="params"></param>
        /// <returns></returns>
        public static (RurMoney salesNds, RurMoney buyNds, RurMoney total) CalculateNds(
            TaxParameters @params)
        {
            switch (@params)
            {
                case OsnTaxParameters op:
                    var ndsKoeff = op.NdsRate.Value / (1M + op.NdsRate.Value);

                    var salesNds = ndsKoeff * @params.CustomerTaxParameters.Income;

                    var buyNds = ndsKoeff * @params.CustomerTaxParameters.Expense;

                    return (salesNds, buyNds, salesNds - buyNds);

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// часть, соответствующая дополнительным страховым взносам
        /// </summary>
        /// <param name="ifnsContribRate"></param>
        /// <param name="freeIncomeLimit"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public static RurMoney CalculateAdditionalInsuranceFee(TaxParameters @params)
        {
            switch (@params.CompanyType)
            {
                case ECompanyType.IP:
                    var bnd = @params.InsuranceFeeParams.AnnualFreeIncomeBoundary;
                    var contribDiff = @params.CustomerTaxParameters.Income - bnd;
                    var value =
                        contribDiff > RurMoney.Zero
                            ? @params.InsuranceFeeParams.AdditionalContribRate.Value * contribDiff
                            : RurMoney.Zero;

                    return value;

                default:
                    return RurMoney.Zero;
            }
        }

        /// <summary>
        /// часть, соответствующая страховым взносам сотрудников
        /// </summary>
        public static EmployeeInsuranceFee CalculateEmployeeFee(TaxParameters @params)
        {
            var salaryRate = new Rate(1M / (1M - @params.NdflRate.Value));

            var salary = salaryRate.Value * @params.CustomerTaxParameters.Salary;

            var pfr = @params.InsuranceFeeParams.PfrRate.Value * salary;

            var ffoms = @params.InsuranceFeeParams.FfomsRate.Value * salary;

            var fss = @params.InsuranceFeeParams.FssRate.Value * salary;

            return new EmployeeInsuranceFee(pfr, ffoms, fss);
        }

        public static RurMoney CalculateTotalFee(
            EmployeeInsuranceFee employeeFee,
            RurMoney additionalFee,
            SelfInsuranceFee selfFee,
            TaxParameters @params)
        {
            var tmp = employeeFee.Total + additionalFee;

            switch (@params.CompanyType)
            {
                case ECompanyType.IP:
                    return tmp + selfFee.Total;

                default:
                    return tmp;
            }
        }

        /// <summary>
        /// часть, соответствующая НДФЛ сотрудников
        /// </summary>
        /// <param name="params"></param>
        /// <returns></returns>
        public static RurMoney CalculateEmployeeNdfl(TaxParameters @params)
        {
            var salaryRate = new Rate(1M / (1M - @params.NdflRate.Value));

            var salary = salaryRate.Value * @params.CustomerTaxParameters.Salary;

            var ndfl = RurMoney.Round(@params.NdflRate.Value * salary);

            return ndfl;
        }

        public static (RurMoney total, decimal burgen) CalculateTotal(RurMoney totalTax, RurMoney totalFee, TaxParameters @params)
        {
            var total = RurMoney.Max(totalTax + totalFee, RurMoney.Zero);

            var income = @params.CustomerTaxParameters.Income;

            if (income > RurMoney.Zero)
            {
                var burgen = (total.Amount / income.Amount) * 100m;
                return (total, Math.Round(burgen * 100m) / 100m);
            }
            else
            {
                return (total, 0m);
            }
        }

        public static RurMoney CalculateTaxDeduction(
            TaxParameters @params,
            TaxCalculationBase @base,
            RurMoney chargedTax)
        {
            return
                @params.CustomerTaxParameters.Salary > RurMoney.Zero
                    ? RurMoney.Min(@base.TotalFee, 0.5M * chargedTax) // есть сотрудники
                    : RurMoney.Min(@base.TotalFee, chargedTax);
        }

        public static RurMoney CalculateTotalExpenseBase(TaxParameters @params, TaxCalculationBase @base)
        {
            return
                @base.EmployeeFee.Total +
                @params.CustomerTaxParameters.Expense +
                @params.CustomerTaxParameters.Salary +
                @base.AdditionalFee;
        }
    }
}
