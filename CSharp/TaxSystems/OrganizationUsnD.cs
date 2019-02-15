using BusinessLogic.Contracts.Common;
using System.Collections.Generic;
using static BusinessLogic.TaxSystem.Calculator.CalculatorHelper;

using ECompanyType = StorageServices.Contracts.Contacts.ECompanyType;

namespace BusinessLogic.TaxSystem.Calculator.TaxSystems
{
    public sealed class OrganizationUsnDTaxParameters : UsnTaxParameters
    {
        public OrganizationUsnDTaxParameters(
            Rate usnRate,
            TaxParametersBase baseParams)
            : base(usnRate, baseParams)
        {
        }

        public override ECompanyType CompanyType => ECompanyType.OOO;

        public override EUsnType UsnType => EUsnType.Income;
    }

    public static class OrgUsnDTaxCalculator
    {
        public static OrgUsnDTaxAggregate Calculate(
            TaxCalculationBase @base,
            OrganizationUsnDTaxParameters @params)
        {
            var usnChargedTax = CalculateUsnDChargedTax(@params);

            var feeDeduction = CalculateTaxDeduction(@params, @base, usnChargedTax);

            var taxTotal = usnChargedTax - feeDeduction;

            var (total, burgen) =
                CalculateTotal(taxTotal, @base.TotalFee, @params);

            return new OrgUsnDTaxAggregate(
                usnChargedTax,
                taxTotal,
                @base.EmployeeFee,
                feeDeduction,
                @base.TotalFee,
                @base.EmployeeNdfl,
                total,
                burgen
                );
        }
    }

    /// <summary>
    /// результат расчета калькулятора для ООО УСН Д
    /// </summary>
    public sealed class OrgUsnDTaxAggregate : TaxAggregate
    {
        public OrgUsnDTaxAggregate(
            RurMoney chargedTax,
            RurMoney sts,
            EmployeeInsuranceFee employeeFee,
            RurMoney feeDeduction,
            RurMoney feeTotal,
            RurMoney ndfl,
            RurMoney total,
            decimal burgen
            ) : base(feeTotal, employeeFee, ndfl, total, burgen)
        {
            STS = sts;
            AccruedSTS = chargedTax;
            InsuranceContributionsDeduction = feeDeduction;
        }

        public RurMoney STS { get; }

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
            if (other is OrgUsnDTaxAggregate oud)
            {
                return
                    base.Equals(oud) &&
                    STS == oud.STS &&
                    AccruedSTS == oud.AccruedSTS &&
                    InsuranceContributionsDeduction == oud.InsuranceContributionsDeduction;
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
            var hashCode = -848369603;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(STS);
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(AccruedSTS);
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(InsuranceContributionsDeduction);
            return hashCode;
        }
    }
}
