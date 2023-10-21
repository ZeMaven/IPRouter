using MomoSwitch.Models.Contracts.Momo;
using MomoSwitch.Models.Contracts.Proxy;
 

namespace MomoSwitch.Actions
{
    public interface ITransposer
    {
        FundTransferResponse ToMomoNameEnquiryResponse(FundTransferPxResponse proxyName);
        NameEnquiryResponse ToMomoNameEnquiryResponse(NameEnquiryPxResponse proxyName);
        TranQueryResponse ToMomoNameEnquiryResponse(TranQueryPxResponse proxyName);
        FundTransferPxRequest ToProxyNameEnquiryRequest(FundTransferRequest Request);
        NameEnquiryPxRequest ToProxyNameEnquiryRequest(NameEnquiryRequest Request);
        TranQueryPxRequest ToProxyNameEnquiryRequest(TranQueryRequest Request);
    }

    public class Transposer : ITransposer
    {

        public NameEnquiryPxRequest ToProxyNameEnquiryRequest(NameEnquiryRequest Request) => new NameEnquiryPxRequest
        {

        };


        public NameEnquiryResponse ToMomoNameEnquiryResponse(NameEnquiryPxResponse proxyName) => new NameEnquiryResponse
        {

        };



        public FundTransferPxRequest ToProxyNameEnquiryRequest(FundTransferRequest Request) => new FundTransferPxRequest
        {

        };


        public FundTransferResponse ToMomoNameEnquiryResponse(FundTransferPxResponse proxyName) => new FundTransferResponse
        {

        };


        public TranQueryPxRequest ToProxyNameEnquiryRequest(TranQueryRequest Request) => new TranQueryPxRequest
        {

        };
        public TranQueryResponse ToMomoNameEnquiryResponse(TranQueryPxResponse proxyName) => new TranQueryResponse
        {

        };
    }
}
