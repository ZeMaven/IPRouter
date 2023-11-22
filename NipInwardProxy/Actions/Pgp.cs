 
using Newtonsoft.Json;
using NipInwardProxy.Models;
using System;
using System.IO;
using System.ServiceModel.Dispatcher;
using System.Text;


namespace NipInwardProxy.Actions
{
   

    public class Pgp { 

        public PgpResponse Encrypt(string Value)
        {
            var Log = new Log();
            try
            {
                //Log.WriteLog("Pgp.Encrypt", $"Request:{Value}");
                var NibssPuk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NipInwardProxy.Properties.Settings.Default.NibssPubKey);
                var Enc = CoralPay.Cryptography.PGP.Actions.Data.Encrypt(new CoralPay.Cryptography.PGP.Models.EncryptionParam
                {
                    ToEncryptText = Value,
                    ExternalPublicKeyPath = NibssPuk
                    // ExternalPublicKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(Key)),
                }).Result;

                if (Enc.Header.ResponseCode == "01") throw new Exception(Enc.Header.ResponseMessage);

                var Resp = new PgpResponse
                {
                    ResponseCode = Enc.Header.ResponseCode,
                    ResponseMessage = Enc.Header.ResponseMessage,
                    Value = Enc.Encryption
                };

                var RespJson = JsonConvert.SerializeObject(Resp);
                //Log.WriteLog("Pgp.Encrypt", $"Response:{RespJson}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.WriteLog("Pgp.Encrypt", $"Err:{Ex}");
                return new PgpResponse() { ResponseCode = "01", ResponseMessage = Ex.Message, Value = null };
            }
        }


        public PgpResponse Decrypt(string Value)
        {
            var Log = new Log();
            try
            {

                //Log.WriteLog("Pgp.Decrypt", $"Request:{Value}");
                var Puk = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NipInwardProxy.Properties.Settings.Default.MyPubKeyPath);
                var Pri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NipInwardProxy.Properties.Settings.Default.MyPriKeyPath);
                var Dec = CoralPay.Cryptography.PGP.Actions.Data.Decrypt(new CoralPay.Cryptography.PGP.Models.DecryptionParam
                {
                    EncryptedData = Value,
                    InternalPrivateKey = Pri,
                    InternalPublicKey = Puk,
                    InternalKeyPassword = NipInwardProxy.Properties.Settings.Default.MyPgpPass
                }).Result;

                if (Dec.Header.ResponseCode == "01") throw new Exception(Dec.Header.ResponseMessage);

                var Resp = new PgpResponse
                {
                    ResponseCode = Dec.Header.ResponseCode,
                    ResponseMessage = Dec.Header.ResponseMessage,
                    Value = Dec.Decryption
                };

                var RespJson = JsonConvert.SerializeObject(Resp);
                //Log.WriteLog("Pgp.Decrypt", $"Response:{RespJson}");
                return Resp;
            }
            catch (Exception Ex)
            {
                Log.WriteLog("Pgp.Decrypt", $"Err:{Ex}");
                return new PgpResponse() { ResponseCode = "01", ResponseMessage = Ex.Message, Value = null };
            }
        }
    }
}
