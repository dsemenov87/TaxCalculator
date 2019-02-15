using BusinessLogic.Contracts.Widget.Tax.Contracts;
using BusinessLogic.TaxSystem.Calculator.TaxSystems;
using StorageServices.Contracts.Taxes;
using System;

namespace BusinessLogic.TaxSystem.Calculator
{
    public sealed class TaxFbrInfo
    {
        /// <summary>
        /// Минимальная сумма к оплате от фбр
        /// </summary>
        public decimal? MinSum { get; set; }
    }

    public sealed class TaxCalculationResponse
    {
        public WidgetInfoOutContract WidgetInfo { get; set; }

        public FbrReportContract FbrReport { get; set; }

        public string TaxationSystem { get; set; }

        public bool IsLegalEntity { get; set; }

        /// <summary>
        /// Введёные в прошлый раз параметры
        /// </summary>
        public TaxCalculatorInputDataContract Input { get; set; }

        public TaxCalculationResponseDetails GTS { get; set; }

        public TaxCalculationResponseDetails STS15 { get; set; }

        public TaxCalculationResponseDetails STS6 { get; set; }
    }

    public sealed class TaxCalculationResponseDetails
    {
        public TaxCalculationResponseDetails(TaxAggregate aggregate)
        {
            PensionInsuranceEmployees = aggregate.EmployeeFee.PFR.Amount;
            HealthInsuranceEmployees = aggregate.EmployeeFee.FFOMS.Amount;
            SocialInsuranceEmployees = aggregate.EmployeeFee.FSS.Amount;
            InsuranceContributions = aggregate.InsuranceContributions.Amount;
            Total = aggregate.Total.Amount;
            TaxBurden = aggregate.TaxBurden <= 100 ? (decimal?)aggregate.TaxBurden : null;

            switch (aggregate)
            {
                case IndividualTaxAggregate ia:
                    PensionInsuranceOneself = ia.InsuranceOneself.Pension.Amount;
                    HealthInsuranceOneself = ia.InsuranceOneself.Health.Amount;

                    switch (ia)
                    {
                        case IndividualUsnTaxAggregate iua:
                            STS = iua.STS.Amount;

                            switch (iua)
                            {
                                case IndividualUsnDTaxAggregate iuda:
                                    AccruedSTS = iuda.AccruedSTS.Amount;
                                    InsuranceContributionsDeduction = iuda.InsuranceContributionsDeduction.Amount;
                                    AdditionalInsuranceFee = iuda.AdditionalInsuranceFee.Amount;
                                    break;

                                case IndividualUsnDRTaxAggregate iudra:
                                    TaxableExpenses = iudra.TaxableExpenses.Amount;
                                    AdditionalInsuranceFee = iudra.AdditionalInsuranceFee.Amount;
                                    break;

                                default:
                                    break;
                            }

                            break;

                        case IndividualOsnTaxAggregate ioa:
                            PIT = ioa.PIT.Amount;
                            VAT = ioa.VAT.Amount;
                            AdditionalInsuranceFee = ioa.AdditionalInsuranceFee.Amount;
                            break;

                        default:
                            break;
                    }

                    break;

                case OrgOsnTaxAggregate ooa:
                    VAT = ooa.VAT.Amount;
                    IncomeTax = ooa.ProfitTax.Amount;
                    break;

                case OrgUsnDTaxAggregate ouda:
                    InsuranceContributionsDeduction = ouda.InsuranceContributionsDeduction.Amount;
                    AccruedSTS = ouda.AccruedSTS.Amount;
                    STS = ouda.STS.Amount;
                    break;

                case OrgUsnDRTaxAggregate oudra:
                    TaxableExpenses = oudra.TaxableExpenses.Amount;
                    STS = oudra.STS.Amount;
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public decimal? PensionInsuranceOneself { get; }
        public decimal? HealthInsuranceOneself { get; }
        public decimal? STS { get; }
        public decimal? AdditionalInsuranceFee { get; }

        public decimal? PIT { get; }
        public decimal? VAT { get; }

        public decimal? AccruedSTS { get; }
        public decimal? InsuranceContributionsDeduction { get; }

        public decimal? TaxableExpenses { get; }

        public decimal? IncomeTax { get; }

        public decimal PensionInsuranceEmployees { get; }
        public decimal HealthInsuranceEmployees { get; }
        public decimal SocialInsuranceEmployees { get; }
        public decimal InsuranceContributions { get; }
        public decimal Total { get; }
        public decimal? TaxBurden { get; }
    }
}
