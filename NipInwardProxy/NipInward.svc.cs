using NipInwardProxy.Actions;
using NipInwardProxy.Actions.Nibss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace NipInwardProxy
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class NipInward : INipInward
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }




        public async Task<string> nameenquirysingleitem(string request)
        {           
            return await new NameEnquiry().InvokeAsync(request);         
        }      
        public async Task<string> fundtransfersingleitem_dd(string request)
        {
            return await new DirectDebit().InvokeAsync(request);
        }

        public async Task<string> fundtransfersingleitem_dc(string request)
        {
            return await new DirectCredit().InvokeAsync(request);
        }

        public async Task<string> txnstatusquerysingleitem(string request)
        {
            return await new TransactionQuery().InvokeAsync(request);
        }

        public async Task<string> balanceenquiry(string request)
        {
            return await new BalanceEnquiry().InvokeAsync(request);
        }

        public async Task<string> fundtransferAdvice_dc(string request)
        {
            return await new DirectCreditAdvice().InvokeAsync(request);
        }

        public async Task<string> fundtransferAdvice_dd(string request)
        {
            return await new DirectDebitAdvice().InvokeAsync(request);
        }





        public async Task<string> amountblock(string request)
        {
            return await new AmountBlock().InvokeAsync(request);
        }

        public async Task<string> amountUnblock(string request)
        {
            return await new AmountUnblock().InvokeAsync(request);
        }

        public async Task<string> accountblock(string request)
        {
            return await new AccountBlock().InvokeAsync(request);
        }

        public async Task<string> accountUnblock(string request)
        {
            return await new AccountUnblock().InvokeAsync(request);
        }

        public async Task<string> financialinstitutionlist(string request)
        {
            return await new FinancialInstitutionList().InvokeAsync(request);
        }

        public async Task<string> mandateadvice(string request)
        {
            return await new MandateAdvice().InvokeAsync(request);
        }

        public async Task<string> ftackcreditrequest(string request)
        {
            return await new Callback().InvokeAsync(request);
        }
    }
}