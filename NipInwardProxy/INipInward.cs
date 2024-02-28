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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface INipInward
    {

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        Task<string> nameenquirysingleitem(string request);
        [OperationContract]
        Task<string> fundtransfersingleitem_dd(string request);
        [OperationContract]

        Task<string> fundtransfersingleitem_dc(string request);
        [OperationContract]
        Task<string> txnstatusquerysingleitem(string request);
        [OperationContract]
        Task<string> balanceenquiry(string request);
        [OperationContract]
        Task<string> fundtransferAdvice_dc(string request);
        [OperationContract]

        Task<string> fundtransferAdvice_dd(string request);
        [OperationContract]

        Task<string> amountblock(string request);
        [OperationContract]
        Task<string> amountUnblock(string request);
        [OperationContract]


        Task<string> accountblock(string request);
        [OperationContract]


        Task<string> accountUnblock(string request);
        [OperationContract]



        Task<string> financialinstitutionlist(string request);
        [OperationContract]
        Task<string> mandateadvice(string request);
        [OperationContract]

        Task<string> ftackcreditrequest(string request);

        // TODO: Add your service operations here




    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
