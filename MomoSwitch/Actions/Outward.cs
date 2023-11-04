using Momo.Common.Models;
using MomoSwitch.Models.Contracts.Momo;

namespace MomoSwitch.Actions
{
    public interface IOutward
    {
        Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest Req);
        Task<TranQueryResponse> TranQuery(TranQueryRequest Req);
        Task<FundTransferResponse> Transfer(FundTransferRequest Req);
    }

    public class Outward : IOutward
    {
        private readonly ISwitchRouter SwitchRouter;
        private readonly ITransposer Transposer;
        private readonly ILog Log;
        public Outward(ISwitchRouter router, ILog log, ITransposer transposer)
        {
            SwitchRouter = router;
            Transposer = transposer;
            Log = log;
        }


        public async Task<NameEnquiryResponse> NameEnquiry(NameEnquiryRequest Req)
        {
            try
            {  //Review
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Amount = 0,
                    BankCode = Req.destinationInstitutionCode,
                    Date = DateTime.Now,
                });

                var ProcessorRequest = Transposer.ToProxyNameEnquiryRequest(Req);



            }
            catch (Exception Ex)
            {

            }
        }


        public async Task<TranQueryResponse> TranQuery(TranQueryRequest Req)
        {
            try
            {
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Processor = ""
                });              
                var ProcessorRequest = Transposer.ToProxyTranQueryyRequest(Req);


            }
            catch (Exception Ex)
            {

            }
        }


        public async Task<FundTransferResponse> Transfer(FundTransferRequest Req)
        {
            try
            {          
                var RouterDetail = SwitchRouter.Route(new Models.RouterRequest
                {
                    Amount = Req.amount,
                    BankCode = Req.destinationInstitutionCode,
                    Date = DateTime.Now,
                });
                var ProcessorRequest = Transposer.ToProxyFundTransferRequest(Req);


            }
            catch (Exception Ex)
            {

            }
        }
    }
}