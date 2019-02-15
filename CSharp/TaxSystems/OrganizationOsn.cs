using BusinessLogic.Contracts.Common;
using System.Collections.Generic;
using static BusinessLogic.TaxSystem.Calculator.CalculatorHelper;

using ECompanyType = StorageServices.Contracts.Contacts.ECompanyType;

namespace BusinessLogic.TaxSystem.Calculator.TaxSystems
{
    public sealed class OrganizationOsnTaxParameters : OsnTaxParameters
    {
        public OrganizationOsnTaxParameters(
            Rate profitTaxRate,
            Rate ndsRate,
            TaxParametersBase baseParams)
            : base(ndsRate, baseParams)
        {
            ProfitTaxRate = profitTaxRate;
        }

        public override ECompanyType CompanyType => ECompanyType.OOO;

        /// <summary>
        /// процент налога на прибыль
        /// </summary>
        public Rate ProfitTaxRate { get; }
    }

    public static class OrgOsnTaxCalculator
    {
        public static OrgOsnTaxAggregate Calculate(
            TaxCalculationBase @base,
            OrganizationOsnTaxParameters @params)
        {
            var (salesNds, buyNds, totalNds) = CalculateNds(@params);

            var profitTax = CalculateProfitTax();

            var totalTax = RurMoney.Max(profitTax + totalNds, RurMoney.Zero);

            var (total, burgen) = CalculateTotal(totalTax, @base.TotalFee, @params);

            return new OrgOsnTaxAggregate(
                profitTax,
                totalNds,
                @base.TotalFee,
                @base.EmployeeFee,
                @base.EmployeeNdfl,
                total,
                burgen
                );

            RurMoney CalculateProfitTax()
            {
                var taxIncome = @params.CustomerTaxParameters.Income - salesNds;

                var taxExpense = CalculateTotalExpenseBase(@params, @base) - buyNds;

                return @params.ProfitTaxRate.Value * RurMoney.Max(taxIncome - taxExpense, RurMoney.Zero);
            }
        }
    }

    public sealed class OrgOsnTaxAggregate : TaxAggregate
    {
        public OrgOsnTaxAggregate(
            RurMoney profitTax,
            RurMoney nds,
            RurMoney feeTotal,
            EmployeeInsuranceFee employeeFee,
            RurMoney ndfl,
            RurMoney total,
            decimal burgen) 
            : base(feeTotal, employeeFee, ndfl, total, burgen)
        {
            ProfitTax = profitTax;
            VAT = nds;
        }

        /// <summary>
        /// налог на прибыль
        /// </summary>
        public RurMoney ProfitTax { get; }

        /// <summary>
        /// НДС
        /// </summary>
        public RurMoney VAT { get; }

        public override bool Equals(TaxAggregate other)
        {
            if (other is OrgOsnTaxAggregate oota)
            {
                return
                    ProfitTax == oota.ProfitTax &&
                    VAT == oota.VAT &&
                    base.Equals(oota);
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
            var hashCode = -665262469;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(ProfitTax);
            hashCode = hashCode * -1521134295 + EqualityComparer<RurMoney>.Default.GetHashCode(VAT);
            return hashCode;
        }
    }
}
