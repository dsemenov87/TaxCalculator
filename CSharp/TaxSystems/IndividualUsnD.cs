using BusinessLogic.Contracts.Common;
using System.Collections.Generic;
using static BusinessLogic.TaxSystem.Calculator.CalculatorHelper;

namespace BusinessLogic.TaxSystem.Calculator.TaxSystems
{
    public sealed class IndividualUsnDTaxParameters : IndividualUsnTaxParameters
    {
        public IndividualUsnDTaxParameters(SelfInsuranceFee selfFee, Rate usnRate, TaxParametersBase @base)
            : base(selfFee, usnRate, @base)
        {
        }

        public override EUsnType UsnType => EUsnType.Income;
    }

    public static class IndividualUsnDTaxCalculator
    {
        public static IndividualUsnDTaxAggregate Calculate(
            TaxCalculationBase @base,
            IndividualUsnDTaxParameters @params)
        {
            var chargedTax = CalculateUsnDChargedTax(@params);

            var feeDeduction = CalculateTaxDeduction(@params, @base, chargedTax);

            var usnTotalTax = chargedTax - feeDeduction;

            var (total, burgen) = CalculateTotal(usnTotalTax, @base.TotalFee, @params);

            return new IndividualUsnDTaxAggregate(
                chargedTax,
                feeDeduction,
                @base.TotalFee,
                usnTotalTax,
                @params.SelfInsuranceFee,
                @base.EmployeeFee,
                @base.AdditionalFee,
                @base.EmployeeNdfl,
                total,
                burgen
                );
        }
    }

    /// <summary>
    /// результат расчета калькулятора для ИП УСН Д
    /// </summary>
    public sealed class IndividualUsnDTaxAggregate : IndividualUsnTaxAggregate
    {
        public IndividualUsnDTaxAggregate(
            RurMoney accruedSTS,
            RurMoney feeDeduction,
            RurMoney feeTotal,
            RurMoney sts,
            SelfInsuranceFee selfInsuranceFee,
            EmployeeInsuranceFee employeeFee,
            RurMoney additionalFee,
            RurMoney employeeNdfl,
            RurMoney total,
            decimal burgen
            ) : base(sts, selfInsuranceFee, additionalFee, feeTotal, employeeFee, employeeNdfl, total, burgen)
        {
            AccruedSTS = accruedSTS;
            InsuranceContributionsDeduction = feeDeduction;
        }

        /// <summary>
        /// Налога начислено
        /// </summary>
        public RurMoney AccruedSTS { get; }

        /// <summary>
        /// Вычет страховых взносов
        /// </summary>
        public RurMoney InsuranceContributionsDeduction { get; }

        public override bool Equals(TaxAggregate other)
        {
            if (other is IndividualUsnDTaxAggregate iud)
            {
                return
                    AccruedSTS == iud.AccruedSTS &&
                    InsuranceContributionsDeduction == iud.InsuranceContributionsDeduction &&
                    base.Equals(iud);
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
            var hashCode = -1930695353;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(AccruedSTS);
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(InsuranceContributionsDeduction);
            return hashCode;
        }
    }
}
