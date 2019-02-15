using BusinessLogic.Contracts.Common;
using System.Collections.Generic;
using static BusinessLogic.TaxSystem.Calculator.CalculatorHelper;

namespace BusinessLogic.TaxSystem.Calculator.TaxSystems
{
    public sealed class IndividualUsnDRTaxParameters : IndividualUsnTaxParameters
    {
        public IndividualUsnDRTaxParameters(
            Rate minTaxRate,
            SelfInsuranceFee selfInsuranceContributions,
            Rate usnRate,
            TaxParametersBase baseParams)
            : base(selfInsuranceContributions, usnRate, baseParams)
        {
            MinTaxRate = minTaxRate;
        }

        public Rate MinTaxRate { get; }

        public override EUsnType UsnType => EUsnType.IncomeExpense;
    }

    public static class IndividualUsnDRTaxCalculator
    {
        public static IndividualUsnDRTaxAggregate Calculate(
            TaxCalculationBase @base,
            IndividualUsnDRTaxParameters @params)
        {
            var totalExpenses = CalculateTotalExpenseBase(@params, @base) + @params.SelfInsuranceFee.Total;

            var chargedTax = CalculateUsnDRChargedTax(@params, totalExpenses);

            var taxTotal = RurMoney.Max(chargedTax, @params.MinTaxRate.Value * @params.CustomerTaxParameters.Income);

            var (total, burgen) = CalculateTotal(taxTotal, @base.TotalFee, @params);

            return new IndividualUsnDRTaxAggregate(
                totalExpenses,
                taxTotal,
                @params.SelfInsuranceFee,
                @base.EmployeeFee,
                @base.AdditionalFee,
                @base.EmployeeNdfl,
                @base.TotalFee,
                total,
                burgen
                );
        }
    }

    /// <summary>
    /// результат расчета калькулятора для ИП УСН Д-Р
    /// </summary>
    public sealed class IndividualUsnDRTaxAggregate : IndividualUsnTaxAggregate
    {
        public IndividualUsnDRTaxAggregate(
            RurMoney taxExpenses,
            RurMoney sts,
            SelfInsuranceFee selfInsuranceFee,
            EmployeeInsuranceFee employeeFee,
            RurMoney additionalFee,
            RurMoney employeeNdfl,
            RurMoney feeTotal,
            RurMoney total,
            decimal burgen
            ) : base(sts, selfInsuranceFee, additionalFee, feeTotal, employeeFee, employeeNdfl, total, burgen)
        {
            TaxableExpenses = taxExpenses;
        }

        /// <summary>
        /// Расходы, принимаемые для целей налогообложения
        /// </summary>
        public RurMoney TaxableExpenses { get; }
        
        public override bool Equals(TaxAggregate other)
        {
            if (other is IndividualUsnDRTaxAggregate iudr)
            {
                return
                    TaxableExpenses == iudr.TaxableExpenses &&
                    base.Equals(iudr);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TaxAggregate);
        }

        public override int GetHashCode()
        {
            var hashCode = 208538877;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(TaxableExpenses);
            return hashCode;
        }
    }
}
