using System.Text;
using System.Xml.Serialization;
 
using System.Xml;

namespace NipOutwardProxy.Actions
{
    public interface IXmlConverter
    {
        object DeSerialize(string XmlString, object Obj);
        string Serialize(object Obj);
    }

    public class XmlConverter : IXmlConverter
    {


        public string Serialize(object Obj)
        {
            using MemoryStream memoryStream = new MemoryStream();
            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add("", "");
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream);
            XmlSerializer xmlSerializer = new XmlSerializer(Obj.GetType());
            xmlSerializer.Serialize(xmlWriter, Obj, xmlSerializerNamespaces);
            xmlWriter.Flush();
            memoryStream.Seek(0L, SeekOrigin.Begin);
            using StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8);
            return streamReader.ReadToEnd();
        }

        public object DeSerialize(string XmlString, object Obj)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(XmlString);
                XmlSerializer xmlSerializer = new XmlSerializer(Obj.GetType());
                return xmlSerializer.Deserialize(new StringReader(xmlDocument.InnerXml));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
