using BusinessLogic.Contracts.Common;
using BusinessLogic.Contracts.TaxSystem;
using BusinessLogic.TaxSystem.Calculator.TaxSystems;
using BusinessLogic.Widget.Tax.Extensions;
using OuterContactsService.Contracts;
using System;
using static BusinessLogic.TaxSystem.Calculator.CalculatorHelper;

using ECompanyType = StorageServices.Contracts.Contacts.ECompanyType;

namespace BusinessLogic.TaxSystem.Calculator
{
    public sealed class TaxCalculatorFactory
    {
        private readonly ITaxAmountFetcher _taxAmountFetcher;
        private readonly ITaxRateFetcher _taxRateFetcher;
        private readonly TaxDateTime _taxDateTime;

        public TaxCalculatorFactory(
            ITaxAmountFetcher taxAmountFetcher,
            ITaxRateFetcher taxRateFetcher,
            TaxDateTime taxDateTime)
        {
            _taxAmountFetcher = taxAmountFetcher;
            _taxRateFetcher = taxRateFetcher;
            _taxDateTime = taxDateTime;
        }

        public Func<CustomerTaxParameters, TaxAggregate> CreateCalculator(
            ECompanyType companyType,
            EAccountTaxationSystem taxSystem)
        {
            var year = _taxDateTime.ThisYear;

            var selfContribsLazy =
                new Lazy<SelfInsuranceFee>(() => new SelfInsuranceFee(
                    _taxAmountFetcher.FetchTaxAmount(ETaxAmount.PfrSelfAmount, year),
                    _taxAmountFetcher.FetchTaxAmount(ETaxAmount.FomsSelfAmount, year)
                    )
                );

            var usnDRateLazy = new Lazy<Rate>(() => _taxRateFetcher.FetchTaxRate(ETaxRate.UsnDRate, year));

            var usnDRRateLazy = new Lazy<Rate>(() => _taxRateFetcher.FetchTaxRate(ETaxRate.UsnDRRate, year));

            var ndsRateLazy = new Lazy<Rate>(() => _taxRateFetcher.FetchTaxRate(ETaxRate.NdsRate, year));

            var minTaxRateLazy = new Lazy<Rate>(() => _taxRateFetcher.FetchTaxRate(ETaxRate.MinTaxRate, year));

            var profitRateLazy = new Lazy<Rate>(() => _taxRateFetcher.FetchTaxRate(ETaxRate.ProfitRate, year));

            switch (companyType)
            {
                case ECompanyType.IP:
                    return CreateIndividualCalculator();

                case ECompanyType.OOO:
                    return CreateOrganizationCalculator();

                default:
                    throw new NotSupportedException();
            }

            TaxParametersBase BaseParams(CustomerTaxParameters customerTaxParameters)
            {
                return new TaxParametersBase(
                    customerTaxParameters,
                    new InsuranceFeeParameters(
                        _taxRateFetcher.FetchTaxRate(ETaxRate.PfrRate, year),
                        _taxRateFetcher.FetchTaxRate(ETaxRate.FomsRate, year),
                        _taxRateFetcher.FetchTaxRate(ETaxRate.FssRate, year),
                        _taxRateFetcher.FetchTaxRate(ETaxRate.AdditionalFeeRate, year),
                        _taxAmountFetcher.FetchTaxAmount(ETaxAmount.AdditionalFeeLimit, year)
                    ),
                    _taxRateFetcher.FetchTaxRate(ETaxRate.NdflRate, year));
            }

            TaxCalculationBase BaseComputations(TaxParameters @params, SelfInsuranceFee selfFee = default(SelfInsuranceFee))
            {
                var employeeFee = CalculateEmployeeFee(@params);
                var employeeNdfl = CalculateEmployeeNdfl(@params);
                var additionalFee = CalculateAdditionalInsuranceFee(@params);

                return new TaxCalculationBase(
                    employeeFee,
                    employeeNdfl,
                    additionalFee,
                    CalculateTotalFee(employeeFee, additionalFee, selfFee, @params)
                    );
            }

            Func<CustomerTaxParameters, TaxAggregate> CreateIndividualCalculator()
            {
                switch (taxSystem)
                {
                    case EAccountTaxationSystem.Usn6:
                        return (customerParams) => {
                            var @params = new IndividualUsnDTaxParameters(
                                selfContribsLazy.Value,
                                usnDRateLazy.Value,
                                BaseParams(customerParams)
                            );

                            return IndividualUsnDTaxCalculator.Calculate(
                                BaseComputations(@params, selfContribsLazy.Value),
                                @params);
                        };

                    case EAccountTaxationSystem.Usn15:
                        return (customerParams) =>
                        {
                            var @params = new IndividualUsnDRTaxParameters(
                                minTaxRateLazy.Value,
                                selfContribsLazy.Value,
                                usnDRRateLazy.Value,
                                BaseParams(customerParams)
                            );

                            return IndividualUsnDRTaxCalculator.Calculate(
                                BaseComputations(@params, selfContribsLazy.Value),
                                @params);
                        };

                    case EAccountTaxationSystem.Osn:
                        return (customerParams) =>
                        {
                            var @params = new IndividualOsnTaxParameters(
                                ndsRateLazy.Value,
                                selfContribsLazy.Value,
                                BaseParams(customerParams)
                            );

                            return IndividualOsnTaxCalculator.Calculate(
                                BaseComputations(@params, selfContribsLazy.Value),
                                @params);
                        };

                    default:
                        throw new NotSupportedException();
                }
            }

            Func<CustomerTaxParameters, TaxAggregate> CreateOrganizationCalculator()
            {
                switch (taxSystem)
                {
                    case EAccountTaxationSystem.Usn6:
                        return (customerParams) =>
                        {
                            var @params = new OrganizationUsnDTaxParameters(
                                usnDRateLazy.Value,
                                BaseParams(customerParams)
                            );

                            return OrgUsnDTaxCalculator.Calculate(
                                BaseComputations(@params),
                                @params);
                        };

                    case EAccountTaxationSystem.Usn15:
                        return (customerParams) => {
                            var @params = new OrganizationUsnDRTaxParameters(
                                minTaxRateLazy.Value,
                                usnDRRateLazy.Value,
                                BaseParams(customerParams)
                            );

                            return OrgUsnDRTaxCalculator.Calculate(
                                BaseComputations(@params),
                                @params);
                        };

                    case EAccountTaxationSystem.Osn:
                        return (customerParams) => {
                            var @params = new OrganizationOsnTaxParameters(
                                profitRateLazy.Value,
                                ndsRateLazy.Value,
                                BaseParams(customerParams)
                            );

                            return OrgOsnTaxCalculator.Calculate(
                                BaseComputations(@params),
                                @params);
                        };

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}
