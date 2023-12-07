using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MomoSwitch.Actions;
using MomoSwitch.Models.Contracts.Specials.Router;

namespace MomoSwitch.Controllers.Inward.NibssExtras
{
    [Route("api/inward/[controller]")]
    [ApiController]
    public class FinancialIntitutionListController : ControllerBase
    {
        private readonly IInward Processor;
        public FinancialIntitutionListController(IInward processor)
        {
            Processor = processor;
        }

        [HttpPost(Name = "InFinancialIntitutionList")]
        public async Task<FinancialInstitutionListPxResponse> FinancialIntitutionList(FinancialInstitutionListPxRequest Req)
        {
            return await Processor.FinancialInstitutionList(Req);
        }
    }
}
