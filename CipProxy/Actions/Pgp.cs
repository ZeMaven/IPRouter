using Momo.Common.Actions;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace CipProxy.Actions
{
    public interface IPgp
    {
        string Decryption(string EncText);
        string Ecryption(string PlainText);
    }

    public class Pgp : IPgp
    {


        private readonly ILog Log;
        private readonly IConfiguration Config;

        public Pgp(ILog log, IConfiguration configuration)
        {
            Log = log;
            Config = configuration;
        }

        public string Ecryption(string PlainText)
        {
            var PubKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.GetSection("CpukPath").Value);
            var KeyContent = new StreamReader(PubKeyPath).ReadToEnd();

            var Enc = CoralPay.Cryptography.Pgp.Invoke.Encrypt(new CoralPay.Cryptography.Pgp.Models.EncryptionParam
            {
                ToEncryptText = PlainText,
                ExternalPublicKeyStream = new MemoryStream(Encoding.ASCII.GetBytes(KeyContent))

                // ExternalPublicKeyPath = PubKeyPath,                       
                // ExternalPublicKeyStream = File.OpenRead(PubKeyPath)
                //ExternalPublicKeyStream = new MemoryStream(new System.Text.UTF8Encoding().GetBytes(KeyContent))

            }).Result;

            string Response = null;
            if (Enc.Header.ResponseCode == "00")
            {
                Response = Enc.Encryption;
            }
            else
            {
                Log.Write("Utilities.Encryption", $"Err {Enc.Header.ResponseMessage}");
                Response = null;
            }
            return Response;
        }




        public string Decryption(string EncText)
        {
            var InternalPublicKey = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.GetSection("MyPukPath").Value);
            var InternalPrivateKey = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config.GetSection("MyPriKPath").Value);
            var MyKeypass = Config.GetSection("MyKeypass").Value;

            var Dec = CoralPay.Cryptography.Pgp.Invoke.Decrypt(new CoralPay.Cryptography.Pgp.Models.DecryptionParam
            {
                EncryptedData = EncText,
                InternalKeyPassword = MyKeypass,
                InternalPublicKey = InternalPublicKey,
                InternalPrivateKey = InternalPrivateKey,

                //InternalPrivateKeyStream= File.OpenRead(InternalPublicKey),
                //InternalPublicKeyStream = File.OpenRead(InternalPrivateKey)
            }).Result;

            string Response = null;
            if (Dec.Header.ResponseCode == "00")
            {
                Response = Dec.Decryption;
            }
            else
            {
                Log.Write("Utilities.Decryption", $"Err {Dec.Header.ResponseMessage}");
                Response = null;
            }
            return Response;
        }






    }
}
