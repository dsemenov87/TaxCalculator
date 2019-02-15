using BusinessLogic.Contracts.Common;
using System.Collections.Generic;
using static BusinessLogic.TaxSystem.Calculator.CalculatorHelper;

using ECompanyType = StorageServices.Contracts.Contacts.ECompanyType;

namespace BusinessLogic.TaxSystem.Calculator.TaxSystems
{
    public sealed class OrganizationUsnDRTaxParameters : UsnTaxParameters
    {
        public OrganizationUsnDRTaxParameters(
            Rate minTaxRate,
            Rate usnRate,
            TaxParametersBase baseParams)
            : base(usnRate, baseParams)
        {
            MinTaxRate = minTaxRate;
        }

        public override ECompanyType CompanyType => ECompanyType.OOO;

        public override EUsnType UsnType => EUsnType.IncomeExpense;

        public Rate MinTaxRate { get; }
    }

    public static class OrgUsnDRTaxCalculator
    {
        public static OrgUsnDRTaxAggregate Calculate(
            TaxCalculationBase @base,
            OrganizationUsnDRTaxParameters @params)
        {
            var totalExpenses = CalculateTotalExpenseBase(@params, @base);

            var chargedTax = CalculateUsnDRChargedTax(@params, totalExpenses);

            var sts = RurMoney.Max(chargedTax, @params.MinTaxRate.Value * @params.CustomerTaxParameters.Income);

            var (total, burgen) = CalculateTotal(sts, @base.TotalFee, @params);

            return new OrgUsnDRTaxAggregate(
                totalExpenses,
                sts,
                @base.EmployeeFee,
                @base.EmployeeNdfl,
                @base.TotalFee,
                total,
                burgen
                );
        }
    }

    /// <summary>
    /// результат расчета калькулятора для ООО УСН Д-Р
    /// </summary>
    public sealed class OrgUsnDRTaxAggregate : TaxAggregate
    {
        public OrgUsnDRTaxAggregate(
            RurMoney taxExpenses,
            RurMoney sts,
            EmployeeInsuranceFee employeeFee,
            RurMoney ndfl,
            RurMoney feeTotal,
            RurMoney total,
            decimal burgen
            ) : base(feeTotal, employeeFee, ndfl, total, burgen)
        {
            TaxableExpenses = taxExpenses;
            STS = sts;
        }

        public RurMoney STS { get; }

        /// <summary>
        /// Расходы, принимаемые для целей налогообложения
        /// </summary>
        public RurMoney TaxableExpenses { get; }

        public override bool Equals(TaxAggregate other)
        {
            if (other is OrgUsnDRTaxAggregate oudr)
            {
                return
                    base.Equals(oudr) &&
                    STS == oudr.STS &&
                    TaxableExpenses == oudr.TaxableExpenses;
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
            var hashCode = 1470217011;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(STS);
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(TaxableExpenses);
            return hashCode;
        }
    }
}
