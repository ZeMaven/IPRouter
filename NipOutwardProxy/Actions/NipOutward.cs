using Momo.Common.Actions;
using Momo.Common.Models;

namespace NipOutwardProxy.Actions
{
    public interface INipOutward
    {
        Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request);
        Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request);
        Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request);
    }

    public class NipOutward : INipOutward
    {

        private readonly IConfiguration Config;
        private readonly ILog Log;
        private readonly IUtilities Utilities;
        private readonly IHttpService HttpService;
        private readonly IPgp Pgp;

        public NipOutward(IConfiguration config, ILog log, IUtilities utilities, IHttpService httpService, IPgp pgp)
        {
            Log = log;
            Config = config;
            Utilities = utilities;
            HttpService = httpService;
            Pgp = pgp;
        }



        public async Task<NameEnquiryPxResponse> NameEnquiry(NameEnquiryPxRequest Request)
        {
            return new NameEnquiryPxResponse { };

        }


        public async Task<TranQueryPxResponse> TransactionQuery(TranQueryPxRequest Request)
        {
            return new TranQueryPxResponse { };


        }




        public async Task<FundTransferPxResponse> Transfer(FundTransferPxRequest Request)
        {

            return new FundTransferPxResponse { };

        }


    }
}
